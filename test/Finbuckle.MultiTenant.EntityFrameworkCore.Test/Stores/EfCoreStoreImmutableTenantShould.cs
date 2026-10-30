// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore.Stores;
using Finbuckle.MultiTenant.Test;
using Finbuckle.MultiTenant.Test.Stores;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test.Stores;

// Proves EFCoreStore persists and materializes a constructor-only immutable tenant type (EF Core constructor binding).
public class EfCoreStoreImmutableTenantShould : ImmutableTenantStoreTestBase, IDisposable
{
    private sealed class ImmutableStoreDbContext(DbContextOptions options)
        : EFCoreStoreDbContext<ImmutableTenantInfo>(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // EF Core does not discover getter-only properties by convention (only Id/Identifier are mapped by the
            // base context), so the extra constructor-bound property must be mapped explicitly.
            modelBuilder.Entity<ImmutableTenantInfo>().Property(t => t.Name);
        }
    }

    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public void Dispose() => _connection.Dispose();

    protected override async Task<IMultiTenantStore<ImmutableTenantInfo>> CreateTestStore()
    {
        _connection.Open();
        var options = new DbContextOptionsBuilder().UseSqlite(_connection).Options;
        var dbContext = new ImmutableStoreDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var store = new EFCoreStore<ImmutableStoreDbContext, ImmutableTenantInfo>(dbContext);
        return await PopulateTestStore(store);
    }

    [Fact]
    public async Task PersistAdditionalConstructorBoundProperties()
    {
        var store = await CreateTestStore();

        var tenant = await store.GetByIdentifierAsync("initech");

        Assert.NotNull(tenant);
        Assert.Equal("initech-id", tenant.Id);
        Assert.Equal("Initech", tenant.Name);
    }

    [Fact]
    public override Task GetTenantInfoFromStoreById() => base.GetTenantInfoFromStoreById();

    [Fact]
    public override Task ReturnNullWhenGettingByIdIfTenantInfoNotFound() => base.ReturnNullWhenGettingByIdIfTenantInfoNotFound();

    [Fact]
    public override Task GetTenantInfoFromStoreByIdentifier() => base.GetTenantInfoFromStoreByIdentifier();

    [Fact]
    public override Task ReturnNullWhenGettingByIdentifierIfTenantInfoNotFound() => base.ReturnNullWhenGettingByIdentifierIfTenantInfoNotFound();

    [Fact]
    public override Task AddTenantInfoToStore() => base.AddTenantInfoToStore();

    [Fact]
    public override Task UpdateTenantInfoInStore() => base.UpdateTenantInfoInStore();

    [Fact]
    public override Task RemoveTenantInfoFromStore() => base.RemoveTenantInfoFromStore();

    [Fact]
    public override Task GetAllTenantsFromStoreAsync() => base.GetAllTenantsFromStoreAsync();

    [Fact]
    public override Task GetAllTenantsFromStoreAsyncSkip1Take1() => base.GetAllTenantsFromStoreAsyncSkip1Take1();
}
