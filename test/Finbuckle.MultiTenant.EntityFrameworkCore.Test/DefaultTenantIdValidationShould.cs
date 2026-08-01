// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore.Stores;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test;

// Verifies default(TId) is treated as an unset/invalid tenant in the EF Core components.
public class DefaultTenantIdValidationShould : IDisposable
{
    private sealed class IntDbContext(DbContextOptions options) : EntityFrameworkCore.MultiTenantDbContext<int>(options);

    private sealed class IntStoreDbContext(DbContextOptions options)
        : EFCoreStoreDbContext<TenantInfo<int>, int>(options);

    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public void Dispose() => _connection.Dispose();

    [Fact]
    public void MultiTenantDbContextRejectsDefaultIdTenant()
    {
        var options = new DbContextOptionsBuilder().UseSqlite("DataSource=:memory:").Options;
        using var db = new IntDbContext(options);

        Assert.Throws<MultiTenantException>(() =>
            db.TenantInfo = new TenantInfo<int> { Id = 0, Identifier = "x" });
    }

    [Fact]
    public async Task EFCoreStoreRejectsDefaultId()
    {
        _connection.Open();
        var options = new DbContextOptionsBuilder().UseSqlite(_connection).Options;
        var db = new IntStoreDbContext(options);
        await db.Database.EnsureCreatedAsync();
        var store = new EFCoreStore<IntStoreDbContext, TenantInfo<int>, int>(db);

        await Assert.ThrowsAsync<MultiTenantException>(() =>
            store.AddAsync(new TenantInfo<int> { Id = 0, Identifier = "x" }));
        await Assert.ThrowsAsync<ArgumentException>(() => store.GetAsync(0));
        await Assert.ThrowsAsync<ArgumentException>(() => store.RemoveAsync(0));
    }
}
