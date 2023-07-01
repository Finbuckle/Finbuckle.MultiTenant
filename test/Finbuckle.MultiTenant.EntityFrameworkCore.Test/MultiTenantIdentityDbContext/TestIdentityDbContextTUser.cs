// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test.MultiTenantIdentityDbContext
{
    public class TestIdentityDbContextTUser : MultiTenantIdentityDbContext<IdentityUser>
    {
        public TestIdentityDbContextTUser(TenantInfo tenantInfo)
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