// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test.MultiTenantIdentityDbContext
{
    public class MultiTenantIdentityDbContextShould
    {
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
}