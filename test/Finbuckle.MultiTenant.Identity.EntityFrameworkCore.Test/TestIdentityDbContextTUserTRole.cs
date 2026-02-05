// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Finbuckle.MultiTenant.Identity.EntityFrameworkCore.Test;

public class TestIdentityDbContextTUserTRole : MultiTenantIdentityDbContext<IdentityUser, IdentityRole, string>
{
    public TestIdentityDbContextTUserTRole(IMultiTenantContextAccessor multiTenantContextAccessor) : base(
        multiTenantContextAccessor)
    {
    }

    public TestIdentityDbContextTUserTRole(IMultiTenantContextAccessor multiTenantContextAccessor,
        DbContextOptions options) : base(multiTenantContextAccessor, options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("DataSource=:memory:");
        base.OnConfiguring(optionsBuilder);
    }
}