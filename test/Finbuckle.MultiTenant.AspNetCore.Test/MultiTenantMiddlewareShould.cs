// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.AspNetCore.Options;
using Finbuckle.MultiTenant.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using IEndpointFeature = Microsoft.AspNetCore.Http.Features.IEndpointFeature;
using MsOptions = Microsoft.Extensions.Options.Options;

namespace Finbuckle.MultiTenant.AspNetCore.Test;

public class MultiTenantMiddlewareShould
{
    private static MultiTenantMiddleware CreateMiddleware(RequestDelegate next,
        BypassWhenOptions? bypassOptions = null,
        ShortCircuitWhenOptions? shortCircuitOptions = null)
        => new(next,
            MsOptions.Create(bypassOptions ?? new BypassWhenOptions()),
            MsOptions.Create(shortCircuitOptions ?? new ShortCircuitWhenOptions()));

    private static Task InvokeMiddleware(MultiTenantMiddleware mw, HttpContext context, IServiceProvider sp) =>
        mw.Invoke(context,
            sp.GetRequiredService<ITenantContext>(),
            sp.GetRequiredService<ITenantResolver>(),
            sp.GetRequiredService<ITenantScopeProvider>());

    [Fact]
    public async Task ResolveTenantContextIfTenantFound()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>().WithStaticStrategy("initech").WithInMemoryStore();
        var sp = services.BuildServiceProvider();
        var store = sp.GetRequiredService<IMultiTenantStore<TenantInfo>>();
        await store.AddAsync(new TenantInfo { Id = "initech", Identifier = "initech" });

        var context = new Mock<HttpContext>();
        context.Setup(c => c.RequestServices).Returns(sp);

        var itemsDict = new Dictionary<object, object?>();
        context.Setup(c => c.Items).Returns(itemsDict);
        context.Setup(c => c.Features).Returns(new FeatureCollection());

        ITenantInfo? observedTenant = null;
        var mw = CreateMiddleware(_ =>
        {
            observedTenant = sp.GetRequiredService<ITenantContext<TenantInfo>>().TenantInfo;
            return Task.CompletedTask;
        });

        await InvokeMiddleware(mw, context.Object, sp);

