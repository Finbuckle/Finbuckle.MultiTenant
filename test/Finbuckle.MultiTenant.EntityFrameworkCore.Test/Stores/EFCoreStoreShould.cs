//    Copyright 2018-2020 Andrew White
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

using System.Linq;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Internal;
using Finbuckle.MultiTenant.Stores;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace EFCoreStoreShould
{
    public class EFCoreStoreShould : IMultiTenantStoreTestBase<EFCoreStore<TestEFCoreStoreDbContext, TenantInfo>>
    {
        private static IProperty GetModelProperty(string propName)
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            var options = new DbContextOptionsBuilder()
                    .UseSqlite(connection)
                    .Options;
            var dbContext = new TestEFCoreStoreDbContext(options);

            var model = dbContext.Model.FindEntityType(typeof(TenantInfo));
            var prop = model.GetProperties().Where(p => p.Name == propName).SingleOrDefault();
            return prop;
        }

        protected override IMultiTenantStore<TenantInfo> CreateTestStore()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var options = new DbContextOptionsBuilder()
                    .UseSqlite(connection)
                    .Options;
            var dbContext = new TestEFCoreStoreDbContext(options);
            dbContext.Database.EnsureCreated();
            
            var store = new EFCoreStore<TestEFCoreStoreDbContext, TenantInfo>(dbContext);

            return PopulateTestStore(store);
        }

        protected override IMultiTenantStore<TenantInfo> PopulateTestStore(IMultiTenantStore<TenantInfo> store)
        {
            return base.PopulateTestStore(store);
        }

        [Fact]
        public void AddTenantIdLengthConstraint()
        {
            var prop = GetModelProperty("Id");
            Assert.Equal(Constants.TenantIdMaxLength, prop.GetMaxLength());
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
    }
}