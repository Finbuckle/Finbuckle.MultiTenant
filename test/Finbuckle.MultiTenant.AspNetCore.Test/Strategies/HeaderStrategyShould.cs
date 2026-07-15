// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.AspNetCore.Strategies;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Finbuckle.MultiTenant.AspNetCore.Test.Strategies;

public class HeaderStrategyShould
{
    private static HttpContext CreateHttpContextMock(string headerKey, string? headerValue)
    {
        var mock = new Mock<HttpContext>();
        var headers = new HeaderDictionary();
        if (headerValue is not null)
            headers[headerKey] = headerValue;
        mock.Setup(c => c.Request.Headers).Returns(headers);

        return mock.Object;
    }

    [Fact]
    public async Task ReturnExpectedIdentifier()
    {
        var httpContext = CreateHttpContextMock("X-Tenant", "initech");
        var strategy = new HeaderStrategy("X-Tenant");

        var identifier = await strategy.GetIdentifierAsync(httpContext);

        Assert.Equal("initech", identifier);
    }

    [Fact]
    public async Task ReturnNullIfHeaderNotPresent()
    {
        var httpContext = CreateHttpContextMock("X-Tenant", null);
        var strategy = new HeaderStrategy("X-Tenant");

        var identifier = await strategy.GetIdentifierAsync(httpContext);

        Assert.Null(identifier);
    }

    [Fact]
    public async Task ReturnFirstValueIfHeaderHasMultipleValues()
    {
        var mock = new Mock<HttpContext>();
        var headers = new HeaderDictionary { ["X-Tenant"] = new[] { "initech", "other" } };
        mock.Setup(c => c.Request.Headers).Returns(headers);
        var strategy = new HeaderStrategy("X-Tenant");

        var identifier = await strategy.GetIdentifierAsync(mock.Object);

        Assert.Equal("initech", identifier);
    }

    [Fact]
    public async Task ReturnNullIfContextIsNotHttpContext()
    {
        var context = new object();
        var strategy = new HeaderStrategy("X-Tenant");

        Assert.Null(await strategy.GetIdentifierAsync(context));
    }
}
