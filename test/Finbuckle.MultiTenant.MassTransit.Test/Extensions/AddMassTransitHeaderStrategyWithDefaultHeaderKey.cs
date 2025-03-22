using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.MassTransit;
using Finbuckle.MultiTenant.MassTransit.Internal;
using Finbuckle.MultiTenant.MassTransit.Strategies;

using Microsoft.Extensions.DependencyInjection;

using System;

using Xunit;

namespace Finbuckle.MultiTenant.MassTransit.Test.Extensions;
public class MultiTenantBuilderExtensionsForMassTransitShould
{
    [Fact]
    public void AddMassTransitHeaderStrategyWithDefaultHeaderKey()
    {
        var services = new ServiceCollection();
        var builder = new MultiTenantBuilder<TenantInfo>(services);

        builder.WithMassTransitHeaderStrategy<TenantInfo>();

        var serviceProvider = services.BuildServiceProvider();
        var strategy = serviceProvider.GetService<IMultiTenantStrategy>();

        Assert.NotNull(strategy);
        Assert.IsType<MassTransitHeaderStrategy>(strategy);
    }

    [Fact]
    public void AddMassTransitHeaderStrategyWithCustomHeaderKey()
    {
        var services = new ServiceCollection();
        var builder = new MultiTenantBuilder<TenantInfo>(services);
        var customHeaderKey = "X-Custom-Tenant-ID";

        builder.WithMassTransitHeaderStrategy<TenantInfo>(customHeaderKey);

        var serviceProvider = services.BuildServiceProvider();
        var headerConfig = serviceProvider.GetService<ITenantHeaderConfiguration>();

        Assert.NotNull(headerConfig);
        Assert.Equal(customHeaderKey, headerConfig.TenantIdentifierHeaderKey);
    }

    [Fact]
    public void ThrowArgumentExceptionForNullOrWhitespaceHeaderKey()
    {
        var services = new ServiceCollection();
        var builder = new MultiTenantBuilder<TenantInfo>(services);

        Assert.Throws<ArgumentException>(() => builder.WithMassTransitHeaderStrategy<TenantInfo>(""));
        Assert.Throws<ArgumentException>(() => builder.WithMassTransitHeaderStrategy<TenantInfo>(" "));
        Assert.Throws<ArgumentException>(() => builder.WithMassTransitHeaderStrategy<TenantInfo>(null!));
    }
}
