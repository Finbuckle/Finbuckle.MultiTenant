// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Diagnostics.CodeAnalysis;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test.Extensions.EntityTypeBuilderExtensions;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
public class TestDbContext : EntityFrameworkCore.MultiTenantDbContext
{
    private readonly Action<ModelBuilder>? _config;

    public TestDbContext(Action<ModelBuilder>? config, TenantInfo tenantInfo, DbContextOptions options) :
        base(new StaticMultiTenantContextAccessor<TenantInfo>(tenantInfo), options)
    {
        this._config = config;
    }

    public DbSet<MyMultiTenantThing>? MyMultiTenantThings { get; set; }
    public DbSet<MyThingWithTenantId>? MyThingsWithTenantIds { get; set; }
    public DbSet<MyThingWithIntTenantId>? MyThingsWithIntTenantId { get; set; }
    public DbSet<MyMultiTenantThingWithAttribute>? MyMultiTenantThingsWithAttribute { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // If the test passed in a custom builder use it
        if (_config != null)
            _config(modelBuilder);
        // Of use the standard builder configuration
        else
        {
            modelBuilder.Entity<MyMultiTenantThing>().IsMultiTenant();
            modelBuilder.Entity<MyThingWithTenantId>().IsMultiTenant();
        }

        base.OnModelCreating(modelBuilder);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
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

[MultiTenant]
// ReSharper disable once ClassNeverInstantiated.Global
// ReSharper disable once MemberCanBePrivate.Global
public class MyMultiTenantThingWithAttribute
{
    public int Id { get; set; }
}

// ReSharper disable once MemberCanBePrivate.Global
public class MyThingWithTenantId
{
    public int Id { get; set; }
    public string? TenantId { get; set; }
}

// ReSharper disable once MemberCanBePrivate.Global
// ReSharper disable once ClassNeverInstantiated.Global
public class MyThingWithIntTenantId
{
    public int Id { get; set; }
    public int TenantId { get; set; }
}