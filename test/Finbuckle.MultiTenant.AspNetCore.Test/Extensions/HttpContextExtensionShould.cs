// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.AspNetCore.Extensions;
using Finbuckle.MultiTenant.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Finbuckle.MultiTenant.AspNetCore.Test.Extensions;

public class HttpContextExtensionShould
{
    [Fact]
    public void GetExistingMultiTenantContext()
    {
        var ti = new TenantInfo { Id = "test", Identifier = "" };
        var mtc = new MultiTenantContext<TenantInfo>(ti);

        var httpContextMock = new Mock<HttpContext>();
        var itemsDict = new Dictionary<object, object?>
        {
            [typeof(IMultiTenantContext)] = mtc
        };
        httpContextMock.Setup(c => c.Items).Returns(itemsDict);

        var returnedMtc = httpContextMock.Object.GetMultiTenantContext<TenantInfo>();

        Assert.Same(mtc, returnedMtc);
    }

    [Fact]
    public void GetEmptyMultiTenantContextIfNoneSet()
    {
        var httpContextMock = new Mock<HttpContext>();
        var itemsDict = new Dictionary<object, object?>();
        httpContextMock.Setup(c => c.Items).Returns(itemsDict);

        var returnedMtc = httpContextMock.Object.GetMultiTenantContext<TenantInfo>();

        Assert.False(returnedMtc.IsResolved);
        Assert.Null(returnedMtc.TenantInfo);
        Assert.Null(returnedMtc.StoreInfo);
        Assert.Null(returnedMtc.StrategyInfo);
    }

    [Fact]
    public void ReturnTenantInfo()
    {
        var ti = new TenantInfo { Id = "test", Identifier = "" };
        var mtc = new MultiTenantContext<TenantInfo>(ti);

        var httpContextMock = new Mock<HttpContext>();
        var itemsDict = new Dictionary<object, object?>
        {
            [typeof(IMultiTenantContext)] = mtc
        };
        httpContextMock.Setup(c => c.Items).Returns(itemsDict);

        var returnedTi = httpContextMock.Object.GetTenantInfo<TenantInfo>();

        Assert.Equal(ti, returnedTi);
    }

    [Fact]
    public void ReturnNullTenantInfoIfNoTenantInfo()
    {
        var httpContextMock = new Mock<HttpContext>();
        var itemsDict = new Dictionary<object, object?>();
        httpContextMock.Setup(c => c.Items).Returns(itemsDict);

        var returnedTi = httpContextMock.Object.GetTenantInfo<TenantInfo>();

        Assert.Null(returnedTi);
    }

    [Fact]
    public void SetTenantInfo()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>();
        var sp = services.BuildServiceProvider();

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.RequestServices).Returns(sp);
        var itemsDict = new Dictionary<object, object?>();
        httpContextMock.Setup(c => c.Items).Returns(itemsDict);
        var context = httpContextMock.Object;

        var ti2 = new TenantInfo { Id = "tenant2", Identifier = "" };
        context.SetTenantInfo(ti2, false);
        var ti = context.GetTenantInfo<TenantInfo>();

        Assert.Equal(ti2, ti);
    }

    [Fact]
    public void SetMultiTenantContextAccessor()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>();
        var sp = services.BuildServiceProvider();

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.RequestServices).Returns(sp);
        var itemsDict = new Dictionary<object, object?>();
        httpContextMock.Setup(c => c.Items).Returns(itemsDict);
        var context = httpContextMock.Object;

        var ti2 = new TenantInfo { Id = "tenant2", Identifier = "" };
        context.SetTenantInfo(ti2, false);
        var mtc = context.GetMultiTenantContext<TenantInfo>();
        var accessor = context.RequestServices.GetRequiredService<IMultiTenantContextAccessor<TenantInfo>>();

        Assert.Same(mtc, accessor.MultiTenantContext);
    }

    [Fact]
    public void SetStoreInfoAndStrategyInfoNull()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>();
        var sp = services.BuildServiceProvider();

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.RequestServices).Returns(sp);
        var itemsDict = new Dictionary<object, object?>();
        httpContextMock.Setup(c => c.Items).Returns(itemsDict);
        var context = httpContextMock.Object;

        var ti2 = new TenantInfo { Id = "tenant2", Identifier = "" };
        context.SetTenantInfo(ti2, false);
        var mtc = context.GetMultiTenantContext<TenantInfo>();

        Assert.Null(mtc.StoreInfo);
        Assert.Null(mtc.StrategyInfo);
    }

    [Fact]
    public void ResetScopeIfApplicable()
    {
        var httpContextMock = new Mock<HttpContext>();

        httpContextMock.SetupProperty(c => c.RequestServices);

        var services = new ServiceCollection();
        services.AddScoped<object>(_ => DateTime.Now);
        services.AddMultiTenant<TenantInfo>();
        var sp = services.BuildServiceProvider();
        httpContextMock.Object.RequestServices = sp;
        var itemsDict = new Dictionary<object, object?>();
        httpContextMock.Setup(c => c.Items).Returns(itemsDict);

        var ti2 = new TenantInfo { Id = "tenant2", Identifier = "" };
        httpContextMock.Object.SetTenantInfo(ti2, true);

        Assert.NotSame(sp, httpContextMock.Object.RequestServices);
        Assert.NotStrictEqual((DateTime?)sp.GetService<object>(),
            (DateTime?)httpContextMock.Object.RequestServices.GetService<object>());
    }

    [Fact]
    public void NotResetScopeIfNotApplicable()
    {
        var httpContextMock = new Mock<HttpContext>();

        httpContextMock.SetupProperty(c => c.RequestServices);

        var services = new ServiceCollection();
        services.AddScoped<object>(_ => DateTime.Now);
        services.AddMultiTenant<TenantInfo>();
        var sp = services.BuildServiceProvider();
        httpContextMock.Object.RequestServices = sp;
        var itemsDict = new Dictionary<object, object?>();
        httpContextMock.Setup(c => c.Items).Returns(itemsDict);

        var ti2 = new TenantInfo { Id = "tenant2", Identifier = "" };
        httpContextMock.Object.SetTenantInfo(ti2, false);

        Assert.Same(sp, httpContextMock.Object.RequestServices);
        Assert.StrictEqual((DateTime?)sp.GetService<object>(),
            (DateTime?)httpContextMock.Object.RequestServices.GetService<object>());
    }
}