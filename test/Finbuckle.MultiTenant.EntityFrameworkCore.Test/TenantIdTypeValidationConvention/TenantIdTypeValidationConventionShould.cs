// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;
using Finbuckle.MultiTenant.EntityFrameworkCore.Test.Extensions.EntityTypeBuilderExtensions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test.TenantIdTypeValidationConvention;

// TestDbContext is a MultiTenantDbContext<string>, which registers the convention.
public class TenantIdTypeValidationConventionShould : IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public TenantIdTypeValidationConventionShould() => _connection.Open();

    public void Dispose() => _connection.Dispose();

    private DbContextOptions Options() => new DbContextOptionsBuilder()
        .ReplaceService<IModelCacheKeyFactory, DynamicModelCacheKeyFactory>() // needed for testing only
        .UseSqlite(_connection)
        .Options;

    private TestDbContext GetDbContext(Action<ModelBuilder> config)
        => new(config, new TenantInfo { Id = "abc", Identifier = "abc" }, Options());

    [Fact]
    public void NameTheEntityTypeAndBothTenantIdTypesInTheMessage()
    {
        using var db = GetDbContext(b => b.Entity<MyBaseThing>().IsMultiTenant<Guid>());

        var ex = Assert.Throws<MultiTenantException>(() => db.Model);
        Assert.Contains($"'{nameof(MyBaseThing)}'", ex.Message);
        Assert.Contains(typeof(Guid).ToString(), ex.Message);
        Assert.Contains(typeof(string).ToString(), ex.Message);
    }

    [Fact]
    public void UseTheDisplayNameOfASharedTypeEntityInTheMessage()
    {
        using var db = GetDbContext(b => b.SharedTypeEntity<Dictionary<string, object>>("SharedThing", e =>
        {
            e.Property<int>("Id");
            e.IsMultiTenant<int>();
        }));

        var ex = Assert.Throws<MultiTenantException>(() => db.Model);
        Assert.Contains("'SharedThing", ex.Message);
        Assert.DoesNotContain("Dictionary`2", ex.Message);
    }

    [Fact]
    public void ThrowWhenOnlyOneOfSeveralEntityTypesHasADifferentTenantIdType()
    {
        using var db = GetDbContext(b =>
        {
            b.Entity<MyMultiTenantThing>().IsMultiTenant<string>();
            b.Entity<MyBaseThing>().IsMultiTenant<int>();
        });

        var ex = Assert.Throws<MultiTenantException>(() => db.Model);
        Assert.Contains($"'{nameof(MyBaseThing)}'", ex.Message);
        Assert.DoesNotContain(nameof(MyMultiTenantThing), ex.Message);
    }

    [Fact]
    public void ReportAMismatchOnTheBaseTypeRatherThanItsDerivedTypes()
    {
        using var db = GetDbContext(b =>
        {
            b.Entity<ZBaseThing>().IsMultiTenant<int>();
            b.Entity<ADerivedThing>();
        });

        var ex = Assert.Throws<MultiTenantException>(() => db.Model);
        Assert.Contains($"'{nameof(ZBaseThing)}'", ex.Message);
        Assert.DoesNotContain(nameof(ADerivedThing), ex.Message);
    }

    [Fact]
    public void ThrowForADerivedEntityTypeConfiguredWithADifferentTenantIdType()
    {
        // only the derived type is multi-tenant, so its TenantId is declared on the derived type
        using var db = GetDbContext(b =>
        {
            b.Entity<MyBaseThing>();
            b.Entity<MyDerivedThing>().IsMultiTenant<int>();
        });

        var ex = Assert.Throws<MultiTenantException>(() => db.Model);
        Assert.Contains($"'{nameof(MyDerivedThing)}'", ex.Message);
    }

    [Fact]
    public void IgnoreAnEntityTypeLaterMarkedNotMultiTenant()
    {
        // the int TenantId is a CLR property, so it remains after IsNotMultiTenant
        using var db = GetDbContext(b =>
        {
            b.Entity<MyThingWithIntTenantId>().IsMultiTenant<int>();
            b.Entity<MyThingWithIntTenantId>().IsNotMultiTenant();
        });

        var entityType = db.Model.FindEntityType(typeof(MyThingWithIntTenantId));
        Assert.NotNull(entityType);
        Assert.False(entityType!.IsMultiTenant);
        Assert.Equal(typeof(int), entityType.FindProperty("TenantId")!.ClrType);
    }

    [Fact]
    public void NotThrowForAnEntityConfiguredThroughTheAttributePath()
    {
        // [MultiTenant] types are configured with the context's own tenant id type, so they always match.
        using var db = GetDbContext(b => b.Entity<MyMultiTenantThingWithAttribute>());

        Assert.True(db.Model.FindEntityType(typeof(MyMultiTenantThingWithAttribute))!.IsMultiTenant);
    }

    [Fact]
    public void ThrowWhenTheTypedOverloadUsesGuidInAnIntContext()
    {
        using var db = new TenantIdTypeDbContext<int>(
            b => b.Entity<TenantIdTypeThing>().IsMultiTenant<TenantIdTypeThing, Guid>(), Options());

        var ex = Assert.Throws<MultiTenantException>(() => db.Model);
        Assert.Contains(typeof(Guid).ToString(), ex.Message);
        Assert.Contains(typeof(int).ToString(), ex.Message);
    }

    [Fact]
    public void ThrowWhenTheTypedOverloadUsesIntInAGuidContext()
    {
        using var db = new TenantIdTypeDbContext<Guid>(
            b => b.Entity<TenantIdTypeThing>().IsMultiTenant<TenantIdTypeThing, int>(), Options());

        var ex = Assert.Throws<MultiTenantException>(() => db.Model);
        Assert.Contains(typeof(int).ToString(), ex.Message);
        Assert.Contains(typeof(Guid).ToString(), ex.Message);
    }

    [Fact]
    public void BuildAndFilterWhenTheTypedOverloadMatchesAnIntContext()
        => AssertTypedOverloadFiltersByTenant(1, 2);

    [Fact]
    public void BuildAndFilterWhenTheTypedOverloadMatchesAGuidContext()
        => AssertTypedOverloadFiltersByTenant(Guid.NewGuid(), Guid.NewGuid());

    private void AssertTypedOverloadFiltersByTenant<TId>(TId tenant1Id, TId tenant2Id) where TId : IEquatable<TId>
    {
        var tenant1 = new TenantInfo<TId> { Id = tenant1Id, Identifier = "t1" };
        var tenant2 = new TenantInfo<TId> { Id = tenant2Id, Identifier = "t2" };

        using var db = new TenantIdTypeDbContext<TId>(
            b => b.Entity<TenantIdTypeThing>().IsMultiTenant<TenantIdTypeThing, TId>(), Options());
        db.TenantInfo = tenant1;
        db.Database.EnsureCreated();
        db.Things.Add(new TenantIdTypeThing { Id = 1 });
        db.SaveChanges();

        Assert.Equal(typeof(TId), db.Model.FindEntityType(typeof(TenantIdTypeThing))!.FindProperty("TenantId")!.ClrType);
        Assert.Equal(1, db.Things.Count());

        db.TenantInfo = tenant2;
        Assert.Equal(0, db.Things.Count());

        db.TenantInfo = null;
        Assert.Equal(0, db.Things.Count());
    }

    [Fact]
    public void BeSkippedWhenADerivedContextDoesNotCallBaseConfigureConventions()
    {
        // Documented behavior: overriding ConfigureConventions without calling the base implementation skips the check.
        using var db = new NoBaseConventionsDbContext(
            b => b.Entity<TenantIdTypeThing>().IsMultiTenant<int>(), Options());

        Assert.True(db.Model.FindEntityType(typeof(TenantIdTypeThing))!.IsMultiTenant);
    }

    [Fact]
    public void NotStoreTypeValuesInModelAnnotations()
    {
        // EF Core writes annotations into migration snapshots, where a System.Type value becomes typeof(...) code that
        // may not compile, so the tenant id type must not be recorded as an annotation.
        using var db = GetDbContext(b =>
        {
            b.Entity<MyMultiTenantThing>().IsMultiTenant<string>();
            b.Entity<MyThingWithTenantId>().IsMultiTenant<MyThingWithTenantId, string>();
            b.Entity<MyMultiTenantThingWithAttribute>();
        });

        var model = db.GetService<IDesignTimeModel>().Model;
        var annotatables = new List<IReadOnlyAnnotatable> { model };
        foreach (var entityType in model.GetEntityTypes())
        {
            annotatables.Add(entityType);
            annotatables.AddRange(entityType.GetDeclaredProperties());
            annotatables.AddRange(entityType.GetDeclaredKeys());
            annotatables.AddRange(entityType.GetDeclaredIndexes());
            annotatables.AddRange(entityType.GetDeclaredForeignKeys());
            annotatables.AddRange(entityType.GetDeclaredNavigations());
        }

        Assert.Contains(model.GetEntityTypes(), et => et.IsMultiTenant);
        Assert.DoesNotContain(annotatables.SelectMany(a => a.GetAnnotations()), a => a.Value is Type);
    }
}
