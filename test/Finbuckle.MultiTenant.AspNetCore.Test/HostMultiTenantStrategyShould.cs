using System;
using System.Collections.Concurrent;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.AspNetCore;
using Finbuckle.MultiTenant.Core;
using Finbuckle.MultiTenant.Core.Abstractions;
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

    [Theory]
    [InlineData("abc.com.test.", null, null)] // invalid pattern
    [InlineData("abc.", null, null)] // invalid pattern
    [InlineData(".abc", null, null)] // invalid pattern
    [InlineData(".abc.", null, null)] // invalid pattern
    [InlineData("abc", "__tenant__", "abc")] // only segment
    [InlineData("abc.com.test", null, "abc")] // first segment
    [InlineData("Abc.com.test", null, "abc")] // first segment, ignore case
    [InlineData("www.example.test", "?.__tenant__.?", "example")] // domain
    [InlineData("www.example.test", "?.__tenant__.*", "example")] // 2nd segment
    [InlineData("www.example", "?.__tenant__.*", "example")] // 2nd segment
    [InlineData("www.example.r", "?.__tenant__.?.*", "example")] // 2nd segment of 3+
    [InlineData("www.example.r.f", "?.__tenant__.?.*", "example")] // 2nd segment of 3+
    [InlineData("example.ok.test", "*.__tenant__.?.?", "example")] // 3rd last segment
    [InlineData("w.example.ok.test", "*.?.__tenant__.?.?", "example")] // 3rd last of 4+ segments
    public void ReturnExpectedIdentifier(string host, string template, string expected)
    {
        var store = CreateTestStore();
        var httpContext = CreateHttpContextMock(host);

        IMultiTenantStrategy strategy = null;

        if (template == null)
        {
            strategy = new HostMultiTenantStrategy();
        }
        else
        {
            strategy = new HostMultiTenantStrategy(template);
        }

        var identifier = strategy.GetIdentifier(httpContext);

        Assert.Equal(expected, identifier);
    }

    [Theory]
    [InlineData("*.__tenant__.*")]
    [InlineData("")]
    [InlineData("     ")]
    
    public void ThrowIfInvalidTemplate(string template)
    {
        Assert.Throws<MultiTenantException>(() => new HostMultiTenantStrategy(template));
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