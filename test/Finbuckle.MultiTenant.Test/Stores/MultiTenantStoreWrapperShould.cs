// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;
using Finbuckle.MultiTenant.Stores;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Finbuckle.MultiTenant.Test.Stores
{
    public class MultiTenantStoreWrappperShould : MultiTenantStoreTestBase
    {
        // Basic store functionality tested in MultiTenantStoresShould.cs

        protected override IMultiTenantStore<TenantInfo> CreateTestStore()
        {
            var store = new MultiTenantStoreWrapper<TenantInfo>(new InMemoryStore<TenantInfo>(null!), NullLogger.Instance);

            return PopulateTestStore(store);
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
        public void ThrowWhenGettingByIdIfTenantIdIsNull()
        {
            var store = CreateTestStore();

            var e = Assert.Throws<AggregateException>(() => store.TryGetAsync(null!).Result);
            Assert.IsType<ArgumentNullException>(e.InnerException);
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
        public void ThrowWhenGettingByIdentifierIfTenantIdentifierIsNull()
        {
            var store = CreateTestStore();

            var e = Assert.Throws<AggregateException>(() => store.TryGetByIdentifierAsync(null!).Result);
            Assert.IsType<ArgumentNullException>(e.InnerException);
        }

        [Fact]
        public override void AddTenantInfoToStore()
        {
            base.AddTenantInfoToStore();
        }

        [Fact]
        public void ThrowWhenAddingIfTenantInfoIsNull()
        {
            var store = CreateTestStore();
            var e = Assert.Throws<AggregateException>(() => store.TryAddAsync(null!).Result);
            Assert.IsType<ArgumentNullException>(e.InnerException);
        }

        [Fact]
        public void ThrowWhenAddingIfTenantInfoIdIsNull()
        {
            var store = CreateTestStore();
            var e = Assert.Throws<AggregateException>(() => store.TryAddAsync(new TenantInfo()).Result);
            Assert.IsType<ArgumentNullException>(e.InnerException);
        }

        [Fact]
        public void ThrowWhenAddingIfTenantInfoIdentifierIsNull()
        {
            var store = CreateTestStore();
            var e = Assert.Throws<AggregateException>(() => store.TryAddAsync(new TenantInfo() { Id = "inittech-id" }).Result);
            Assert.IsType<ArgumentNullException>(e.InnerException);
        }

        [Fact]
        public void ReturnFalseWhenAddingIfDuplicateId()
        {
            var store = CreateTestStore();
            // Try to add with duplicate identifier.
            Assert.False(store.TryAddAsync(new TenantInfo { Id = "initech-id", Identifier = "initech2" }).Result);
        }

        [Fact]
        public void ReturnFalseWhenAddingIfDuplicateIdentifier()
        {
            var store = CreateTestStore();
            // Try to add with duplicate identifier.
            Assert.False(store.TryAddAsync(new TenantInfo { Id = "initech-id2", Identifier = "initech" }).Result);
        }

        [Fact]
        public virtual void ThrowWhenUpdatingIfTenantInfoIsNull()
        {
            var store = CreateTestStore();

            var e = Assert.Throws<AggregateException>(() => store.TryUpdateAsync(null!).Result);
            Assert.IsType<ArgumentNullException>(e.InnerException);
        }

        [Fact]
        public virtual void ThrowWhenUpdatingIfTenantInfoIdIsNull()
        {
            var store = CreateTestStore();

            var e = Assert.Throws<AggregateException>(() => store.TryUpdateAsync(new TenantInfo()).Result);
            Assert.IsType<ArgumentNullException>(e.InnerException);
        }

        [Fact]
        public void ReturnFalseWhenUpdatingIfTenantIdIsNotFound()
        {
            var store = CreateTestStore();

            var result = store.TryUpdateAsync(new TenantInfo{Id = "not-found"}).Result;
            Assert.False(result);
        }

        [Fact]
        public override void RemoveTenantInfoFromStore()
        {
            base.RemoveTenantInfoFromStore();
        }

        [Fact]
        public void ThrowWhenRemovingIfTenantIdentifierIsNull()
        {
            var store = CreateTestStore();
            var e = Assert.Throws<AggregateException>(() => store.TryRemoveAsync(null!).Result);
            Assert.IsType<ArgumentNullException>(e.InnerException);
        }

        [Fact]
        public void ReturnFalseWhenRemovingIfTenantInfoNotFound()
        {
            var store = CreateTestStore();
            Assert.False(store.TryRemoveAsync("not-there-identifier").Result);
        }

        [Fact]
        public override void UpdateTenantInfoInStore()
        {
            base.UpdateTenantInfoInStore();
        }
    }
}