// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Microsoft.EntityFrameworkCore;

namespace Finbuckle.MultiTenant.Identity.EntityFrameworkCore.Test;

public class TestIdentityDbContext : MultiTenantIdentityDbContext
{
    public TestIdentityDbContext()
    {
    }

    public TestIdentityDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("DataSource=:memory:");
        base.OnConfiguring(optionsBuilder);
    }
}