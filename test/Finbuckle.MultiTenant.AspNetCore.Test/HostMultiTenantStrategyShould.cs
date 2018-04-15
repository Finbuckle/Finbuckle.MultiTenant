using System;
using System.Collections.Concurrent;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.AspNetCore;
using Finbuckle.MultiTenant.Core;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

public class HostMultiTenantStrategyShould
{
    private InMemoryMultiTenantStore CreateTestStore()
    {
        var store = new InMemoryMultiTenantStore();
        store.TryAdd(new TenantContext("initech", "initech", "Initech", null, null, null));

        return store;
    }

    private HttpContext CreateHttpContextMock(string host)
    {
        var mock = new Mock<HttpContext>();
        mock.Setup(c => c.Request.Host).Returns(new HostString(host));

        return mock.Object;
    }

    [Fact]
    public void GetTenantFromStore()
    {
        var store = CreateTestStore();
        var httpContext = CreateHttpContextMock("initech");

        var resolver = new TenantResolver(store, new HostMultiTenantStrategy());
        var tc = resolver.ResolveAsync(httpContext).Result;

        Assert.Equal("initech", tc.Id);
        Assert.Equal("initech", tc.Identifier);
        Assert.Equal("Initech", tc.Name);
        Assert.Equal(typeof(HostMultiTenantStrategy), tc.MultiTenantStrategyType);
        Assert.Equal(typeof(InMemoryMultiTenantStore), tc.MultiTenantStoreType);
    }

    [Fact]
    public void ReturnNullIfNoHost()
    {
        var store = CreateTestStore();
        var httpContext = CreateHttpContextMock("");

        var resolver = new TenantResolver(store, new HostMultiTenantStrategy());
        var tc = resolver.ResolveAsync(httpContext).Result;

        Assert.Null(tc);
    }

    [Fact]
    public void HandleMultipleHostSegments()
    {
        var store = CreateTestStore();
        var httpContext = CreateHttpContextMock("initech.ignore.ignore");

        var resolver = new TenantResolver(store, new HostMultiTenantStrategy());
        var tc = resolver.ResolveAsync(httpContext).Result;

        Assert.Equal("initech", tc.Id);
        Assert.Equal("initech", tc.Identifier);
        Assert.Equal("Initech", tc.Name);
        Assert.Equal(typeof(HostMultiTenantStrategy), tc.MultiTenantStrategyType);
        Assert.Equal(typeof(InMemoryMultiTenantStore), tc.MultiTenantStoreType);
    }

    [Fact]
    public void ThrowIfContextIsNotHttpContext()
    {
        var store = CreateTestStore();
        var httpContext = new Object();
        var resolver = new TenantResolver(store, new HostMultiTenantStrategy());

        Assert.Throws<MultiTenantException>(() => resolver.ResolveAsync(httpContext).GetAwaiter().GetResult());
    }
}