        Assert.NotNull(observedTenant);
        Assert.Equal("initech", observedTenant.Id);
    }

    [Fact]
    public async Task NotShortCircuitIfTenantFound()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>().WithStaticStrategy("initech").WithInMemoryStore();
        var sp = services.BuildServiceProvider();
        var store = sp.GetRequiredService<IMultiTenantStore<TenantInfo>>();
        await store.AddAsync(new TenantInfo { Id = "initech", Identifier = "initech" });

        var context = new Mock<HttpContext>();
        context.Setup(c => c.RequestServices).Returns(sp);
        context.Setup(c => c.Features).Returns(new FeatureCollection());
        var response = new Mock<HttpResponse>();
        context.Setup(c => c.Response).Returns(response.Object);

        var itemsDict = new Dictionary<object, object?>();
        context.Setup(c => c.Items).Returns(itemsDict);

        var calledNext = false;
        var resolvedInPredicate = false;
        ITenantInfo? observedTenant = null;
        var mw = CreateMiddleware(
            _ =>
            {
                calledNext = true;
                observedTenant = sp.GetRequiredService<ITenantContext<TenantInfo>>().TenantInfo;
                return Task.CompletedTask;
            },
            shortCircuitOptions: new ShortCircuitWhenOptions
            {
                Predicate = mtc =>
                {
                    resolvedInPredicate = mtc.IsResolved;
                    return !mtc.IsResolved;
                }
            });

        await InvokeMiddleware(mw, context.Object, sp);

        Assert.True(resolvedInPredicate);
        Assert.NotNull(observedTenant);
        Assert.Equal("initech", observedTenant.Id);
        Assert.True(calledNext);
        response.Verify(r => r.Redirect("/tenant/notfound"), Times.Never);
    }

    [Fact]
    public async Task SetTenantContext()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>().WithStaticStrategy("initech").WithInMemoryStore();
        var sp = services.BuildServiceProvider();
        var store = sp.GetRequiredService<IMultiTenantStore<TenantInfo>>();
        await store.AddAsync(new TenantInfo { Id = "initech", Identifier = "initech" });

        var context = new Mock<HttpContext>();
        context.Setup(c => c.RequestServices).Returns(sp);
        context.Setup(c => c.Features).Returns(new FeatureCollection());

        var itemsDict = new Dictionary<object, object?>();
        context.Setup(c => c.Items).Returns(itemsDict);

        var resolved = false;
        TenantInfo? observedTenant = null;
        var mw = CreateMiddleware(_ =>
        {
            var tenantContext = sp.GetRequiredService<ITenantContext<TenantInfo>>();
            resolved = tenantContext.IsResolved;
            observedTenant = tenantContext.TenantInfo;
            return Task.CompletedTask;
        });

        await InvokeMiddleware(mw, context.Object, sp);

        Assert.True(resolved);
        Assert.NotNull(observedTenant);
    }

    [Fact]
    public async Task NotResolveTenantIfNoTenantFound()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>().WithStaticStrategy("not_initech").WithInMemoryStore();
        var sp = services.BuildServiceProvider();
        var store = sp.GetRequiredService<IMultiTenantStore<TenantInfo>>();
        await store.AddAsync(new TenantInfo { Id = "initech", Identifier = "initech" });

        var context = new Mock<HttpContext>();
        context.Setup(c => c.RequestServices).Returns(sp);
        context.Setup(c => c.Features).Returns(new FeatureCollection());

        var itemsDict = new Dictionary<object, object?>();
        context.Setup(c => c.Items).Returns(itemsDict);

        var resolved = true;
        TenantInfo? observedTenant = null;
        var mw = CreateMiddleware(_ =>
        {
            var tenantContext = sp.GetRequiredService<ITenantContext<TenantInfo>>();
            resolved = tenantContext.IsResolved;
            observedTenant = tenantContext.TenantInfo;
            return Task.CompletedTask;
        });

        await InvokeMiddleware(mw, context.Object, sp);

        Assert.False(resolved);
        Assert.Null(observedTenant);
    }

    [Fact]
    public async Task ShortCircuitIfNoTenant()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>().WithStaticStrategy("not_initech").WithInMemoryStore();
        var sp = services.BuildServiceProvider();
        var store = sp.GetRequiredService<IMultiTenantStore<TenantInfo>>();
        await store.AddAsync(new TenantInfo { Id = "initech", Identifier = "initech" });

        var context = new Mock<HttpContext>();
        context.Setup(c => c.RequestServices).Returns(sp);
        context.Setup(c => c.Features).Returns(new FeatureCollection());
        var response = new Mock<HttpResponse>();
        context.Setup(c => c.Response).Returns(response.Object);

        var itemsDict = new Dictionary<object, object?>();
        context.Setup(c => c.Items).Returns(itemsDict);

        var calledNext = false;
        bool? resolvedInPredicate = null;
        ITenantInfo? observedTenant = null;
        var mw = CreateMiddleware(
            _ =>
            {
                calledNext = true;
                return Task.CompletedTask;
            },
            shortCircuitOptions: new ShortCircuitWhenOptions
            {
                Predicate = mtc =>
                {
                    resolvedInPredicate = mtc.IsResolved;
                    observedTenant = mtc.TenantInfo;
                    return !mtc.IsResolved;
                }
            });

        await InvokeMiddleware(mw, context.Object, sp);

        Assert.False(resolvedInPredicate);
        Assert.Null(observedTenant);
        Assert.False(calledNext);
        response.Verify(r => r.Redirect("/tenant/notfound"), Times.Never);
    }

    [Fact]
    public async Task ShortCircuitAndRedirectIfNoTenant()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>().WithStaticStrategy("not_initech").WithInMemoryStore();
        var sp = services.BuildServiceProvider();
        var store = sp.GetRequiredService<IMultiTenantStore<TenantInfo>>();
        await store.AddAsync(new TenantInfo { Id = "initech", Identifier = "initech" });

        var context = new Mock<HttpContext>();
        context.Setup(c => c.RequestServices).Returns(sp);
        context.Setup(c => c.Features).Returns(new FeatureCollection());
        var response = new Mock<HttpResponse>();
        context.Setup(c => c.Response).Returns(response.Object);

        var itemsDict = new Dictionary<object, object?>();
        context.Setup(c => c.Items).Returns(itemsDict);

        var calledNext = false;
        bool? resolvedInPredicate = null;
        ITenantInfo? observedTenant = null;
        var mw = CreateMiddleware(
            _ =>
            {
                calledNext = true;
                return Task.CompletedTask;
            },
            shortCircuitOptions: new ShortCircuitWhenOptions
            {
                Predicate = mtc =>
                {
                    resolvedInPredicate = mtc.IsResolved;
                    observedTenant = mtc.TenantInfo;
                    return !mtc.IsResolved;
                },
                RedirectTo = new Uri("/tenant/notfound", UriKind.Relative)
            });

        await InvokeMiddleware(mw, context.Object, sp);

        Assert.False(resolvedInPredicate);
        Assert.Null(observedTenant);
        Assert.False(calledNext);
        response.Verify(r => r.Redirect("/tenant/notfound"), Times.Once);
    }

    [Fact]
    public async Task BypassResolutionWhenNoEndpointAndOptionEnabled()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>().WithStaticStrategy("initech").WithInMemoryStore();
        var sp = services.BuildServiceProvider();
        var store = sp.GetRequiredService<IMultiTenantStore<TenantInfo>>();
        await store.AddAsync(new TenantInfo { Id = "initech", Identifier = "initech" });

        var context = new Mock<HttpContext>();
        context.Setup(c => c.RequestServices).Returns(sp);
        // Empty feature collection means GetEndpoint() returns null (no route matched).
        context.Setup(c => c.Features).Returns(new FeatureCollection());

        var itemsDict = new Dictionary<object, object?>();
        context.Setup(c => c.Items).Returns(itemsDict);

        var calledNext = false;
        var mw = CreateMiddleware(
            _ =>
            {
                calledNext = true;
                return Task.CompletedTask;
            },
            bypassOptions: new BypassWhenOptions { Predicate = ctx => ctx.GetEndpoint() is null });

        await InvokeMiddleware(mw, context.Object, sp);

        // next should have been called, but resolution should have been bypassed
        Assert.True(calledNext);
    }

    [Fact]
    public async Task DoesNotBypassResolutionWhenEndpointExistsAndOptionEnabled()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>().WithStaticStrategy("initech").WithInMemoryStore();
        var sp = services.BuildServiceProvider();
        var store = sp.GetRequiredService<IMultiTenantStore<TenantInfo>>();
        await store.AddAsync(new TenantInfo { Id = "initech", Identifier = "initech" });

        // Set up an endpoint on the feature collection so GetEndpoint() returns non-null.
        var features = new FeatureCollection();
        var endpointFeature = new Mock<IEndpointFeature>();
        endpointFeature.Setup(e => e.Endpoint)
            .Returns(new Endpoint(null, EndpointMetadataCollection.Empty, "test"));
        features.Set(endpointFeature.Object);

        var context = new Mock<HttpContext>();
        context.Setup(c => c.RequestServices).Returns(sp);
        context.Setup(c => c.Features).Returns(features);

        var itemsDict = new Dictionary<object, object?>();
        context.Setup(c => c.Items).Returns(itemsDict);

        var calledNext = false;
        var mw = CreateMiddleware(
            _ =>
            {
                calledNext = true;
                return Task.CompletedTask;
            },
            bypassOptions: new BypassWhenOptions { Predicate = ctx => ctx.GetEndpoint() is null });

        await InvokeMiddleware(mw, context.Object, sp);

        // Endpoint exists, so resolution should proceed normally.
        Assert.True(calledNext);
    }

    [Fact]
    public async Task DoesNotBypassResolutionWhenNoEndpointAndOptionDisabled()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>().WithStaticStrategy("initech").WithInMemoryStore();
        var sp = services.BuildServiceProvider();
        var store = sp.GetRequiredService<IMultiTenantStore<TenantInfo>>();
        await store.AddAsync(new TenantInfo { Id = "initech", Identifier = "initech" });

        var context = new Mock<HttpContext>();
        context.Setup(c => c.RequestServices).Returns(sp);
        context.Setup(c => c.Features).Returns(new FeatureCollection());

        var itemsDict = new Dictionary<object, object?>();
        context.Setup(c => c.Items).Returns(itemsDict);

        var calledNext = false;
        TenantInfo? observedTenant = null;
        var mw = CreateMiddleware(
            _ =>
            {
                calledNext = true;
                observedTenant = sp.GetRequiredService<ITenantContext<TenantInfo>>().TenantInfo;
                return Task.CompletedTask;
            });
        // No bypassOptions = null predicate = no bypass applied

        await InvokeMiddleware(mw, context.Object, sp);

        // No predicate set, so resolution should proceed as normal even with no endpoint.
        Assert.True(calledNext);
        Assert.NotNull(observedTenant);
        Assert.Equal("initech", observedTenant.Id);
    }

    [Fact]
    public async Task BeginsFreshScopeForEachInvocation()
    {
        var currentIdentifier = "tenant-1";
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>()
            .WithDelegateStrategy(_ => Task.FromResult<string?>(currentIdentifier))
            .WithInMemoryStore();
        var sp = services.BuildServiceProvider();
        var tenantManager = sp.GetRequiredService<TenantManager<TenantInfo>>();
        await tenantManager.AddAsync(new TenantInfo { Id = "tenant-1", Identifier = "tenant-1" });
        await tenantManager.AddAsync(new TenantInfo { Id = "tenant-2", Identifier = "tenant-2" });
        var context = new Mock<HttpContext>();
        context.Setup(c => c.RequestServices).Returns(sp);
        context.Setup(c => c.Features).Returns(new FeatureCollection());

        var observations = new List<string?>();
        var mw = CreateMiddleware(_ =>
        {
            observations.Add(sp.GetRequiredService<ITenantContext<TenantInfo>>().TenantInfo?.Identifier);
            return Task.CompletedTask;
        });

        await InvokeMiddleware(mw, context.Object, sp);
        currentIdentifier = "tenant-2";
        await InvokeMiddleware(mw, context.Object, sp);

        Assert.Equal(new[] { "tenant-1", "tenant-2" }, observations);
    }
}
