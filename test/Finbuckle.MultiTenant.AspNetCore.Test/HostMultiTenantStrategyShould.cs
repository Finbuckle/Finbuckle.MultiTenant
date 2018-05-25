using System;
using Finbuckle.MultiTenant.AspNetCore;
using Finbuckle.MultiTenant.Core;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

public class HostMultiTenantStrategyShould
{
    private HttpContext CreateHttpContextMock(string host)
    {
        var mock = new Mock<HttpContext>();
        mock.Setup(c => c.Request.Host).Returns(new HostString(host));

        return mock.Object;
    }

    [Theory]
    [InlineData("", "template", null)] // no host
    [InlineData("abc.com.test.", "template", null)] // invalid pattern
    [InlineData("abc.", "template", null)] // invalid pattern
    [InlineData(".abc", "template", null)] // invalid pattern
    [InlineData(".abc.", "template", null)] // invalid pattern
    [InlineData("abc", "__tenant__", "abc")] // only segment
    [InlineData("abc.com.test", "__tenant__.*", "abc")] // first segment
    [InlineData("Abc.com.test", "__tenant__.*", "abc")] // first segment, ignore case
    [InlineData("www.example.test", "?.__tenant__.?", "example")] // domain
    [InlineData("www.example.test", "?.__tenant__.*", "example")] // 2nd segment
    [InlineData("www.example", "?.__tenant__.*", "example")] // 2nd segment
    [InlineData("www.example.r", "?.__tenant__.?.*", "example")] // 2nd segment of 3+
    [InlineData("www.example.r.f", "?.__tenant__.?.*", "example")] // 2nd segment of 3+
    [InlineData("example.ok.test", "*.__tenant__.?.?", "example")] // 3rd last segment
    [InlineData("w.example.ok.test", "*.?.__tenant__.?.?", "example")] // 3rd last of 4+ segments
    public void ReturnExpectedIdentifier(string host, string template, string expected)
    {
        var httpContext = CreateHttpContextMock(host);
        var strategy = new HostMultiTenantStrategy(template);

        var identifier = strategy.GetIdentifier(httpContext);

        Assert.Equal(expected, identifier);
    }

    [Theory]
    [InlineData("*.__tenant__.*")]
    [InlineData("")]
    [InlineData("     ")]
    [InlineData(null)]

    public void ThrowIfInvalidTemplate(string template)
    {
        Assert.Throws<MultiTenantException>(() => new HostMultiTenantStrategy(template));
    }

    [Fact]
    public void ThrowIfContextIsNotHttpContext()
    {
        var context = new Object();
        var strategy = new HostMultiTenantStrategy("__tenant__.*");

        Assert.Throws<MultiTenantException>(() => strategy.GetIdentifier(context));
    }
}