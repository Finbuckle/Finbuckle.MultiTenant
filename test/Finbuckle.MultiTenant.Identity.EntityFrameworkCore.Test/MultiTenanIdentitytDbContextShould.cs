// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;
using Finbuckle.MultiTenant.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Finbuckle.MultiTenant.Identity.EntityFrameworkCore.Test;

public class MultiTenantIdentityDbContextShould
{
    private TContext CreateDbContextViaDi<TContext>(int schemaVersion) where TContext : DbContext, IMultiTenantDbContext
    {
        var services = new ServiceCollection();
        services.AddOptions();
        services.Configure<IdentityOptions>(o => o.Stores.SchemaVersion = new Version(schemaVersion, 0));
        var tenant = new TenantInfo { Id = "abc", Identifier = "abc" };
        services.AddMultiTenant<TenantInfo>()
            .WithStaticStrategy(tenant.Identifier)
            .WithInMemoryStore();
        services.AddMultiTenantDbContext<TContext>(o =>
        {
            o.UseSqlite("DataSource=:memory:");
            o.ReplaceService<IModelCacheKeyFactory, DynamicModelCacheKeyFactory>();
        });
        var sp = services.BuildServiceProvider();
        var scope = sp.CreateScope();
        scope.ServiceProvider.BeginTenantScope(tenant);
        return scope.ServiceProvider.GetRequiredService<TContext>();
    }

    [Fact]
    public void WorkWithDependencyInjection()
    {
        var services = new ServiceCollection();
        var tenant = new TenantInfo { Id = "abc", Identifier = "abc" };
        services.AddMultiTenant<TenantInfo>()
            .WithStaticStrategy(tenant.Identifier)
            .WithInMemoryStore();
        services.AddMultiTenantDbContext<TestIdentityDbContext>();
        var scope = services.BuildServiceProvider().CreateScope();
        scope.ServiceProvider.BeginTenantScope(tenant);

        var context = scope.ServiceProvider.GetService<TestIdentityDbContext>();
        Assert.NotNull(context);
    }

    [Fact]
    public void WorkWithSingleParamCtor()
    {
        var c = new TestIdentityDbContext();
        Assert.NotNull(c);
    }

    [Theory]
    [InlineData(typeof(IdentityUser))]
    [InlineData(typeof(IdentityRole))]
    [InlineData(typeof(IdentityUserClaim<string>))]
    [InlineData(typeof(IdentityUserRole<string>))]
    [InlineData(typeof(IdentityUserLogin<string>))]
    [InlineData(typeof(IdentityRoleClaim<string>))]
    [InlineData(typeof(IdentityUserToken<string>))]
    public void AdjustUniqueIndexes_DefaultSchema(Type entityType)
    {
        var c = CreateDbContextViaDi<TestIdentityDbContext>(2);
        var et = c.Model.FindEntityType(entityType);
        Assert.NotNull(et);
        foreach (var idx in et.GetIndexes().Where(i => i.IsUnique))
            Assert.Contains("TenantId", idx.Properties.Select(p => p.Name));
    }

    [Fact]
    public void AdjustUniqueIndexes_DefaultSchema_Passkey()
    {
        var c = CreateDbContextViaDi<TestIdentityDbContext>(2);
        var et = c.Model.FindEntityType(typeof(IdentityUserPasskey<string>));
        if (et == null) return; // Passkey not present for schema 2.
        // For schema 2 passkey should NOT be multi-tenant and unique indexes should not include TenantId.
        Assert.False(et.IsMultiTenant());
        var uniqueProps = et.GetIndexes().Where(i => i.IsUnique).SelectMany(i => i.Properties.Select(p => p.Name));
        Assert.DoesNotContain("TenantId", uniqueProps);
    }

    [Theory]
    [InlineData(typeof(IdentityUser))]
    [InlineData(typeof(IdentityRole))]
    [InlineData(typeof(IdentityUserClaim<string>))]
    [InlineData(typeof(IdentityUserRole<string>))]
    [InlineData(typeof(IdentityUserLogin<string>))]
    [InlineData(typeof(IdentityRoleClaim<string>))]
    [InlineData(typeof(IdentityUserToken<string>))]
    [InlineData(typeof(IdentityUserPasskey<string>))]
    public void AdjustUniqueIndexes_Schema3(Type entityType)
    {
        var c = CreateDbContextViaDi<TestIdentityDbContext>(3);
        var et = c.Model.FindEntityType(entityType);
        Assert.NotNull(et);
        foreach (var idx in et.GetIndexes().Where(i => i.IsUnique))
            Assert.Contains("TenantId", idx.Properties.Select(p => p.Name));
    }

