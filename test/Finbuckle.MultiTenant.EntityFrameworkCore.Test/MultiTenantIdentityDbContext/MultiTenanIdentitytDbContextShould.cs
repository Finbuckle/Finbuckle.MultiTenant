// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test.MultiTenantIdentityDbContext;

public class MultiTenantIdentityDbContextShould
{
    [Fact]
    public void WorkWithDependencyInjection()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>();
        services.AddDbContext<TestIdentityDbContext>();
        var scope = services.BuildServiceProvider().CreateScope();
            
        var context = scope.ServiceProvider.GetService<TestIdentityDbContext>();
        Assert.NotNull(context);
    }
    
    [Fact]
    public void WorkWithSingleParamCtor()
    {
        var tenant1 = new TenantInfo
        {
            Id = "abc",
            Identifier = "abc",
            Name = "abc",
            ConnectionString = "DataSource=testDb.db"
        };
        using var c = new TestIdentityDbContext(tenant1);

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
    public void AdjustUniqueIndexes(Type entityType)
    {
        var tenant1 = new TenantInfo
        {
            Id = "abc",
            Identifier = "abc",
            Name = "abc",
            ConnectionString = "DataSource=testDb.db"
        };
        using var c = new TestIdentityDbContext(tenant1);

        foreach (var index in c.Model.FindEntityType(entityType)!.GetIndexes().Where(i => i.IsUnique))
        {
            var props = index.Properties.Select(p => p.Name);
            Assert.Contains("TenantId", props);
        }
    }
    
    [Theory]
    // [InlineData(typeof(IdentityUser))]
    // [InlineData(typeof(IdentityRole))]
    // [InlineData(typeof(IdentityUserClaim<string>))]
    // [InlineData(typeof(IdentityUserRole<string>))]
    [InlineData(typeof(IdentityUserLogin<string>))]
    // [InlineData(typeof(IdentityRoleClaim<string>))]
    // [InlineData(typeof(IdentityUserToken<string>))]
    public void AdjustPrimaryKeys(Type entityType)
    {
        var tenant1 = new TenantInfo
        {
            Id = "abc",
            Identifier = "abc",
            Name = "abc",
            ConnectionString = "DataSource=testDb.db"
        };
        using var c = new TestIdentityDbContext(tenant1);

        foreach (var key in c.Model.FindEntityType(entityType)!.GetKeys())
        {
            var props = key.Properties.Select(p => p.Name);
            Assert.Contains("TenantId", props);
        }
    }

    [Theory]
    [InlineData(typeof(IdentityUser), true)]
    [InlineData(typeof(IdentityRole), true)]
    [InlineData(typeof(IdentityUserClaim<string>), true)]
    [InlineData(typeof(IdentityUserRole<string>), true)]
    [InlineData(typeof(IdentityUserLogin<string>), true)]
    [InlineData(typeof(IdentityRoleClaim<string>), true)]
    [InlineData(typeof(IdentityUserToken<string>), true)]
    public void SetMultiTenantOnIdentityDbContextVariant_None(Type entityType, bool isMultiTenant)
    {
        var tenant1 = new TenantInfo
        {
            Id = "abc",
            Identifier = "abc",
            Name = "abc",
            ConnectionString = "DataSource=testDb.db"
        };
        using var c = new TestIdentityDbContext(tenant1);

        Assert.Equal(isMultiTenant, c.Model.FindEntityType(entityType).IsMultiTenant());
    }

    [Theory]
    [InlineData(typeof(IdentityUser), false)]
    [InlineData(typeof(IdentityRole), true)]
    [InlineData(typeof(IdentityUserClaim<string>), true)]
    [InlineData(typeof(IdentityUserRole<string>), true)]
    [InlineData(typeof(IdentityUserLogin<string>), true)]
    [InlineData(typeof(IdentityRoleClaim<string>), true)]
    [InlineData(typeof(IdentityUserToken<string>), true)]
    public void SetMultiTenantOnIdentityDbContextVariant_TUser(Type entityType, bool isMultiTenant)
    {
        var tenant1 = new TenantInfo
        {
            Id = "abc",
            Identifier = "abc",
            Name = "abc",
            ConnectionString = "DataSource=testDb.db"
        };
        using var c = new TestIdentityDbContextTUser(tenant1);

        Assert.Equal(isMultiTenant, c.Model.FindEntityType(entityType).IsMultiTenant());
    }

    [Theory]
    [InlineData(typeof(IdentityUser), false)]
    [InlineData(typeof(IdentityRole), false)]
    [InlineData(typeof(IdentityUserClaim<string>), true)]
    [InlineData(typeof(IdentityUserRole<string>), true)]
    [InlineData(typeof(IdentityUserLogin<string>), true)]
    [InlineData(typeof(IdentityRoleClaim<string>), true)]
    [InlineData(typeof(IdentityUserToken<string>), true)]
    public void SetMultiTenantOnIdentityDbContextVariant_TUser_TRole(Type entityType, bool isMultiTenant)
    {
        var tenant1 = new TenantInfo
        {
            Id = "abc",
            Identifier = "abc",
            Name = "abc",
            ConnectionString = "DataSource=testDb.db"
        };
        using var c = new TestIdentityDbContextTUserTRole(tenant1);

        Assert.Equal(isMultiTenant, c.Model.FindEntityType(entityType).IsMultiTenant());
    }

    [Theory]
    [InlineData(typeof(IdentityUser), false)]
    [InlineData(typeof(IdentityRole), false)]
    [InlineData(typeof(IdentityUserClaim<string>), false)]
    [InlineData(typeof(IdentityUserRole<string>), false)]
    [InlineData(typeof(IdentityUserLogin<string>), false)]
    [InlineData(typeof(IdentityRoleClaim<string>), false)]
    [InlineData(typeof(IdentityUserToken<string>), false)]
    public void SetMultiTenantOnIdentityDbContextVariant_All(Type entityType, bool isMultiTenant)
    {
        var tenant1 = new TenantInfo
        {
            Id = "abc",
            Identifier = "abc",
            Name = "abc",
            ConnectionString = "DataSource=testDb.db"
        };
        using var c = new TestIdentityDbContextAll(tenant1);

        Assert.Equal(isMultiTenant, c.Model.FindEntityType(entityType).IsMultiTenant());
    }
}