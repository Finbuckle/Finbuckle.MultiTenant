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
    public void GetExistingTenantContext()
    {
        var ti = new TenantInfo { Id = "test", Identifier = "" };

        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>();
        var sp = services.BuildServiceProvider();
        sp.BeginTenantScope(ti);

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.RequestServices).Returns(sp);

        var returnedMtc = httpContextMock.Object.GetTenantContext<TenantInfo>();

        Assert.Same(sp.GetRequiredService<ITenantContext<TenantInfo>>(), returnedMtc);
    }

    [Fact]
    public void GetExistingNonGenericTenantContext()
    {
        var ti = new TenantInfo { Id = "test", Identifier = "" };

        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>();
        var sp = services.BuildServiceProvider();
        sp.BeginTenantScope(ti);

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.RequestServices).Returns(sp);

        var returnedMtc = httpContextMock.Object.TenantContext;

        Assert.Same(sp.GetRequiredService<ITenantContext>(), returnedMtc);
    }

    [Fact]
    public void GetEmptyTenantContextIfNoneSet()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>();
        var sp = services.BuildServiceProvider();
        sp.BeginTenantScope();

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.RequestServices).Returns(sp);

        var returnedMtc = httpContextMock.Object.GetTenantContext<TenantInfo>();

        Assert.False(returnedMtc.IsResolved);
        Assert.Null(returnedMtc.TenantInfo);
    }

    [Fact]
    public void ReturnTenantInfo()
    {
        var ti = new TenantInfo { Id = "test", Identifier = "" };

        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>();
        var sp = services.BuildServiceProvider();
        sp.BeginTenantScope(ti);

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.RequestServices).Returns(sp);

        var returnedTi = httpContextMock.Object.GetTenantInfo<TenantInfo>();

        Assert.Equal(ti, returnedTi);
    }

    [Fact]
    public void ReturnNullTenantInfoIfNoTenantInfo()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>();
        var sp = services.BuildServiceProvider();
        sp.BeginTenantScope();

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.RequestServices).Returns(sp);

        var returnedTi = httpContextMock.Object.GetTenantInfo<TenantInfo>();

        Assert.Null(returnedTi);
    }

    [Fact]
    public void ReturnCurrentTenant()
    {
        var ti = new TenantInfo { Id = "test", Identifier = "" };

        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>();
        var sp = services.BuildServiceProvider();
        sp.BeginTenantScope(ti);

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.RequestServices).Returns(sp);

        var returnedTi = httpContextMock.Object.TenantInfo;

        Assert.Equal(ti, returnedTi);
    }

    [Fact]
    public void ReturnNullCurrentTenantIfNoTenantInfo()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>();
        var sp = services.BuildServiceProvider();
        sp.BeginTenantScope();

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.RequestServices).Returns(sp);

        var returnedTi = httpContextMock.Object.TenantInfo;

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
        sp.BeginTenantScope();

        var ti2 = new TenantInfo { Id = "tenant2", Identifier = "" };
        context.SetTenantInfo(ti2);
        var ti = context.GetTenantInfo<TenantInfo>();

        Assert.Equal(ti2, ti);
    }

    [Fact]
    public void TrySetTenantInfoDoesNotReplaceExistingTenant()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>();
        var sp = services.BuildServiceProvider();
        sp.BeginTenantScope();

        var httpContext = new Mock<HttpContext>();
        httpContext.Setup(c => c.RequestServices).Returns(sp);
        var first = new TenantInfo { Id = "first", Identifier = "first" };
        var second = new TenantInfo { Id = "second", Identifier = "second" };

        httpContext.Object.TrySetTenantInfo(first);
        httpContext.Object.TrySetTenantInfo(second);

        Assert.Same(first, httpContext.Object.GetTenantInfo<TenantInfo>());
    }
}
