// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;
using System.Data.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test
{
    public class MultiTenantIdentityDbContextShould : IDisposable
    {
        public class TestIdentityDbContext : MultiTenantIdentityDbContext
        {
            public TestIdentityDbContext(TenantInfo tenantInfo)
                : base(tenantInfo)
            {
            }

            public TestIdentityDbContext(TenantInfo tenantInfo, DbContextOptions options)
                : base(tenantInfo, options)
            {
            }
        }

        public class TestIdentityDbContextTUser : MultiTenantIdentityDbContext<IdentityUser>
        {
            public TestIdentityDbContextTUser(TenantInfo tenantInfo)
                : base(tenantInfo)
            {
            }

            public TestIdentityDbContextTUser(TenantInfo tenantInfo, DbContextOptions options)
                : base(tenantInfo, options)
            {
            }
        }

        public class TestIdentityDbContextTUserTRole : MultiTenantIdentityDbContext<IdentityUser, IdentityRole, string>
        {
            public TestIdentityDbContextTUserTRole(TenantInfo tenantInfo)
                : base(tenantInfo)
            {
            }

            public TestIdentityDbContextTUserTRole(TenantInfo tenantInfo, DbContextOptions options)
                : base(tenantInfo, options)
            {
            }
        }

        public class TestIdentityDbContextAll : MultiTenantIdentityDbContext<IdentityUser, IdentityRole, string, IdentityUserClaim<string>, IdentityUserRole<string>, IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>
        {

            public TestIdentityDbContextAll(TenantInfo tenantInfo)
                : base(tenantInfo)
            {
            }

            public TestIdentityDbContextAll(TenantInfo tenantInfo, DbContextOptions options)
                : base(tenantInfo, options)
            {
            }
        }
        
        private readonly DbContextOptions _options;
        private readonly DbConnection _connection = new SqliteConnection("DataSource=:memory:");

        public MultiTenantIdentityDbContextShould()
        {
            _connection.Open();
            _options = new DbContextOptionsBuilder()
                    .UseSqlite(_connection)
                    .Options;

        }

        public void Dispose()
        {
            _connection.Close();
        }

        [Fact]
        public void WorkWithSingleParamCtor()
        {
            var tenant1 = new TenantInfo
            {
                Id = "abc",
                Identifier = "abc",
                Name = "abc",
                ConnectionString = "DataSource=testdb.db"
            };
            var c = new TestIdentityDbContext(tenant1);

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
                ConnectionString = "DataSource=testdb.db"
            };
            var c = new TestIdentityDbContext(tenant1, _options);

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
                ConnectionString = "DataSource=testdb.db"
            };
            var c = new TestIdentityDbContextTUser(tenant1, _options);

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
                ConnectionString = "DataSource=testdb.db"
            };
            var c = new TestIdentityDbContextTUserTRole(tenant1, _options);

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
                ConnectionString = "DataSource=testdb.db"
            };
            var c = new TestIdentityDbContextAll(tenant1, _options);

            Assert.Equal(isMultiTenant, c.Model.FindEntityType(entityType).IsMultiTenant());
        }
    }
}