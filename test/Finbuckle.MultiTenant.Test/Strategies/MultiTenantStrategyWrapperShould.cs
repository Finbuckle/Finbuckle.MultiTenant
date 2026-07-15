// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Strategies;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Finbuckle.MultiTenant.Test.Strategies;

public class MultiTenantStrategyWrapperShould
{
    [Fact]
    public void ThrowIfStrategyIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new MultiTenantStrategyWrapper(null!, Mock.Of<ILogger>()));
    }

    [Fact]
    public void ThrowIfLoggerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new MultiTenantStrategyWrapper(Mock.Of<IMultiTenantStrategy>(), null!));
    }

    [Theory]
    [InlineData("initech")]
    [InlineData(null)]
    public async Task ReturnWrappedStrategyResult(string? identifier)
    {
        var strategy = new Mock<IMultiTenantStrategy>();
        strategy.Setup(s => s.GetIdentifierAsync(It.IsAny<object>())).ReturnsAsync(identifier);
        var wrapper = new MultiTenantStrategyWrapper(strategy.Object, Mock.Of<ILogger>());

        Assert.Equal(identifier, await wrapper.GetIdentifierAsync(new object()));
    }

    [Fact]
    public async Task WrapStrategyExceptions()
    {
        var innerException = new InvalidOperationException("failed");
        var strategy = new Mock<IMultiTenantStrategy>();
        strategy.Setup(s => s.GetIdentifierAsync(It.IsAny<object>())).ThrowsAsync(innerException);
        var wrapper = new MultiTenantStrategyWrapper(strategy.Object, Mock.Of<ILogger>());

        var exception = await Assert.ThrowsAsync<MultiTenantException>(() =>
            wrapper.GetIdentifierAsync(new object()));

        Assert.Same(innerException, exception.InnerException);
        Assert.Contains(strategy.Object.GetType().ToString(), exception.Message);
    }
}
