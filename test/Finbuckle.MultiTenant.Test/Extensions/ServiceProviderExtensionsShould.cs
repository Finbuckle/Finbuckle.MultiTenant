// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Finbuckle.MultiTenant.Test.Extensions;

public class ServiceProviderExtensionsShould
{
    [Fact]
    public void BeginUnresolvedTenantScope()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>();
        using var provider = services.BuildServiceProvider();

        provider.BeginTenantScope();

        var tenantContext = provider.GetRequiredService<ITenantContext<TenantInfo>>();
        Assert.False(tenantContext.IsResolved);
        Assert.Null(tenantContext.TenantInfo);
    }

    [Fact]
    public void BeginResolvedTenantScope()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>();
        using var provider = services.BuildServiceProvider();
        var tenantInfo = new TenantInfo { Id = "tenant-id", Identifier = "tenant" };

        provider.BeginTenantScope(tenantInfo);

        var tenantContext = provider.GetRequiredService<ITenantContext<TenantInfo>>();
        Assert.True(tenantContext.IsResolved);
        Assert.Same(tenantInfo, tenantContext.TenantInfo);
    }

    [Fact]
    public void ReplaceCurrentTenantScope()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>();
        using var provider = services.BuildServiceProvider();
        var tenantInfo = new TenantInfo { Id = "tenant-id", Identifier = "tenant" };
        provider.BeginTenantScope(tenantInfo);

        provider.BeginTenantScope();

        var tenantContext = provider.GetRequiredService<ITenantContext<TenantInfo>>();
        Assert.False(tenantContext.IsResolved);
        Assert.Null(tenantContext.TenantInfo);
    }

    [Fact]
    public void ThrowIfServiceProviderIsNull()
    {
        IServiceProvider services = null!;

        Assert.Throws<ArgumentNullException>(() => services.BeginTenantScope());
    }

    [Fact]
    public void ThrowIfTenantInfoIsNull()
    {
        using var provider = new ServiceCollection().BuildServiceProvider();

        Assert.Throws<ArgumentNullException>(() => provider.BeginTenantScope(null!));
    }
}
