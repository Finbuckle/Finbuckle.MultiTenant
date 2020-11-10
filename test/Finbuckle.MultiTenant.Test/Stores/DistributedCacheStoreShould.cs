//    Copyright 2020 Finbuckle LLC, Andrew White, and Contributors
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

using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Stores;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Distributed;
using Xunit;
using System;
using Finbuckle.MultiTenant.Internal;
using System.Threading;

public class DistributedCacheStoreShould : IMultiTenantStoreTestBase<InMemoryStore<TenantInfo>>
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
    public void AddDualEntriesOnAddOrUpdate()
    {
        var store = CreateTestStore();

        var t1 = store.TryGetAsync("lol-id").Result;
        var t2 = store.TryGetByIdentifierAsync("lol").Result;

        Assert.NotNull(t1);
        Assert.NotNull(t2);
        Assert.Equal("lol-id", t1.Id);
        Assert.Equal("lol-id", t2.Id);
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
        Assert.Equal("lol-id", t1.Id);
        Assert.Equal("lol-id", t2.Id);
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

    protected override IMultiTenantStore<TenantInfo> PopulateTestStore(IMultiTenantStore<TenantInfo> store)
    {
        return base.PopulateTestStore(store);
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

    //[Fact(Skip="Not valid for this store")]
    public override void GetAllTenantsFromStoreAsync()
    {
        base.GetAllTenantsFromStoreAsync();
    }
}