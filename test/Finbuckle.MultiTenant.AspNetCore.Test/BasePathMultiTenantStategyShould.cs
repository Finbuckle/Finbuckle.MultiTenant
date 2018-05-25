using System;
using Finbuckle.MultiTenant.AspNetCore;
using Finbuckle.MultiTenant.Core;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

public class BasePathMultiTenantStrategyShould
{
    private HttpContext CreateHttpContextMock(string path)
    {
        var mock = new Mock<HttpContext>();
        mock.Setup(c => c.Request.Path).Returns(path);

        return mock.Object;
    }

    [Theory]
    [InlineData("/test", "test")] // single path
    [InlineData("/Test", "Test")] // maintain case
    [InlineData("", null)] // no path
    [InlineData("/", null)] // just trailing slash
    [InlineData("/initech/ignore/ignore", "initech")] // multiple path segments
    public void ReturnExpectedIdentifier(string path, string expected)
    {
        var httpContext = CreateHttpContextMock(path);
        var strategy = new BasePathMultiTenantStrategy();

        var identifier = strategy.GetIdentifier(httpContext);

        Assert.Equal(expected, identifier);
    }

    [Fact]
    public void ThrowIfContextIsNotHttpContext()
    {
        var context = new Object();
        var strategy = new BasePathMultiTenantStrategy();

        Assert.Throws<MultiTenantException>(() => strategy.GetIdentifier(context));
    }
}