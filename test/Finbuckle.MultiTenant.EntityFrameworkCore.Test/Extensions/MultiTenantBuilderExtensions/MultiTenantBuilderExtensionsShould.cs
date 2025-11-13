// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;
using Finbuckle.MultiTenant.EntityFrameworkCore.Stores;
using Finbuckle.MultiTenant.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test.Extensions.MultiTenantBuilderExtensions;

public class MultiTenantBuilderExtensionsShould
{
    [Fact]
    public void AddEfCoreStore()
    {
        var services = new ServiceCollection();
        var builder = new MultiTenantBuilder<TenantInfo>(services);
        builder.WithStaticStrategy("initech").WithEFCoreStore<TestEfCoreStoreDbContext, TenantInfo>();
        var sp = services.BuildServiceProvider().CreateScope().ServiceProvider;

        var resolver = sp.GetRequiredService<IMultiTenantStore<TenantInfo>>();
        Assert.IsType<EFCoreStore<TestEfCoreStoreDbContext, TenantInfo>>(resolver);
    }

    [Fact]
    public void AddEfCoreStoreWithExistingDbContext()
    {
        var services = new ServiceCollection();
        var builder = new MultiTenantBuilder<TenantInfo>(services);
        services.AddDbContext<TestEfCoreStoreDbContext>(o => o.UseSqlite("DataSource=:memory:"));
        builder.WithStaticStrategy("initech").WithEFCoreStore<TestEfCoreStoreDbContext, TenantInfo>();
        var sp = services.BuildServiceProvider().CreateScope().ServiceProvider;

        var resolver = sp.GetRequiredService<IMultiTenantStore<TenantInfo>>();
        Assert.IsType<EFCoreStore<TestEfCoreStoreDbContext, TenantInfo>>(resolver);
    }
}