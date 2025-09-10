// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.AspNetCore.Strategies;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Finbuckle.MultiTenant.AspNetCore.Test.Strategies;

public class HostStrategyShould
{
    private HttpContext CreateHttpContextMock(string host)
    {
        var mock = new Mock<HttpContext>();
        mock.Setup(c => c.Request.Host).Returns(new HostString(host));

        return mock.Object;
    }

    [Theory]
    [InlineData("", "__tenant__", null)] // no host
    [InlineData("initech", "__tenant__", "initech")] // basic match
    [InlineData("Initech", "__tenant__", "Initech")] // maintain case
    [InlineData("abc.com.test.", "__tenant__.", null)] // invalid pattern
    [InlineData("abc", "__tenant__.", null)] // invalid pattern
    [InlineData("abc", ".__tenant__", null)] // invalid pattern
    [InlineData("abc", ".__tenant__.", null)] // invalid pattern
    [InlineData("abc-cool.org", "__tenant__-cool.org", "abc")] // mixed segment
    [InlineData("abc.com.test", "__tenant__.*", "abc")] // first segment
    [InlineData("abc", "__tenant__.*", "abc")] // first and only segment
    [InlineData("www.example.test", "?.__tenant__.?", "example")] // domain
    [InlineData("www.example.test", "?.__tenant__.*", "example")] // 2nd segment
    [InlineData("www.example", "?.__tenant__.*", "example")] // 2nd segment
    [InlineData("www.example.r", "?.__tenant__.?.*", "example")] // 2nd segment of 3+
    [InlineData("www.example.r.f", "?.__tenant__.?.*", "example")] // 2nd segment of 3+
    [InlineData("example.ok.test", "*.__tenant__.?.?", "example")] // 3rd last segment
    [InlineData("w.example.ok.test", "*.?.__tenant__.?.?", "example")] // 3rd last of 4+ segments
    [InlineData("example.com", "__tenant__", "example.com")] // match entire domain (2.1)
    public async Task ReturnExpectedIdentifier(string host, string template, string? expected)
    {
        var httpContext = CreateHttpContextMock(host);
        var strategy = new HostStrategy(template);

        var identifier = await strategy.GetIdentifierAsync(httpContext);

        Assert.Equal(expected, identifier);
    }

    [Theory]
    [InlineData("*.__tenant__.*")]
    [InlineData("*a.__tenant__")]
    [InlineData("a*a.__tenant__")]
    [InlineData("a*.__tenant__")]
    [InlineData("*-.__tenant__")]
    [InlineData("-*-.__tenant__")]
    [InlineData("-*.__tenant__")]
    [InlineData("__tenant__.-?")]
    [InlineData("__tenant__.-?-")]
    [InlineData("__tenant__.?-")]
    [InlineData("")]
    [InlineData("     ")]
    [InlineData(null)]
    public void ThrowIfInvalidTemplate(string? template)
    {
        Assert.Throws<MultiTenantException>(() =>
            new HostStrategy(template!));
    }

    [Fact]
    public async Task ReturnNullIfContextIsNotHttpContext()
    {
        var context = new object();
        var strategy = new HostStrategy("__tenant__.*");

        Assert.Null(await strategy.GetIdentifierAsync(context));
    }
}