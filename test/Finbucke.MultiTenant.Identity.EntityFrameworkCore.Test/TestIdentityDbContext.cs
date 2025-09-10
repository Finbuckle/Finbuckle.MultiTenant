// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Finbucke.MultiTenant.Identity.EntityFrameworkCore.Test;

public class TestIdentityDbContext : MultiTenantIdentityDbContext
{
    public TestIdentityDbContext(IMultiTenantContextAccessor multiTenantContextAccessor) : base(
        multiTenantContextAccessor)
    {
    }

    public TestIdentityDbContext(IMultiTenantContextAccessor multiTenantContextAccessor, DbContextOptions options) :
        base(multiTenantContextAccessor, options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("DataSource=:memory:");
        base.OnConfiguring(optionsBuilder);
    }
}