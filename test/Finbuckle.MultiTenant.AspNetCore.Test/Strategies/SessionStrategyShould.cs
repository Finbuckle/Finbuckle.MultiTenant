// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Text;
using Finbuckle.MultiTenant.AspNetCore.Strategies;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Finbuckle.MultiTenant.AspNetCore.Test.Strategies;

public class SessionStrategyShould
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ThrowIfInvalidSessionKey(string? key)
    {
        Assert.Throws<ArgumentException>(() => new SessionStrategy(key!));
    }

    [Fact]
    public async Task ReturnNullIfContextIsNotHttpContext()
    {
        var context = new object();
        var strategy = new SessionStrategy("__tenant__");

        Assert.Null(await strategy.GetIdentifierAsync(context));
    }

    [Fact]
    public async Task ReturnNullIfNoSessionValue()
    {
        var sessionData = new Dictionary<string, string>();
        var mockSession = new Mock<ISession>();
        mockSession
            .Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny!))
            .Returns((string key, out byte[] value) =>
            {
                if (sessionData.TryGetValue(key, out var str))
                {
                    value = Encoding.UTF8.GetBytes(str);
                    return true;
                }

                value = null!;
                return false;
            });

        var mockContext = new Mock<HttpContext>();
        mockContext.Setup(c => c.Session).Returns(mockSession.Object);

        var strategy = new SessionStrategy("__tenant__");

        Assert.Null(await strategy.GetIdentifierAsync(mockContext.Object));
    }

    [Theory]
    [InlineData("__tenant__", "tenant", null)]
    [InlineData("__tenant__", "__tenant__", "initech")]
    public async Task ReturnIdentifierIfSessionValue(string tenantSessionKey, string sessionKey, string? expected)
    {
        var sessionData = new Dictionary<string, string>();
        if (expected != null)
            sessionData[sessionKey] = expected;

        var mockSession = new Mock<ISession>();
        mockSession
            .Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny!))
            .Returns((string key, out byte[] value) =>
            {
                if (sessionData.TryGetValue(key, out var str))
                {
                    value = Encoding.UTF8.GetBytes(str);
                    return true;
                }

                value = null!;
                return false;
            });

        var mockContext = new Mock<HttpContext>();
        mockContext.Setup(c => c.Session).Returns(mockSession.Object);

        var strategy = new SessionStrategy(tenantSessionKey);

        Assert.Equal(expected, await strategy.GetIdentifierAsync(mockContext.Object));
    }
}