    [Theory]
    [InlineData(typeof(IdentityUser))]
    [InlineData(typeof(IdentityRole))]
    [InlineData(typeof(IdentityUserClaim<string>))]
    [InlineData(typeof(IdentityUserRole<string>))]
    [InlineData(typeof(IdentityUserLogin<string>))]
    [InlineData(typeof(IdentityRoleClaim<string>))]
    [InlineData(typeof(IdentityUserToken<string>))]
    public void SetMultiTenantOnIdentityDbContextVariant_None_DefaultSchema(Type entityType)
    {
        var c = CreateDbContextViaDi<TestIdentityDbContext>(2);
        var et = c.Model.FindEntityType(entityType);
        Assert.NotNull(et);
        Assert.True(et.IsMultiTenant());
    }

    [Fact]
    public void SetMultiTenantOnIdentityDbContextVariant_None_DefaultSchema_Passkey()
    {
        var c = CreateDbContextViaDi<TestIdentityDbContext>(2);
        var et = c.Model.FindEntityType(typeof(IdentityUserPasskey<string>));
        if (et == null) return; // May not exist schema 2.
        Assert.False(et.IsMultiTenant());
    }

    [Theory]
    [InlineData(typeof(IdentityUser))]
    [InlineData(typeof(IdentityRole))]
    [InlineData(typeof(IdentityUserClaim<string>))]
    [InlineData(typeof(IdentityUserRole<string>))]
    [InlineData(typeof(IdentityUserLogin<string>))]
    [InlineData(typeof(IdentityRoleClaim<string>))]
    [InlineData(typeof(IdentityUserToken<string>))]
    [InlineData(typeof(IdentityUserPasskey<string>))]
    public void SetMultiTenantOnIdentityDbContextVariant_None_Schema3(Type entityType)
    {
        var c = CreateDbContextViaDi<TestIdentityDbContext>(3);
        var et = c.Model.FindEntityType(entityType);
        Assert.NotNull(et);
        Assert.True(et.IsMultiTenant());
    }

    [Theory]
    [InlineData(typeof(IdentityUser))]
    [InlineData(typeof(IdentityRole))]
    [InlineData(typeof(IdentityUserClaim<string>))]
    [InlineData(typeof(IdentityUserRole<string>))]
    [InlineData(typeof(IdentityUserLogin<string>))]
    [InlineData(typeof(IdentityRoleClaim<string>))]
    [InlineData(typeof(IdentityUserToken<string>))]
    public void SetMultiTenantOnIdentityDbContextVariant_TUser_DefaultSchema(Type entityType)
    {
        var c = CreateDbContextViaDi<TestIdentityDbContextTUser>(2);
        var et = c.Model.FindEntityType(entityType);
        Assert.NotNull(et);
        Assert.True(et.IsMultiTenant());
    }

    [Fact]
    public void SetMultiTenantOnIdentityDbContextVariant_TUser_DefaultSchema_Passkey()
    {
        var c = CreateDbContextViaDi<TestIdentityDbContextTUser>(2);
        var et = c.Model.FindEntityType(typeof(IdentityUserPasskey<string>));
        if (et == null) return;
        Assert.False(et.IsMultiTenant());
    }

    [Theory]
    [InlineData(typeof(IdentityUser))]
    [InlineData(typeof(IdentityRole))]
    [InlineData(typeof(IdentityUserClaim<string>))]
    [InlineData(typeof(IdentityUserRole<string>))]
    [InlineData(typeof(IdentityUserLogin<string>))]
    [InlineData(typeof(IdentityRoleClaim<string>))]
    [InlineData(typeof(IdentityUserToken<string>))]
    [InlineData(typeof(IdentityUserPasskey<string>))]
    public void SetMultiTenantOnIdentityDbContextVariant_TUser_Schema3(Type entityType)
    {
        var c = CreateDbContextViaDi<TestIdentityDbContextTUser>(3);
        var et = c.Model.FindEntityType(entityType);
        Assert.NotNull(et);
        Assert.True(et.IsMultiTenant());
    }

    [Theory]
    [InlineData(typeof(IdentityUser))]
    [InlineData(typeof(IdentityRole))]
    [InlineData(typeof(IdentityUserClaim<string>))]
    [InlineData(typeof(IdentityUserRole<string>))]
    [InlineData(typeof(IdentityUserLogin<string>))]
    [InlineData(typeof(IdentityRoleClaim<string>))]
    [InlineData(typeof(IdentityUserToken<string>))]
    public void SetMultiTenantOnIdentityDbContextVariant_TUser_TRole_DefaultSchema(Type entityType)
    {
        var c = CreateDbContextViaDi<TestIdentityDbContextTUserTRole>(2);
        var et = c.Model.FindEntityType(entityType);
        Assert.NotNull(et);
        Assert.True(et.IsMultiTenant());
    }

