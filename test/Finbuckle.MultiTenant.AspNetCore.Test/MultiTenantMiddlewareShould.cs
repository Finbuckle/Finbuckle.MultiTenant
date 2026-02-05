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

namespace Finbuckle.MultiTenant.AspNetCore.Test;

public class MultiTenantMiddlewareShould
{
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

        var mw = new MultiTenantMiddleware(_ => Task.CompletedTask);

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

        var options = new ShortCircuitWhenOptions { Predicate = context => !context.IsResolved };
        var optionsMock = new Mock<IOptions<ShortCircuitWhenOptions>>();
        optionsMock.Setup(c => c.Value).Returns(options);

        var calledNext = false;
        var mw = new MultiTenantMiddleware(_ =>
        {
            calledNext = true;

            return Task.CompletedTask;
        }, optionsMock.Object);

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

        var mw = new MultiTenantMiddleware(httpContext =>
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

        var mw = new MultiTenantMiddleware(httpContext =>
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

        var options = new ShortCircuitWhenOptions { Predicate = context => !context.IsResolved };
        var optionsMock = new Mock<IOptions<ShortCircuitWhenOptions>>();
        optionsMock.Setup(c => c.Value).Returns(options);

        var calledNext = false;
        var mw = new MultiTenantMiddleware(_ =>
        {
            calledNext = true;

            return Task.CompletedTask;
        }, optionsMock.Object);

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

        var options = new ShortCircuitWhenOptions
        {
            Predicate = context => !context.IsResolved,
            RedirectTo = new Uri("/tenant/notfound", UriKind.Relative)
        };
        var optionsMock = new Mock<IOptions<ShortCircuitWhenOptions>>();
        optionsMock.Setup(c => c.Value).Returns(options);

        var calledNext = false;
        var mw = new MultiTenantMiddleware(_ =>
        {
            calledNext = true;

            return Task.CompletedTask;
        }, optionsMock.Object);

        await mw.Invoke(context.Object);

        var mtc = (IMultiTenantContext<TenantInfo>?)context.Object.Items[typeof(IMultiTenantContext)];

        Assert.NotNull(mtc);
        Assert.False(mtc.IsResolved);
        Assert.Null(mtc.TenantInfo);
        Assert.False(calledNext);
        response.Verify(r => r.Redirect("/tenant/notfound"), Times.Once);
    }
}