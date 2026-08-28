// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;
using Finbuckle.MultiTenant.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Finbuckle.MultiTenant.Identity.EntityFrameworkCore.Test;

// Proves the Identity DbContext works with a constructor-only immutable tenant type.
public class ImmutableTenantIdentityDbContextShould : IDisposable
{
    private sealed class IdentityDbContext(DbContextOptions options) : MultiTenantIdentityDbContext<string>(options);

    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public void Dispose() => _connection.Dispose();

    [Fact]
    public void ResolveFromAmbientTenantScope()
    {
        var services = new ServiceCollection();
        var tenant = new ImmutableTenantInfo("abc", "abc");
        services.AddMultiTenant<ImmutableTenantInfo, string>()
            .WithStaticStrategy(tenant.Identifier)
            .WithInMemoryStore();
        services.AddMultiTenantDbContext<TestIdentityDbContext, string>();
        using var scope = services.BuildServiceProvider().CreateScope();
        scope.ServiceProvider.BeginTenantScope(tenant);

        var db = scope.ServiceProvider.GetRequiredService<TestIdentityDbContext>();

        Assert.Same(tenant, db.TenantInfo);
    }

    [Fact]
    public void StampTenantIdOnIdentityEntities()
    {
        _connection.Open();
        var options = new DbContextOptionsBuilder().UseSqlite(_connection).Options;
        using var db = new IdentityDbContext(options) { TenantInfo = new ImmutableTenantInfo("abc", "abc") };
        db.Database.EnsureCreated();

        var user = new IdentityUser { UserName = "user" };
        db.Users.Add(user);
        db.SaveChanges();

        Assert.Equal("abc", db.Entry(user).Property("TenantId").CurrentValue);
    }
}
