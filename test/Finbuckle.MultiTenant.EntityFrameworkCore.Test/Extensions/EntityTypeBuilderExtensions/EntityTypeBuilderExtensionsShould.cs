// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
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
        return new TestDbContext(config, tenant ?? new TenantInfo("", ""), options);
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
        var tenant1 = new TenantInfo(Id: "abc", "");

        var tenant2 = new TenantInfo(Id: "123", "");

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
        var tenant1 = new TenantInfo(Id: "abc", "");

        var tenant2 = new TenantInfo(Id: "123", "");

        using var db = GetDbContext(null, tenant1);
        db.Database.EnsureCreated();
        db.MyMultiTenantThings?.Add(new MyMultiTenantThing { Id = 1 });
        db.SaveChanges();

        db.TenantInfo = tenant2;
        db.MyMultiTenantThings?.Add(new MyMultiTenantThing { Id = 2 });
        db.SaveChanges();
        
        Assert.Equal(2, db.MyMultiTenantThings!.IgnoreQueryFilters([Abstractions.Constants.TenantToken]).Count());
    }
}