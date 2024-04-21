// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test.MultiTenantIdentityDbContext;

public class TestIdentityDbContext : EntityFrameworkCore.MultiTenantIdentityDbContext
{
    public TestIdentityDbContext(TenantInfo tenantInfo)
        : base(tenantInfo)
    {
    }

    public TestIdentityDbContext(IMultiTenantContextAccessor multiTenantContextAccessor) : base(multiTenantContextAccessor)
    {
    }

    public TestIdentityDbContext(ITenantInfo tenantInfo) : base(tenantInfo)
    {
    }

    public TestIdentityDbContext(IMultiTenantContextAccessor multiTenantContextAccessor, DbContextOptions options) : base(multiTenantContextAccessor, options)
    {
    }

    public TestIdentityDbContext(ITenantInfo tenantInfo, DbContextOptions options) : base(tenantInfo, options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("DataSource=:memory:");
        base.OnConfiguring(optionsBuilder);
    }
}