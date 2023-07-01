// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Microsoft.EntityFrameworkCore;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test.MultiTenantIdentityDbContext
{
    public class TestIdentityDbContext : MultiTenant.MultiTenantIdentityDbContext
    {
        public TestIdentityDbContext(TenantInfo tenantInfo)
            : base(tenantInfo)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("DataSource=:memory:");
            base.OnConfiguring(optionsBuilder);
        }
    }
}