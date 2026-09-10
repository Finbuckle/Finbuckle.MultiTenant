// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.AspNetCore.Options;
using Finbuckle.MultiTenant.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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

    [Fact]
    public async Task SetHttpContextItemIfTenantFound()
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

        var mw = CreateMiddleware(_ => Task.CompletedTask);

        await mw.Invoke(context.Object);

        var mtc = (IMultiTenantContext<TenantInfo>?)context.Object.Items[typeof(IMultiTenantContext)];

        Assert.NotNull(mtc?.TenantInfo);
        Assert.Equal("initech", mtc.TenantInfo.Id);
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
        var mw = CreateMiddleware(
            _ =>
            {
                calledNext = true;
                return Task.CompletedTask;
            },
            shortCircuitOptions: new ShortCircuitWhenOptions { Predicate = mtc => !mtc.IsResolved });

        await mw.Invoke(context.Object);

        var mtc = (IMultiTenantContext<TenantInfo>?)context.Object.Items[typeof(IMultiTenantContext)];

        Assert.NotNull(mtc?.TenantInfo);
        Assert.Equal("initech", mtc.TenantInfo.Id);
        Assert.True(calledNext);
        response.Verify(r => r.Redirect("/tenant/notfound"), Times.Never);
    }

    [Fact]
    public async Task SetTenantAccessor()
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

        IMultiTenantContext<TenantInfo>? mtc = null;

        var mw = CreateMiddleware(_ =>
        {
            // have to check in this Async chain...
            var accessor = context.Object.RequestServices.GetRequiredService<IMultiTenantContextAccessor<TenantInfo>>();
            mtc = accessor.MultiTenantContext;

            return Task.CompletedTask;
        });

        await mw.Invoke(context.Object);

        Assert.NotNull(mtc);
        Assert.True(mtc.IsResolved);
        Assert.NotNull(mtc.TenantInfo);
    }

    [Fact]
    public async Task NotSetTenantAccessorIfNoTenant()
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

        IMultiTenantContext<TenantInfo>? mtc = null;

        var mw = CreateMiddleware(_ =>
        {
            // have to check in this Async chain...
            var accessor = context.Object.RequestServices.GetRequiredService<IMultiTenantContextAccessor<TenantInfo>>();
            mtc = accessor.MultiTenantContext;

            return Task.CompletedTask;
        });

        await mw.Invoke(context.Object);

        Assert.NotNull(mtc);
        Assert.False(mtc.IsResolved);
        Assert.Null(mtc.TenantInfo);
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
        var mw = CreateMiddleware(
            _ =>
            {
                calledNext = true;
                return Task.CompletedTask;
            },
            shortCircuitOptions: new ShortCircuitWhenOptions { Predicate = mtc => !mtc.IsResolved });

        await mw.Invoke(context.Object);

        var mtc = (IMultiTenantContext<TenantInfo>?)context.Object.Items[typeof(IMultiTenantContext)];

        Assert.NotNull(mtc);
        Assert.False(mtc.IsResolved);
        Assert.Null(mtc.TenantInfo);
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
        var mw = CreateMiddleware(
            _ =>
            {
                calledNext = true;
                return Task.CompletedTask;
            },
            shortCircuitOptions: new ShortCircuitWhenOptions
            {
                Predicate = mtc => !mtc.IsResolved,
                RedirectTo = new Uri("/tenant/notfound", UriKind.Relative)
            });

        await mw.Invoke(context.Object);

        var mtc = (IMultiTenantContext<TenantInfo>?)context.Object.Items[typeof(IMultiTenantContext)];

        Assert.NotNull(mtc);
        Assert.False(mtc.IsResolved);
        Assert.Null(mtc.TenantInfo);
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

        await mw.Invoke(context.Object);

        // next should have been called, but resolution should have been bypassed
        Assert.True(calledNext);
        Assert.False(itemsDict.ContainsKey(typeof(IMultiTenantContext)));
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

        await mw.Invoke(context.Object);

        // Endpoint exists, so resolution should proceed normally.
        Assert.True(calledNext);
        var mtc = (IMultiTenantContext<TenantInfo>?)itemsDict[typeof(IMultiTenantContext)];
        Assert.NotNull(mtc?.TenantInfo);
        Assert.Equal("initech", mtc.TenantInfo.Id);
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
        var mw = CreateMiddleware(
            _ =>
            {
                calledNext = true;
                return Task.CompletedTask;
            });
        // No bypassOptions = null predicate = no bypass applied

        await mw.Invoke(context.Object);

        // No predicate set, so resolution should proceed as normal even with no endpoint.
        Assert.True(calledNext);
        var mtc = (IMultiTenantContext<TenantInfo>?)itemsDict[typeof(IMultiTenantContext)];
        Assert.NotNull(mtc?.TenantInfo);
        Assert.Equal("initech", mtc.TenantInfo.Id);
    }
}