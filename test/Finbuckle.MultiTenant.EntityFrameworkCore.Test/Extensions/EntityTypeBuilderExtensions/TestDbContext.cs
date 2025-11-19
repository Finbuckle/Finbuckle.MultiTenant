// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test.Extensions.EntityTypeBuilderExtensions;

public class TestDbContext(Action<ModelBuilder>? config, TenantInfo tenantInfo, DbContextOptions options)
    : EntityFrameworkCore.MultiTenantDbContext(new StaticMultiTenantContextAccessor<TenantInfo>(tenantInfo), options)
{
    public DbSet<MyMultiTenantThing>? MyMultiTenantThings { get; set; }
    public DbSet<MyThingWithTenantId>? MyThingsWithTenantIds { get; set; }
    public DbSet<MyThingWithIntTenantId>? MyThingsWithIntTenantId { get; set; }
    public DbSet<MyMultiTenantThingWithAttribute>? MyMultiTenantThingsWithAttribute { get; set; }
    public DbSet<MyNonMultiTenantThing>? MyNonMultiTenantThings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // if the test passed in a custom builder use it
        if (config != null)
            config(modelBuilder);
        // or use the standard builder configuration
        else
        {
            modelBuilder.Entity<MyMultiTenantThing>().IsMultiTenant();
            modelBuilder.Entity<MyThingWithTenantId>().IsMultiTenant();
        }

        base.OnModelCreating(modelBuilder);
    }
}

public class DynamicModelCacheKeyFactory : IModelCacheKeyFactory
{
    public object Create(DbContext context)
    {
        return new object();
    }

    public object Create(DbContext context, bool designTime)
    {
        return new object();
    }
}

public class MyMultiTenantThing
{
    public int Id { get; set; }
}

public class MyNonMultiTenantThing
{
    public int Id { get; set; }
}

[MultiTenant]
public class MyMultiTenantThingWithAttribute
{
    public int Id { get; set; }
}

public class MyThingWithTenantId
{
    public int Id { get; set; }
    public string? TenantId { get; set; }
}

public class MyThingWithIntTenantId
{
    public int Id { get; set; }
    public int TenantId { get; set; }
}