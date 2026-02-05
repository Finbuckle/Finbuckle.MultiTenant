// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.AspNetCore.Strategies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Moq;
using Xunit;

namespace Finbuckle.MultiTenant.AspNetCore.Test.Strategies;

public class RouteStrategyShould
{
    [Theory]
    [InlineData("__tenant__", "initech")]
    [InlineData("customParam", "initech123")]
    public async Task ReturnExpectedIdentifier(string identifier, string expected)
    {
        var routeData = new RouteValueDictionary
        {
            [identifier] = expected
        };
        var mockContext = new Mock<HttpContext>();
        mockContext.Setup(c => c.Request.RouteValues).Returns(routeData);

        var strategy = new RouteStrategy(identifier);

        Assert.Equal(expected, await strategy.GetIdentifierAsync(mockContext.Object));
    }

    [Fact]
    public async Task ReturnNullIfContextIsNotHttpContext()
    {
        var context = new object();
        var strategy = new RouteStrategy("__tenant__");

        Assert.Null(await strategy.GetIdentifierAsync(context));
    }

    [Fact]
    public async Task ReturnNullIfNoRouteParamMatch()
    {
        var routeData = new RouteValueDictionary();
        var mockContext = new Mock<HttpContext>();
        mockContext.Setup(c => c.Request.RouteValues).Returns(routeData);

        var strategy = new RouteStrategy("__tenant__");

        Assert.Null(await strategy.GetIdentifierAsync(mockContext.Object));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ThrowIfRouteParamIsNullOrWhitespace(string? testString)
    {
        Assert.Throws<ArgumentException>(() =>
            new RouteStrategy(testString!));
    }
}