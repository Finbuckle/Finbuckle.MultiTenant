// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Stores;
using Microsoft.EntityFrameworkCore;

namespace EFCoreStoreSample.Data
{

    public class MultiTenantStoreDbContext : EFCoreStoreDbContext<TenantInfo>
    {
        public MultiTenantStoreDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("EFCoreStoreSampleConnectionString");
            base.OnConfiguring(optionsBuilder);
        }
    }
}