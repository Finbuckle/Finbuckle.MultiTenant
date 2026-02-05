// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test.Extensions.ModelBuilderExtensions;

public class TestDbContext : DbContext
{
    public DbSet<MyMultiTenantThing>? MyMultiTenantThings { get; set; }
    public DbSet<MyThing>? MyThings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("DataSource=:memory:");
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureMultiTenant();
    }
}

[MultiTenant]
public class MyMultiTenantThing
{
    public int Id { get; set; }
}

public class MyThing
{
    public int Id { get; set; }
}