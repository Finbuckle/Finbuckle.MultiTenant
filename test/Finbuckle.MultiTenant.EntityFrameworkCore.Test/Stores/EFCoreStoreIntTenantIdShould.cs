// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore.Stores;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test.Stores;

// Verifies EFCoreStore works with a value-type (int) tenant id, including a real int Id column.
public class EFCoreStoreIntTenantIdShould : IDisposable
{
    private sealed class IntStoreDbContext(DbContextOptions options)
        : EFCoreStoreDbContext<TenantInfo<int>, int>(options);

    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public void Dispose() => _connection.Dispose();

    private async Task<EFCoreStore<IntStoreDbContext, TenantInfo<int>, int>> CreateStore()
    {
        _connection.Open();
        var options = new DbContextOptionsBuilder().UseSqlite(_connection).Options;
        var db = new IntStoreDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var store = new EFCoreStore<IntStoreDbContext, TenantInfo<int>, int>(db);
        await store.AddAsync(new TenantInfo<int> { Id = 1, Identifier = "initech" });
        await store.AddAsync(new TenantInfo<int> { Id = 2, Identifier = "lol" });
        return store;
    }

    [Fact]
    public void UseIntTypedIdColumn()
    {
        _connection.Open();
        var options = new DbContextOptionsBuilder().UseSqlite(_connection).Options;
        using var db = new IntStoreDbContext(options);

        var idProperty = db.Model.FindEntityType(typeof(TenantInfo<int>))!.FindProperty("Id")!;
        Assert.Equal(typeof(int), idProperty.ClrType);
        Assert.True(idProperty.IsPrimaryKey());
    }

    [Fact]
    public async Task RoundTripIntTenantId()
    {
        var store = await CreateStore();

        Assert.Equal("initech", (await store.GetAsync(1))!.Identifier);
        Assert.Equal(2, (await store.GetByIdentifierAsync("lol"))!.Id);

        Assert.True(await store.RemoveAsync(1));
        Assert.Null(await store.GetAsync(1));
        Assert.NotNull(await store.GetAsync(2));
    }
}
