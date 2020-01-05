//    Copyright 2018 Andrew White
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
using Finbuckle.MultiTenant.Core;
using Finbuckle.MultiTenant.Stores;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace EFCoreStoreShould
{
    public class TestEFCoreStoreDbContext : EFCoreStoreDbContext<TestTenantInfoEntity>
    {
        public TestEFCoreStoreDbContext(DbContextOptions options) : base(options)
        {
        }
    }

    public class TestTenantInfoEntity : IEFCoreStoreTenantInfo
    {
        public string Id { get; set; }
        public string Identifier { get; set; }
        public string Name { get; set; }
        public string ConnectionString { get; set; }
    }

    public class EFCoreStoreShould : IMultiTenantStoreTestBase<EFCoreStore<TestEFCoreStoreDbContext, TestTenantInfoEntity>>
    {
        private static IProperty GetModelProperty(string propName)
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            var options = new DbContextOptionsBuilder()
                    .UseSqlite(connection)
                    .Options;
            var dbContext = new TestEFCoreStoreDbContext(options);

            var model = dbContext.Model.FindEntityType(typeof(TestTenantInfoEntity));
            var prop = model.GetProperties().Where(p => p.Name == propName).Single();
            return prop;
        }

        // Basic store functionality tested in MultiTenantStoresShould.cs

        protected override IMultiTenantStore CreateTestStore()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var options = new DbContextOptionsBuilder()
                    .UseSqlite(connection)
                    .Options;
            var dbContext = new TestEFCoreStoreDbContext(options);
            dbContext.Database.EnsureCreated();

            var store = new MultiTenantStoreWrapper<EFCoreStore<TestEFCoreStoreDbContext, TestTenantInfoEntity>>
                (new EFCoreStore<TestEFCoreStoreDbContext, TestTenantInfoEntity>(dbContext), null);

            return PopulateTestStore(store);
        }

        protected override IMultiTenantStore PopulateTestStore(IMultiTenantStore store)
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

        [Fact]
        public void AddNameRequiredConstraint()
        {
            var prop = GetModelProperty("Name");
            Assert.False(prop.IsNullable);
        }

        [Fact]
        public void AddConnectionStringRequiredConstraint()
        {
            var prop = GetModelProperty("ConnectionString");
            Assert.False(prop.IsNullable);
        }

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
        public override void ThrowWhenGettingByIdIfTenantIdIsNull()
        {
            base.ThrowWhenGettingByIdIfTenantIdIsNull();
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
        public override void ThrowWhenGettingByIdentifierIfTenantIdentifierIsNull()
        {
            base.ThrowWhenGettingByIdentifierIfTenantIdentifierIsNull();
        }

        [Fact]
        public override void AddTenantInfoToStore()
        {
            base.AddTenantInfoToStore();
        }

        [Fact]
        public override void ThrowWhenAddingIfTenantInfoIsNull()
        {
            base.ThrowWhenAddingIfTenantInfoIsNull();
        }

        [Fact]
        public override void ThrowWhenAddingIfTenantInfoIdIsNull()
        {
            base.ThrowWhenAddingIfTenantInfoIdIsNull();
        }

        [Fact]
        public override void ReturnFalseWhenAddingIfDuplicateId()
        {
            base.ReturnFalseWhenAddingIfDuplicateId();
        }

        [Fact]
        public override void ReturnFalseWhenAddingIfDuplicateIdentifier()
        {
            base.ReturnFalseWhenAddingIfDuplicateIdentifier();
        }

        [Fact]
        public override void ThrowWhenUpdatingIfTenantInfoIsNull()
        {
            base.ThrowWhenUpdatingIfTenantInfoIsNull();
        }

        [Fact]
        public override void ThrowWhenUpdatingIfTenantInfoIdIsNull()
        {
            base.ThrowWhenUpdatingIfTenantInfoIdIsNull();
        }

        [Fact]
        public override void ReturnFalseWhenUpdatingIfTenantIdIsNotFound()
        {
            base.ReturnFalseWhenUpdatingIfTenantIdIsNotFound();
        }

        [Fact]
        public override void RemoveTenantInfoFromStore()
        {
            base.RemoveTenantInfoFromStore();
        }

        [Fact]
        public override void ThrowWhenRemovingIfTenantIdentifierIsNull()
        {
            base.ThrowWhenRemovingIfTenantIdentifierIsNull();
        }

        [Fact]
        public override void ReturnFalseWhenRemovingIfTenantInfoNotFound()
        {
            base.ReturnFalseWhenRemovingIfTenantInfoNotFound();
        }

        [Theory]
        [InlineData("initech-id", true)]
        [InlineData("notFound", false)]
        public override void UpdateTenantInfoInStore(string id, bool expected)
        {
            base.UpdateTenantInfoInStore(id, expected);
        }
    }
}