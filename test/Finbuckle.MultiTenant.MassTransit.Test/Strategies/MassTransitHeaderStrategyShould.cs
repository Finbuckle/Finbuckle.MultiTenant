using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.MassTransit.Internal;
using Finbuckle.MultiTenant.MassTransit.Strategies;

using MassTransit;

using Moq;

using System.Threading.Tasks;

using Xunit;

namespace Finbuckle.MultiTenant.MassTransit.Test.Strategies;

public class MassTransitHeaderStrategyShould
{
    private ConsumeContext CreateConsumeContextMock(string tenantIdentifierHeaderKey, object? tenantId)
    {
        var headersMock = new Mock<Headers>();
        headersMock.Setup(h => h.TryGetHeader(tenantIdentifierHeaderKey, out tenantId)).Returns(!string.IsNullOrEmpty((string?)tenantId));

        var contextMock = new Mock<ConsumeContext>();
        contextMock.Setup(c => c.Headers).Returns(headersMock.Object);

        return contextMock.Object;
    }

    private CompensateContext CreateCompensateContextMock(string tenantIdentifierHeaderKey, object? tenantId)
    {
        var headersMock = new Mock<Headers>();
        headersMock.Setup(h => h.TryGetHeader(tenantIdentifierHeaderKey, out tenantId)).Returns(!string.IsNullOrEmpty((string?)tenantId));

        var contextMock = new Mock<CompensateContext>();
        contextMock.Setup(c => c.Headers).Returns(headersMock.Object);

        return contextMock.Object;
    }

    private ExecuteContext CreateExecuteContextMock(string tenantIdentifierHeaderKey, object? tenantId)
    {
        var headersMock = new Mock<Headers>();
        headersMock.Setup(h => h.TryGetHeader(tenantIdentifierHeaderKey, out tenantId)).Returns(!string.IsNullOrEmpty((string?)tenantId));

        var contextMock = new Mock<ExecuteContext>();
        contextMock.Setup(c => c.Headers).Returns(headersMock.Object);

        return contextMock.Object;
    }

    [Theory]
    [InlineData("X-Tenant-ID", "tenant-123", "tenant-123")] // Header present
    [InlineData("X-Tenant-ID", null, null)] // Header missing
    [InlineData("__tenant__", "tenant-1", "tenant-1")] // Header present
    [InlineData("__tenant__", null, null)] // Header missing
    [InlineData("tenantIdentifier", "tenant-1", "tenant-1")] // Header present
    [InlineData("tenantIdentifier", null, null)] // Header missing
    public async Task ReturnExpectedIdentifierConsumeContext(string tenantIdentifierHeaderKey, string? headerValue, string? expected)
    {
        var headerConfigMock = new Mock<ITenantHeaderConfiguration>();
        headerConfigMock.SetupGet(c => c.TenantIdentifierHeaderKey).Returns(tenantIdentifierHeaderKey);

        var consumeContext = CreateConsumeContextMock(tenantIdentifierHeaderKey, headerValue);
        var strategy = new MassTransitHeaderStrategy(headerConfigMock.Object);

        var identifier = await strategy.GetIdentifierAsync(consumeContext);

        Assert.Equal(expected, identifier);
    }

    [Theory]
    [InlineData("X-Tenant-ID", "tenant-123", "tenant-123")] // Header present
    [InlineData("X-Tenant-ID", null, null)] // Header missing
    [InlineData("__tenant__", "tenant-1", "tenant-1")] // Header present
    [InlineData("__tenant__", null, null)] // Header missing
    [InlineData("tenantIdentifier", "tenant-1", "tenant-1")] // Header present
    [InlineData("tenantIdentifier", null, null)] // Header missing
    public async Task ReturnExpectedIdentifierCompensateContext(string tenantIdentifierHeaderKey, string? headerValue, string? expected)
    {
        var headerConfigMock = new Mock<ITenantHeaderConfiguration>();
        headerConfigMock.SetupGet(c => c.TenantIdentifierHeaderKey).Returns(tenantIdentifierHeaderKey);

        var compensateContext = CreateCompensateContextMock(tenantIdentifierHeaderKey, headerValue);
        var strategy = new MassTransitHeaderStrategy(headerConfigMock.Object);

        var identifier = await strategy.GetIdentifierAsync(compensateContext);

        Assert.Equal(expected, identifier);
    }

    [Theory]
    [InlineData("X-Tenant-ID", "tenant-123", "tenant-123")] // Header present
    [InlineData("X-Tenant-ID", null, null)] // Header missing
    [InlineData("__tenant__", "tenant-1", "tenant-1")] // Header present
    [InlineData("__tenant__", null, null)] // Header missing
    [InlineData("tenantIdentifier", "tenant-1", "tenant-1")] // Header present
    [InlineData("tenantIdentifier", null, null)] // Header missing
    public async Task ReturnExpectedIdentifierExecuteContext(string tenantIdentifierHeaderKey, string? headerValue, string? expected)
    {
        var headerConfigMock = new Mock<ITenantHeaderConfiguration>();
        headerConfigMock.SetupGet(c => c.TenantIdentifierHeaderKey).Returns(tenantIdentifierHeaderKey);

        var executeContext = CreateExecuteContextMock(tenantIdentifierHeaderKey, headerValue);
        var strategy = new MassTransitHeaderStrategy(headerConfigMock.Object);

        var identifier = await strategy.GetIdentifierAsync(executeContext);

        Assert.Equal(expected, identifier);
    }

    [Fact]
    public async Task ReturnNullForUnsupportedContextTypes()
    {
        var headerConfigMock = new Mock<ITenantHeaderConfiguration>();
        var strategy = new MassTransitHeaderStrategy(headerConfigMock.Object);
        var unsupportedContext = new object();

        var identifier = await strategy.GetIdentifierAsync(unsupportedContext);

        Assert.Null(identifier);
    }
}
