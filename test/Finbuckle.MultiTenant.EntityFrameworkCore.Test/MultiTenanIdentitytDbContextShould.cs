//    Copyright 2018-2020 Finbuckle LLC, Andrew White, and Contributors
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace MultiTenantIdentityDbContextShould
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

    public class TestIdentityDbContext_TUser : MultiTenantIdentityDbContext<IdentityUser>
    {
        public TestIdentityDbContext_TUser(TenantInfo tenantInfo)
            : base(tenantInfo)
        {
        }

        public TestIdentityDbContext_TUser(TenantInfo tenantInfo, DbContextOptions options)
            : base(tenantInfo, options)
        {
        }
    }

    public class TestIdentityDbContext_TUser_TRole : MultiTenantIdentityDbContext<IdentityUser, IdentityRole, string>
    {
        public TestIdentityDbContext_TUser_TRole(TenantInfo tenantInfo)
            : base(tenantInfo)
        {
        }

        public TestIdentityDbContext_TUser_TRole(TenantInfo tenantInfo, DbContextOptions options)
            : base(tenantInfo, options)
        {
        }
    }

    public class TestIdentityDbContext_All : MultiTenantIdentityDbContext<IdentityUser, IdentityRole, string, IdentityUserClaim<string>, IdentityUserRole<string>, IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>
    {

        public TestIdentityDbContext_All(TenantInfo tenantInfo)
            : base(tenantInfo)
        {
        }

        public TestIdentityDbContext_All(TenantInfo tenantInfo, DbContextOptions options)
            : base(tenantInfo, options)
        {
        }
    }

    public class MultiTenantIdentityDbContextShould
    {
        private DbContextOptions _options;
        private DbConnection _connection;

        public MultiTenantIdentityDbContextShould()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _options = new DbContextOptionsBuilder()
                    .UseSqlite(_connection)
                    .Options;

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
            var multitenantEntities = c.Model.GetEntityTypes().Where(et => et.IsMultiTenant()).Select(et => et.ClrType).ToList();

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
            var c = new TestIdentityDbContext_TUser(tenant1, _options);
            var multitenantEntities = c.Model.GetEntityTypes().Where(et => et.IsMultiTenant()).Select(et => et.ClrType).ToList();

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
            var c = new TestIdentityDbContext_TUser_TRole(tenant1, _options);
            var multitenantEntities = c.Model.GetEntityTypes().Where(et => et.IsMultiTenant()).Select(et => et.ClrType).ToList();

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
            var c = new TestIdentityDbContext_All(tenant1, _options);
            var multitenantEntities = c.Model.GetEntityTypes().Where(et => et.IsMultiTenant()).Select(et => et.ClrType).ToList();

            Assert.Equal(isMultiTenant, c.Model.FindEntityType(entityType).IsMultiTenant());
        }
    }
}