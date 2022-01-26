// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;
using System.Linq;
using Finbuckle.MultiTenant.Stores;
using Finbuckle.MultiTenant.Test.Stores;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test.Stores
{
    public class EfCoreStoreShould
        : MultiTenantStoreTestBase, IDisposable
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

        private IProperty? GetModelProperty(string propName)
        {
            _connection.Open();
            var options = new DbContextOptionsBuilder().UseSqlite(_connection).Options;
            var dbContext = new TestEfCoreStoreDbContext(options);

            var model = dbContext.Model.FindEntityType(typeof(TenantInfo));
            var prop = model?.GetProperties().SingleOrDefault(p => p.Name == propName);
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
            Assert.Equal(Internal.Constants.TenantIdMaxLength, prop!.GetMaxLength());
        }

        [Fact]
        public void AddTenantIdAsKey()
        {
            var prop = GetModelProperty("Id");
            Assert.True(prop!.IsPrimaryKey());
        }

        [Fact]
        public void AddIdentifierUniqueConstraint()
        {
            var prop = GetModelProperty("Identifier");
            Assert.True(prop!.IsIndex());
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