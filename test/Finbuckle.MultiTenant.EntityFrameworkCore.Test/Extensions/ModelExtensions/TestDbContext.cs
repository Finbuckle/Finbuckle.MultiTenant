// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Diagnostics.CodeAnalysis;
using Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test.Extensions.ModelExtensions;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
public class TestDbContext : DbContext
{
    DbSet<MyMultiTenantThing>? MyMultiTenantThings { get; set; }
    DbSet<MyThing>? MyThings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("DataSource=:memory:");
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MyMultiTenantThing>().IsMultiTenant();
    }
}

public class MyMultiTenantThing
{
    public int Id { get; set; }
}

public class MyThing
{
    public int Id { get; set; }
}