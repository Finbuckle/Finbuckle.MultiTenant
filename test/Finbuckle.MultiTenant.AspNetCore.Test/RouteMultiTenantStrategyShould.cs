using System;
using System.Collections.Concurrent;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.AspNetCore;
using Finbuckle.MultiTenant.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Moq;
using Xunit;

public class RouteMultiTenantStrategyShould
{
    private InMemoryMultiTenantStore CreateTestStore()
    {
        var store = new InMemoryMultiTenantStore();
        store.TryAdd(new TenantContext("initech", "initech", "Initech", null, null, null));

        return store;
    }

    private HttpContext CreateHttpContextMock(string routeValue)
    {
        var routeData = new RouteData();
        routeData.Values.Add("tenant", routeValue);
        var mockFeature = new Mock<IRoutingFeature>();
        mockFeature.Setup(f=>f.RouteData).Returns(routeData);

        var mock = new Mock<HttpContext>();
        mock.Setup(c => c.Features[typeof(IRoutingFeature)]).Returns(mockFeature.Object);

        return mock.Object;
    }

    private HttpContext CreateHttpContextMockWithNoRouteData(string routeValue)
    {
        var mock = new Mock<HttpContext>();
        mock.Setup(c => c.Features[typeof(IRoutingFeature)]).Returns(null);

        return mock.Object;
    }

    [Fact]
    public void GetTenantFromStore()
    {
        var store = CreateTestStore();
        var httpContext = CreateHttpContextMock("initech");

        var resolver = new TenantResolver(store, new RouteMultiTenantStrategy("tenant"));
        var tc = resolver.ResolveAsync(httpContext).Result;

        Assert.Equal("initech", tc.Id);
        Assert.Equal("initech", tc.Identifier);
        Assert.Equal("Initech", tc.Name);
        Assert.Equal(typeof(RouteMultiTenantStrategy), tc.MultiTenantStrategyType);
        Assert.Equal(typeof(InMemoryMultiTenantStore), tc.MultiTenantStoreType);
    }

    [Fact]
    public void ThrowIfContextIsNotHttpContext()
    {
        var store = CreateTestStore();
        var httpContext = new Object();
        var resolver = new TenantResolver(store, new RouteMultiTenantStrategy("tenant"));

        Assert.Throws<MultiTenantException>(() => resolver.ResolveAsync(httpContext).GetAwaiter().GetResult());
    }

    [Fact]
    public void ReturnNullIfNoRouteParamMatch()
    {
        var store = CreateTestStore();
        var httpContext = CreateHttpContextMock("initech");

        var resolver = new TenantResolver(store, new RouteMultiTenantStrategy("nomatch_tenant"));
        var tc = resolver.ResolveAsync(httpContext).Result;

        Assert.Null(tc);
    }

    [Fact]
    public void ReturnNullIfNoRouteData()
    {
        var store = CreateTestStore();
        var httpContext = CreateHttpContextMockWithNoRouteData("initech");

        var resolver = new TenantResolver(store, new RouteMultiTenantStrategy("tenant"));
        var tc = resolver.ResolveAsync(httpContext).Result;

        Assert.Null(tc);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ThrowIfRouteParamIsNullOrWhitespace(string testString)
    {
        Assert.Throws<MultiTenantException>(() => new RouteMultiTenantStrategy(testString));
    }
}