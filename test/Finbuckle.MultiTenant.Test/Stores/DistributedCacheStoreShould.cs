// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;
using System.Threading;
using Finbuckle.MultiTenant.Internal;
using Finbuckle.MultiTenant.Stores;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Finbuckle.MultiTenant.Test.Stores
{
    public class DistributedCacheStoreShould : MultiTenantStoreTestBase
    {
        [Fact]
        public void ThrownOnGetAllTenantsFromStoreAsync()
        {
            var store = CreateTestStore();
            Assert.Throws<NotImplementedException>(() => store.GetAllAsync().Wait());
        }

        [Fact]
        public void RemoveDualEntriesOnRemove()
        {
            var store = CreateTestStore();

            var r = store.TryRemoveAsync("lol").Result;
            Assert.True(r);

            var t1 = store.TryGetAsync("lol-id").Result;
            var t2 = store.TryGetByIdentifierAsync("lol").Result;

            Assert.Null(t1);
            Assert.Null(t2);
        }

        [Fact]
        public void RemoveReturnsFalseWhenNoMatchingIdentifierFound()
        {
            var store = CreateTestStore();

            var r = store.TryRemoveAsync("DOESNOTEXIST").Result;

            Assert.False(r);
        }

        [Fact]
        public void AddDualEntriesOnAddOrUpdate()
        {
            var store = CreateTestStore();

            var t1 = store.TryGetAsync("lol-id").Result;
            var t2 = store.TryGetByIdentifierAsync("lol").Result;

            Assert.NotNull(t1);
            Assert.NotNull(t2);
            Assert.Equal("lol-id", t1!.Id);
            Assert.Equal("lol-id", t2!.Id);
            Assert.Equal("lol", t1.Identifier);
            Assert.Equal("lol", t2.Identifier);
        }

        [Fact]
        public void RefreshDualEntriesOnAddOrUpdate()
        {
            var store = CreateTestStore();
            Thread.Sleep(2000);
            var t1 = store.TryGetAsync("lol-id").Result;
            Thread.Sleep(2000);
            var t2 = store.TryGetByIdentifierAsync("lol").Result;

            Assert.NotNull(t1);
            Assert.NotNull(t2);
            Assert.Equal("lol-id", t1!.Id);
            Assert.Equal("lol-id", t2!.Id);
            Assert.Equal("lol", t1.Identifier);
            Assert.Equal("lol", t2.Identifier);
        }

        [Fact]
        public void ExpireDualEntriesAfterTimespan()
        {
            var store = CreateTestStore();
            Thread.Sleep(3100);
            var t1 = store.TryGetAsync("lol-id").Result;
            var t2 = store.TryGetByIdentifierAsync("lol").Result;

            Assert.Null(t1);
            Assert.Null(t2);
        }

        // Basic store functionality tested in MultiTenantStoresShould.cs

        protected override IMultiTenantStore<TenantInfo> CreateTestStore()
        {
            var services = new ServiceCollection();
            services.AddOptions().AddDistributedMemoryCache();
            var sp = services.BuildServiceProvider();

            var store = new DistributedCacheStore<TenantInfo>(sp.GetRequiredService<IDistributedCache>(), Constants.TenantToken, TimeSpan.FromSeconds(3));

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