    [Fact]
    public void SetMultiTenantOnIdentityDbContextVariant_TUser_TRole_DefaultSchema_Passkey()
    {
        var c = CreateDbContextViaDi<TestIdentityDbContextTUserTRole>(2);
        var et = c.Model.FindEntityType(typeof(IdentityUserPasskey<string>));
        if (et == null) return;
        Assert.False(et.IsMultiTenant());
    }

    [Theory]
    [InlineData(typeof(IdentityUser))]
    [InlineData(typeof(IdentityRole))]
    [InlineData(typeof(IdentityUserClaim<string>))]
    [InlineData(typeof(IdentityUserRole<string>))]
    [InlineData(typeof(IdentityUserLogin<string>))]
    [InlineData(typeof(IdentityRoleClaim<string>))]
    [InlineData(typeof(IdentityUserToken<string>))]
    [InlineData(typeof(IdentityUserPasskey<string>))]
    public void SetMultiTenantOnIdentityDbContextVariant_TUser_TRole_Schema3(Type entityType)
    {
        var c = CreateDbContextViaDi<TestIdentityDbContextTUserTRole>(3);
        var et = c.Model.FindEntityType(entityType);
        Assert.NotNull(et);
        Assert.True(et.IsMultiTenant());
    }

    [Theory]
    [InlineData(typeof(IdentityUser))]
    [InlineData(typeof(IdentityRole))]
    [InlineData(typeof(IdentityUserClaim<string>))]
    [InlineData(typeof(IdentityUserRole<string>))]
    [InlineData(typeof(IdentityUserLogin<string>))]
    [InlineData(typeof(IdentityRoleClaim<string>))]
    [InlineData(typeof(IdentityUserToken<string>))]
    public void SetMultiTenantOnIdentityDbContextVariant_All_DefaultSchema(Type entityType)
    {
        var c = CreateDbContextViaDi<TestIdentityDbContextAll>(2);
        var et = c.Model.FindEntityType(entityType);
        Assert.NotNull(et);
        Assert.True(et.IsMultiTenant());
    }

    [Fact]
    public void SetMultiTenantOnIdentityDbContextVariant_All_DefaultSchema_Passkey()
    {
        var c = CreateDbContextViaDi<TestIdentityDbContextAll>(2);
        var et = c.Model.FindEntityType(typeof(IdentityUserPasskey<string>));
        if (et == null) return;
        Assert.False(et.IsMultiTenant());
    }

    [Theory]
    [InlineData(typeof(IdentityUser))]
    [InlineData(typeof(IdentityRole))]
    [InlineData(typeof(IdentityUserClaim<string>))]
    [InlineData(typeof(IdentityUserRole<string>))]
    [InlineData(typeof(IdentityUserLogin<string>))]
    [InlineData(typeof(IdentityRoleClaim<string>))]
    [InlineData(typeof(IdentityUserToken<string>))]
    [InlineData(typeof(IdentityUserPasskey<string>))]
    public void SetMultiTenantOnIdentityDbContextVariant_All_Schema3(Type entityType)
    {
        var c = CreateDbContextViaDi<TestIdentityDbContextAll>(3);
        var et = c.Model.FindEntityType(entityType);
        Assert.NotNull(et);
        Assert.True(et.IsMultiTenant());
    }

    [Fact]
    public void CreateMultiTenantIdentityDbContextWithFactory()
    {
        var tenant1 = new TenantInfo { Id = "abc", Identifier = "abc" };
        var c = MultiTenantDbContext.Create<MultiTenantIdentityDbContext, TenantInfo>(tenant1);

        Assert.NotNull(c);
    }

    [Fact]
    public void QueryFilterIsolatesUsersByTenant()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder().UseSqlite(connection).Options;

        var tenant1 = new TenantInfo { Id = "t1", Identifier = "t1" };
        var tenant2 = new TenantInfo { Id = "t2", Identifier = "t2" };

        using var setup = new MultiTenantIdentityDbContext(options);
        setup.TenantInfo = tenant1;
        setup.Database.EnsureCreated();
        setup.Users.Add(new IdentityUser { UserName = "tenant1-user", NormalizedUserName = "TENANT1-USER" });
        setup.SaveChanges();

        using var asT2 = new MultiTenantIdentityDbContext(options);
        asT2.TenantInfo = tenant2;
        Assert.Equal(0, asT2.Users.Count());

        using var asT1 = new MultiTenantIdentityDbContext(options);
        asT1.TenantInfo = tenant1;
        Assert.Equal(1, asT1.Users.Count());
    }

    [Fact]
    public void ThrowWhenTenantInfoIsNullAndIdentityEntitiesChanged()
    {
        using var db = new TestIdentityDbContext();
        db.Users.Add(new IdentityUser { UserName = "null-tenant-user", NormalizedUserName = "NULL-TENANT-USER" });

        Assert.Throws<MultiTenantException>(() => db.SaveChanges());
    }
}
