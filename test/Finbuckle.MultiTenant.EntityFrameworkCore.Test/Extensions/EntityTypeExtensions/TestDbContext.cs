// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test.Extensions.EntityTypeExtensions;

public class TestDbContext : DbContext
{
    // ReSharper disable once MemberHidesStaticFromOuterClass
    // ReSharper disable once UnusedMember.Local
    DbSet<MyMultiTenantThing>? MyMultiTenantThing { get; set; }

    // ReSharper disable once MemberHidesStaticFromOuterClass
    // ReSharper disable once UnusedMember.Local
    DbSet<MyThing>? MyThing { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("DataSource=:memory:");
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MyMultiTenantThing>().IsMultiTenant();
        modelBuilder.Entity<MyMultiTenantChildThing>();
    }
}

public class MyThing
{
    public int Id { get; set; }
}

public class MyMultiTenantThing
{
    public int Id { get; set; }
}

public class MyMultiTenantChildThing : MyMultiTenantThing
{
}