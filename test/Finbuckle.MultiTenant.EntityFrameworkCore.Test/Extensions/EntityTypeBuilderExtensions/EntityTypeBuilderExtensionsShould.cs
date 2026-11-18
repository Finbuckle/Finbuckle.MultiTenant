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
        return new TestDbContext(config, tenant ?? new TenantInfo { Id = "placeholder", Identifier = "placeholder" }, options);
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
        using var db = GetDbContext(b => b.Entity<MyThingWithIntTenantId>().IsMultiTenant<string>());
        Assert.Throws<MultiTenantException>(() => db.Model);
    }

    [Fact]
    public void SetNamedFilterQuery()
    {
        // Doesn't appear to be a way to test this except to try it out...
        var tenant1 = new TenantInfo { Id = "abc", Identifier = "abc" };

        var tenant2 = new TenantInfo { Id = "123", Identifier = "123" };

        using var db = GetDbContext(null, tenant1);
        db.Database.EnsureCreated();
        db.MyMultiTenantThings?.Add(new MyMultiTenantThing { Id = 1 });
        db.SaveChanges();

        Assert.Equal(1, db.MyMultiTenantThings!.Count());
        db.TenantInfo = tenant2;
        Assert.Equal(0, db.MyMultiTenantThings!.Count());
    }

    [Fact]
    public void SetNamedFilterQueryWithTypedBuilder()
    {
        var tenant1 = new TenantInfo { Id = "abc", Identifier = "abc" };
        var tenant2 = new TenantInfo { Id = "123", Identifier = "123" };

        using var db = GetDbContext(
            b => b.Entity<MyMultiTenantThing>().IsMultiTenant<MyMultiTenantThing, string>(),
            tenant1);
        db.Database.EnsureCreated();
        db.MyMultiTenantThings?.Add(new MyMultiTenantThing { Id = 1 });
        db.SaveChanges();

        Assert.Equal(1, db.MyMultiTenantThings!.Count());
        db.TenantInfo = tenant2;
        Assert.Equal(0, db.MyMultiTenantThings!.Count());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ReturnNoRecordsWhenQueryingWithNullTenantInfo(bool useTypedBuilder)
    {
        Action<ModelBuilder> config = useTypedBuilder
            ? b => b.Entity<MyMultiTenantThing>().IsMultiTenant<MyMultiTenantThing, string>()
            : b => b.Entity(typeof(MyMultiTenantThing)).IsMultiTenant<string>();
        using var db = GetDbContext(config, new TenantInfo { Id = "abc", Identifier = "abc" });
        db.Database.EnsureCreated();
        db.MyMultiTenantThings?.Add(new MyMultiTenantThing { Id = 1 });
        db.SaveChanges();
        Assert.Equal(1, db.MyMultiTenantThings!.Count()); // sanity check data exists

        db.TenantInfo = null;

        var result = db.MyMultiTenantThings!.ToList();
        Assert.Empty(result);
    }

    [Fact]
    public void CanIgnoreNamedFilterQuery()
    {
        // Doesn't appear to be a way to test this except to try it out...
        var tenant1 = new TenantInfo { Id = "abc", Identifier = "abc" };

        var tenant2 = new TenantInfo { Id = "123", Identifier = "123" };

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
            b.Entity<MyNonMultiTenantThing>().IsMultiTenant<string>();
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
            b.Entity<MyNonMultiTenantThing>().IsMultiTenant<string>();
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
            b.Entity<MyNonMultiTenantThing>().IsMultiTenant<string>();
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
        var tenant1 = new TenantInfo { Id = "abc", Identifier = "abc" };
        var tenant2 = new TenantInfo { Id = "123", Identifier = "123" };

        using var db = GetDbContext(b =>
        {
            b.Entity<MyMultiTenantThing>().IsMultiTenant<string>();
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

    [Fact]
    public void IsMultiTenantIsIdempotent()
    {
        // Calling IsMultiTenant<TId>() twice on the same entity should be a no-op the second time.
        using var db = GetDbContext(b =>
        {
            b.Entity<MyMultiTenantThing>().IsMultiTenant<string>();
            b.Entity<MyMultiTenantThing>().IsMultiTenant<string>();
        });

        var entityType = db.Model.FindEntityType(typeof(MyMultiTenantThing));
        Assert.NotNull(entityType);
        Assert.True(entityType!.IsMultiTenant);

        // TenantId shadow property should exist exactly once
        var tenantIdProps = entityType.GetProperties().Where(p => p.Name == "TenantId").ToList();
        Assert.Single(tenantIdProps);
    }

    [Fact]
    public void ThrowOnNonStringExistingTenantIdPropertyWithTypedBuilder()
    {
        using var db = GetDbContext(b =>
            b.Entity<MyThingWithIntTenantId>().IsMultiTenant<MyThingWithIntTenantId, string>());
        Assert.Throws<MultiTenantException>(() => db.Model);
    }

    [Fact]
    public void RemoveFilterAndShadowPropertyWhenIsNotMultiTenantFollowsTypedBuilder()
    {
        using var db = GetDbContext(b =>
        {
            b.Entity<MyNonMultiTenantThing>().IsMultiTenant<MyNonMultiTenantThing, string>();
            b.Entity<MyNonMultiTenantThing>().IsNotMultiTenant();
        });

        var entityType = db.Model.FindEntityType(typeof(MyNonMultiTenantThing));
        Assert.NotNull(entityType);
        Assert.False(entityType!.IsMultiTenant);
        Assert.Null(entityType.FindProperty("TenantId"));
        Assert.Null(entityType.FindDeclaredQueryFilter(Abstractions.Constants.TenantToken));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ApplyTheFilterToDerivedEntityTypes(bool useTypedBuilder)
    {
        var tenant1 = new TenantInfo { Id = "abc", Identifier = "abc" };
        var tenant2 = new TenantInfo { Id = "123", Identifier = "123" };

        Action<ModelBuilder> config = useTypedBuilder
            ? b =>
            {
                b.Entity<MyBaseThing>().IsMultiTenant<MyBaseThing, string>();
                b.Entity<MyDerivedThing>();
            }
            : b =>
            {
                b.Entity<MyBaseThing>().IsMultiTenant<string>();
                b.Entity<MyDerivedThing>();
            };

        using var db = GetDbContext(config, tenant1);
        Assert.True(db.Model.FindEntityType(typeof(MyDerivedThing))!.IsMultiTenant);

        db.Database.EnsureCreated();
        db.Set<MyDerivedThing>().Add(new MyDerivedThing { Id = 1 });
        db.SaveChanges();
        Assert.Equal(1, db.Set<MyDerivedThing>().Count());

        db.TenantInfo = tenant2;
        Assert.Equal(0, db.Set<MyDerivedThing>().Count());
        Assert.Equal(0, db.Set<MyBaseThing>().Count());

        db.TenantInfo = null;
        Assert.Empty(db.Set<MyDerivedThing>().ToList());
    }

    // The isolation tests below deliberately use the default model cache key, so every context of a given type shares
    // one cached model and one set of compiled queries. That proves the query filter reads the tenant of the context
    // that runs the query, rather than a value captured when the model or query was first built.

    private readonly TenantInfo _tenantA = new() { Id = "a", Identifier = "a" };
    private readonly TenantInfo _tenantB = new() { Id = "b", Identifier = "b" };

    public enum Builder
    {
        NonGeneric,
        Typed
    }

    private DbContextOptions<T> Options<T>() where T : DbContext
        => new DbContextOptionsBuilder<T>().UseSqlite(_connection).Options;

    private IsolationDbContextBase Create(Builder builder, TenantInfo? tenant)
        => builder == Builder.Typed
            ? new TypedIsolationDbContext(Options<TypedIsolationDbContext>()) { TenantInfo = tenant }
            : new NonGenericIsolationDbContext(Options<NonGenericIsolationDbContext>()) { TenantInfo = tenant };

    private void Seed(Builder builder)
    {
        using (var db = Create(builder, _tenantA))
        {
            db.Database.EnsureCreated();
            db.Things.AddRange(new IsolationThing { Id = 1 }, new IsolationThing { Id = 2 });
            db.SaveChanges();
        }

        using (var db = Create(builder, _tenantB))
        {
            db.Things.Add(new IsolationThing { Id = 3 });
            db.SaveChanges();
        }
    }

    private static int[] Ids(IsolationDbContextBase db) => db.Things.Select(t => t.Id).OrderBy(i => i).ToArray();

    [Theory]
    [InlineData(Builder.NonGeneric)]
    [InlineData(Builder.Typed)]
    public void ReturnOnlyTheCurrentTenantsRowsAcrossContextsSharingAModel(Builder builder)
    {
        Seed(builder);

        using (var db = Create(builder, _tenantA))
            Assert.Equal([1, 2], Ids(db));

        using (var db = Create(builder, _tenantB))
            Assert.Equal([3], Ids(db));
    }

    [Theory]
    [InlineData(Builder.NonGeneric)]
    [InlineData(Builder.Typed)]
    public void NotReuseAnEarlierTenantsValueWhenRepeatingTheSameQuery(Builder builder)
    {
        Seed(builder);

        // A, B, A with the identical query shape so a captured tenant value would be served from the query cache.
        foreach (var (tenant, expected) in new (TenantInfo, int[])[]
                 {
                     (_tenantA, [1, 2]), (_tenantB, [3]), (_tenantA, [1, 2]), (_tenantB, [3])
                 })
        {
            using var db = Create(builder, tenant);
            Assert.Equal(expected, Ids(db));
        }
    }

    [Theory]
    [InlineData(Builder.NonGeneric)]
    [InlineData(Builder.Typed)]
    public void IsolateContextsThatAreAliveAtTheSameTime(Builder builder)
    {
        Seed(builder);

        using var dbA = Create(builder, _tenantA);
        using var dbB = Create(builder, _tenantB);
        using var dbNone = Create(builder, null);

        // interleave the calls so no context is queried twice in a row
        Assert.Equal([1, 2], Ids(dbA));
        Assert.Equal([3], Ids(dbB));
        Assert.Empty(Ids(dbNone));
        Assert.Equal([3], Ids(dbB));
        Assert.Equal([1, 2], Ids(dbA));
    }

    [Theory]
    [InlineData(Builder.NonGeneric)]
    [InlineData(Builder.Typed)]
    public void FollowTenantInfoChangedOnAnExistingContext(Builder builder)
    {
        Seed(builder);

        using var db = Create(builder, _tenantA);
        Assert.Equal([1, 2], Ids(db));

        db.TenantInfo = _tenantB;
        Assert.Equal([3], Ids(db));

        db.TenantInfo = null;
        Assert.Empty(Ids(db));

        db.TenantInfo = _tenantA;
        Assert.Equal([1, 2], Ids(db));
    }

    [Theory]
    [InlineData(Builder.NonGeneric)]
    [InlineData(Builder.Typed)]
    public void ApplyTheFilterToCountAndAnyAsWellAsToList(Builder builder)
    {
        Seed(builder);

        using var dbA = Create(builder, _tenantA);
        using var dbB = Create(builder, _tenantB);
        using var dbNone = Create(builder, null);

        Assert.Equal(2, dbA.Things.Count());
        Assert.Equal(1, dbB.Things.Count());
        Assert.False(dbNone.Things.Any());
        Assert.False(dbB.Things.Any(t => t.Id == 1));
    }

    [Theory]
    [InlineData(Builder.NonGeneric)]
    [InlineData(Builder.Typed)]
    public void StillReturnAllRowsWhenTheTenantFilterIsIgnored(Builder builder)
    {
        Seed(builder);

        using var db = Create(builder, _tenantA);
        Assert.Equal(3, db.Things.IgnoreQueryFilters([Abstractions.Constants.TenantToken]).Count());
    }
}
