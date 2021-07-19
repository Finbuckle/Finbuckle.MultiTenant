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
using System.Linq;
using Finbuckle.MultiTenant.Stores;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test.Stores
{
    public class EfCoreStoreShould
        : IMultiTenantStoreTestBase<EFCoreStore<EfCoreStoreShould.TestEfCoreStoreDbContext, TenantInfo>>, IDisposable
    {
        public class TestEfCoreStoreDbContext : EFCoreStoreDbContext<TenantInfo>
        {
            public TestEfCoreStoreDbContext(DbContextOptions options) : base(options)
            {
            }
        }

        private readonly SqliteConnection _connection = new SqliteConnection("DataSource=:memory:");

        public void Dispose()
        {
            _connection.Dispose();
        }

        private IProperty GetModelProperty(string propName)
        {
            _connection.Open();
            var options = new DbContextOptionsBuilder().UseSqlite(_connection).Options;
            var dbContext = new TestEfCoreStoreDbContext(options);

            var model = dbContext.Model.FindEntityType(typeof(TenantInfo));
            var prop = model.GetProperties().SingleOrDefault(p => p.Name == propName);
            return prop;
        }

        protected override IMultiTenantStore<TenantInfo> CreateTestStore()
        {
            _connection.Open();
            var options = new DbContextOptionsBuilder().UseSqlite(_connection).Options;
            var dbContext = new TestEfCoreStoreDbContext(options);
            dbContext.Database.EnsureCreated();

            var store = new EFCoreStore<TestEfCoreStoreDbContext, TenantInfo>(dbContext);
            return PopulateTestStore(store);
        }

        // ReSharper disable once RedundantOverriddenMember
        protected override IMultiTenantStore<TenantInfo> PopulateTestStore(IMultiTenantStore<TenantInfo> store)
        {
            return base.PopulateTestStore(store);
        }

        [Fact]
        public void AddTenantIdLengthConstraint()
        {
            var prop = GetModelProperty("Id");
            Assert.Equal(Internal.Constants.TenantIdMaxLength, prop.GetMaxLength());
        }

        [Fact]
        public void AddTenantIdAsKey()
        {
            var prop = GetModelProperty("Id");
            Assert.True(prop.IsPrimaryKey());
        }

        [Fact]
        public void AddIdentifierUniqueConstraint()
        {
            var prop = GetModelProperty("Identifier");
            Assert.True(prop.IsIndex());
        }

        // Basic store functionality tested in MultiTenantStoresShould.cs

        [Fact]
        public override void GetTenantInfoFromStoreById()
        {
            base.GetTenantInfoFromStoreById();
        }

        [Fact]
        public override void ReturnNullWhenGettingByIdIfTenantInfoNotFound()
        {
            base.ReturnNullWhenGettingByIdIfTenantInfoNotFound();
        }

        [Fact]
        public override void GetTenantInfoFromStoreByIdentifier()
        {
            base.GetTenantInfoFromStoreByIdentifier();
        }

        [Fact]
        public override void ReturnNullWhenGettingByIdentifierIfTenantInfoNotFound()
        {
            base.ReturnNullWhenGettingByIdentifierIfTenantInfoNotFound();
        }

        [Fact]
        public override void AddTenantInfoToStore()
        {
            base.AddTenantInfoToStore();
        }

        [Fact]
        public override void RemoveTenantInfoFromStore()
        {
            base.RemoveTenantInfoFromStore();
        }

        [Fact]
        public override void UpdateTenantInfoInStore()
        {
            base.UpdateTenantInfoInStore();
        }

        [Fact]
        public override void GetAllTenantsFromStoreAsync()
        {
            base.GetAllTenantsFromStoreAsync();
        }
    }
}