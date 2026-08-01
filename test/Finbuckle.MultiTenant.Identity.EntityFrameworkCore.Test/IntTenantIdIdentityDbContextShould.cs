// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Finbuckle.MultiTenant.Identity.EntityFrameworkCore.Test;

// Verifies the Identity DbContext configures its multi-tenant entities with a value-type (int) tenant id.
public class IntTenantIdIdentityDbContextShould : IDisposable
{
    private sealed class IntIdentityDbContext(DbContextOptions options)
        : MultiTenantIdentityDbContext<int>(options);

    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public void Dispose() => _connection.Dispose();

    [Fact]
    public void UseIntTenantIdShadowPropertyOnIdentityEntities()
    {
        _connection.Open();
        var options = new DbContextOptionsBuilder().UseSqlite(_connection).Options;
        using var db = new IntIdentityDbContext(options)
        {
            TenantInfo = new TenantInfo<int> { Id = 1, Identifier = "t" }
        };
        db.Database.EnsureCreated();

        var userTenantId = db.Model.FindEntityType(typeof(IdentityUser))!.FindProperty("TenantId")!;
        var roleTenantId = db.Model.FindEntityType(typeof(IdentityRole))!.FindProperty("TenantId")!;

        Assert.Equal(typeof(int), userTenantId.ClrType);
        Assert.Equal(typeof(int), roleTenantId.ClrType);
    }

    [Fact]
    public void RejectDefaultIdTenant()
    {
        _connection.Open();
        var options = new DbContextOptionsBuilder().UseSqlite(_connection).Options;
        using var db = new IntIdentityDbContext(options);

        Assert.Throws<MultiTenantException>(() =>
            db.TenantInfo = new TenantInfo<int> { Id = 0, Identifier = "x" });
    }
}
