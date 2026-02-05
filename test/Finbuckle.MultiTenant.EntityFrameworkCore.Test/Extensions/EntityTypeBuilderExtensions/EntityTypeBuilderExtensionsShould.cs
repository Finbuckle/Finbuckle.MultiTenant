// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Linq;
using Xunit;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test.Extensions.EntityTypeBuilderExtensions;

public class EntityTypeBuilderExtensionsShould : IDisposable
{
    private readonly SqliteConnection _connection;

    public EntityTypeBuilderExtensionsShould()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    public void Dispose()
    {
        _connection.Dispose();
    }

    private TestDbContext GetDbContext(Action<ModelBuilder>? config = null, TenantInfo? tenant = null)
    {
        var options = new DbContextOptionsBuilder()
            .ReplaceService<IModelCacheKeyFactory, DynamicModelCacheKeyFactory>() // needed for testing only
            .UseSqlite(_connection)
            .Options;
        return new TestDbContext(config, tenant ?? new TenantInfo { Id = "", Identifier = "" }, options);
    }

    [Fact]
    public void SetMultiTenantAnnotation()
    {
        using var db = GetDbContext();
        var annotation = db.Model.FindEntityType(typeof(MyMultiTenantThing))?
            .FindAnnotation(Constants.MultiTenantAnnotationName);

        Assert.True((bool)annotation!.Value!);
    }

    [Fact]
    public void AddTenantIdStringShadowProperty()
    {
        using var db = GetDbContext();
        var prop = db.Model.FindEntityType(typeof(MyMultiTenantThing))?.FindProperty("TenantId");

        Assert.Equal(typeof(string), prop?.ClrType);
        Assert.True(prop?.IsShadowProperty());
        Assert.Null(prop?.FieldInfo);
    }

    [Fact]
    public void RespectExistingTenantIdStringProperty()
    {
        using var db = GetDbContext();
        var prop = db.Model.FindEntityType(typeof(MyThingWithTenantId))?.FindProperty("TenantId");

        Assert.Equal(typeof(string), prop!.ClrType);
        Assert.False(prop.IsShadowProperty());
        Assert.NotNull(prop.FieldInfo);
    }

    [Fact]
    public void ThrowOnNonStringExistingTenantIdProperty()
    {
        using var db = GetDbContext(b => b.Entity<MyThingWithIntTenantId>().IsMultiTenant());
        Assert.Throws<MultiTenantException>(() => db.Model);
    }

    [Fact]
    public void SetNamedFilterQuery()
    {
        // Doesn't appear to be a way to test this except to try it out...
        var tenant1 = new TenantInfo { Id = "abc", Identifier = "" };

        var tenant2 = new TenantInfo { Id = "123", Identifier = "" };

        using var db = GetDbContext(null, tenant1);
        db.Database.EnsureCreated();
        db.MyMultiTenantThings?.Add(new MyMultiTenantThing { Id = 1 });
        db.SaveChanges();

        Assert.Equal(1, db.MyMultiTenantThings!.Count());
        db.TenantInfo = tenant2;
        Assert.Equal(0, db.MyMultiTenantThings!.Count());
    }

    [Fact]
    public void CanIgnoreNamedFilterQuery()
    {
        // Doesn't appear to be a way to test this except to try it out...
        var tenant1 = new TenantInfo { Id = "abc", Identifier = "" };

        var tenant2 = new TenantInfo { Id = "123", Identifier = "" };

        using var db = GetDbContext(null, tenant1);
        db.Database.EnsureCreated();
        db.MyMultiTenantThings?.Add(new MyMultiTenantThing { Id = 1 });
        db.SaveChanges();

        db.TenantInfo = tenant2;
        db.MyMultiTenantThings?.Add(new MyMultiTenantThing { Id = 2 });
        db.SaveChanges();

        Assert.Equal(2, db.MyMultiTenantThings!.IgnoreQueryFilters([Abstractions.Constants.TenantToken]).Count());
    }

    [Fact]
    public void UnSetMultiTenantAnnotationOnIsNotMultiTenant()
    {
        using var db = GetDbContext(b =>
        { 
            b.Entity<MyNonMultiTenantThing>().IsMultiTenant();
            b.Entity<MyNonMultiTenantThing>().IsNotMultiTenant();
        });
        var annotation = db.Model.FindEntityType(typeof(MyNonMultiTenantThing))?
            .FindAnnotation(Constants.MultiTenantAnnotationName);

        Assert.False((bool)annotation!.Value!);
    }
    
    [Fact]
    public void RemoveShadowTenantIdPropertyForNonMultiTenantEntity()
    {
        using var db = GetDbContext(b =>
        { 
            b.Entity<MyNonMultiTenantThing>().IsMultiTenant();
            b.Entity<MyNonMultiTenantThing>().IsNotMultiTenant();
        });
        var prop = db.Model.FindEntityType(typeof(MyNonMultiTenantThing))?.FindProperty("TenantId");

        Assert.Null(prop);
    }

    [Fact]
    public void RemoveNamedTenantFilterForNonMultiTenantEntity()
    {
        // First mark it as multi-tenant, then mark it as not multi-tenant
        using var db = GetDbContext(b =>
        {
            b.Entity<MyNonMultiTenantThing>().IsMultiTenant();
            b.Entity<MyNonMultiTenantThing>().IsNotMultiTenant();
        });
        
        var entityType = db.Model.FindEntityType(typeof(MyNonMultiTenantThing));
        var filter = entityType?.FindDeclaredQueryFilter(Abstractions.Constants.TenantToken);

        // The filter should be set to always return true (effectively removing tenant filtering)
        Assert.Null(filter);
    }

    [Fact]
    public void NotFilterNonMultiTenantEntity()
    {
        var tenant1 = new TenantInfo { Id = "abc", Identifier = "" };
        var tenant2 = new TenantInfo { Id = "123", Identifier = "" };

        using var db = GetDbContext(b =>
        {
            b.Entity<MyMultiTenantThing>().IsMultiTenant();
            b.Entity<MyNonMultiTenantThing>().IsNotMultiTenant();
        }, tenant1);
        
        db.Database.EnsureCreated();
        
        // Add a multi-tenant entity
        db.MyMultiTenantThings?.Add(new MyMultiTenantThing { Id = 1 });
        // Add a non-multi-tenant entity
        db.MyNonMultiTenantThings?.Add(new MyNonMultiTenantThing { Id = 1 });
        db.SaveChanges();

        // Switch to tenant2
        db.TenantInfo = tenant2;
        
        // Add another non-multi-tenant entity under tenant2
        db.MyNonMultiTenantThings?.Add(new MyNonMultiTenantThing { Id = 2 });
        db.SaveChanges();

        // Multi-tenant entities should be filtered (0 for tenant2)
        Assert.Equal(0, db.MyMultiTenantThings!.Count());
        
        // Non-multi-tenant entities should not be filtered (both should be visible)
        Assert.Equal(2, db.MyNonMultiTenantThings!.Count());
    }
}