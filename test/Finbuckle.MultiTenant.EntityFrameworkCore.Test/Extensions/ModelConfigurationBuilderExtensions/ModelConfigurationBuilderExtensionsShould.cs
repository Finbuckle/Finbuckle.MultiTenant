// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;
using Finbuckle.MultiTenant.EntityFrameworkCore.Test.Extensions.EntityTypeBuilderExtensions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Xunit;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test.Extensions.ModelConfigurationBuilderExtensions;

public class ModelConfigurationBuilderExtensionsShould : IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public ModelConfigurationBuilderExtensionsShould() => _connection.Open();

    public void Dispose() => _connection.Dispose();

    private DirectMultiTenantDbContext GetDbContext(Action<ModelConfigurationBuilder>? conventions,
        Action<ModelBuilder> model)
    {
        var options = new DbContextOptionsBuilder()
            .ReplaceService<IModelCacheKeyFactory, DynamicModelCacheKeyFactory>() // needed for testing only
            .UseSqlite(_connection)
            .Options;
        return new DirectMultiTenantDbContext(conventions, model, options);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ThrowWhenBuildingModelIfTenantIdTypeDiffersFromContext(bool useTypedBuilder)
    {
        Action<ModelBuilder> model = useTypedBuilder
            ? b => b.Entity<DirectThing>().IsMultiTenant<DirectThing, int>()
            : b => b.Entity<DirectThing>().IsMultiTenant<int>();
        using var db = GetDbContext(c => c.ValidateMultiTenantIdType<string>(), model);

        Assert.Throws<MultiTenantException>(() => db.Model);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BuildModelWhenTenantIdTypeMatchesContext(bool useTypedBuilder)
    {
        Action<ModelBuilder> model = useTypedBuilder
            ? b => b.Entity<DirectThing>().IsMultiTenant<DirectThing, string>()
            : b => b.Entity<DirectThing>().IsMultiTenant<string>();
        using var db = GetDbContext(c => c.ValidateMultiTenantIdType<string>(), model);

        Assert.True(db.Model.FindEntityType(typeof(DirectThing))!.IsMultiTenant);
    }

    [Fact]
    public void NotValidateWhenNotRegistered()
    {
        using var db = GetDbContext(null, b => b.Entity<DirectThing>().IsMultiTenant<int>());

        Assert.True(db.Model.FindEntityType(typeof(DirectThing))!.IsMultiTenant);
    }

    [Fact]
    public void IgnoreEntityTypesThatAreNotMultiTenant()
    {
        // the entity has its own int TenantId property but is not multi-tenant, so it is not checked
        using var db = GetDbContext(c => c.ValidateMultiTenantIdType<string>(),
            b => b.Entity<DirectThingWithIntTenantId>());

        var entityType = db.Model.FindEntityType(typeof(DirectThingWithIntTenantId));
        Assert.NotNull(entityType);
        Assert.False(entityType!.IsMultiTenant);
        Assert.Equal(typeof(int), entityType.FindProperty("TenantId")!.ClrType);
    }

    [Fact]
    public void ReturnTheSameBuilder()
    {
        ModelConfigurationBuilder? returned = null;
        ModelConfigurationBuilder? passed = null;
        using var db = GetDbContext(c =>
        {
            passed = c;
            returned = c.ValidateMultiTenantIdType<string>();
        }, b => b.Entity<DirectThing>());

        _ = db.Model;

        Assert.NotNull(passed);
        Assert.Same(passed, returned);
    }

    [Fact]
    public void ThrowOnNullBuilder()
    {
        Assert.Throws<ArgumentNullException>(() =>
            ((ModelConfigurationBuilder)null!).ValidateMultiTenantIdType<string>());
    }
}
