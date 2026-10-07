// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Finbuckle.MultiTenant.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Finbuckle.MultiTenant.Identity.EntityFrameworkCore.Test;

/// <summary>
/// A constructor-only <see cref="ITenantInfo"/> implementation (no setters, not derived from <see cref="TenantInfo"/>,
/// not a record) used to prove the Identity integration only depends on the interface contract.
/// </summary>
public sealed class ImmutableTenantInfo(string id, string identifier) : ITenantInfo
{
    public string Id { get; } = id;
    public string Identifier { get; } = identifier;
}

// Proves the Identity DbContext works with a constructor-only immutable tenant type.
public class ImmutableTenantIdentityDbContextShould : IDisposable
{
    private sealed class IdentityDbContext(IMultiTenantContextAccessor accessor, DbContextOptions options)
        : MultiTenantIdentityDbContext(accessor, options);

    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public void Dispose() => _connection.Dispose();

    [Fact]
    public void ResolveFromDependencyInjection()
    {
        var services = new ServiceCollection();
        var tenant = new ImmutableTenantInfo("abc", "abc");
        services.AddMultiTenant<ImmutableTenantInfo>();
        services.AddSingleton<IMultiTenantContextAccessor<ImmutableTenantInfo>>(
            new StaticMultiTenantContextAccessor<ImmutableTenantInfo>(tenant));
        services.AddDbContext<TestIdentityDbContext>();
        using var scope = services.BuildServiceProvider().CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<TestIdentityDbContext>();

        Assert.Same(tenant, db.TenantInfo);
    }

    [Fact]
    public void StampTenantIdOnIdentityEntities()
    {
        _connection.Open();
        var options = new DbContextOptionsBuilder().UseSqlite(_connection).Options;
        var accessor = new StaticMultiTenantContextAccessor<ImmutableTenantInfo>(new ImmutableTenantInfo("abc", "abc"));
        using var db = new IdentityDbContext(accessor, options);
        db.Database.EnsureCreated();

        var user = new IdentityUser { UserName = "user" };
        db.Users.Add(user);
        db.SaveChanges();

        Assert.Equal("abc", db.Entry(user).Property("TenantId").CurrentValue);
    }
}
