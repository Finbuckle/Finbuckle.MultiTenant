// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using Finbuckle.MultiTenant.Stores;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test.Extensions
{
    public class MultiTenantBuilderExtensionsShould
    {
        public class TestEfCoreStoreDbContext : EFCoreStoreDbContext<TenantInfo>
        {
            public TestEfCoreStoreDbContext(DbContextOptions options) : base(options)
            {
            }
        }

        [Fact]
        public void AddEfCoreStore()
        {
            var services = new ServiceCollection();
            var builder = new FinbuckleMultiTenantBuilder<TenantInfo>(services);
            builder.WithStaticStrategy("initech").WithEFCoreStore<TestEfCoreStoreDbContext, TenantInfo>();
            var sp = services.BuildServiceProvider().CreateScope().ServiceProvider;

            var resolver = sp.GetRequiredService<IMultiTenantStore<TenantInfo>>();
            Assert.IsType<EFCoreStore<TestEfCoreStoreDbContext, TenantInfo>>(resolver);
        }

        [Fact]
        public void AddEfCoreStoreWithExistingDbContext()
        {
            var services = new ServiceCollection();
            var builder = new FinbuckleMultiTenantBuilder<TenantInfo>(services);
            services.AddDbContext<TestEfCoreStoreDbContext>(o => o.UseSqlite("DataSource=:memory:"));
            builder.WithStaticStrategy("initech").WithEFCoreStore<TestEfCoreStoreDbContext, TenantInfo>();
            var sp = services.BuildServiceProvider().CreateScope().ServiceProvider;

            var resolver = sp.GetRequiredService<IMultiTenantStore<TenantInfo>>();
            Assert.IsType<EFCoreStore<TestEfCoreStoreDbContext, TenantInfo>>(resolver);
        }
    }
}