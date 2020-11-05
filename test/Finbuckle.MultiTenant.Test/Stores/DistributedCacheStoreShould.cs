//    Copyright 2020 Andrew White
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
using Finbuckle.MultiTenant.Stores;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Distributed;
using Xunit;
using System;
using Finbuckle.MultiTenant.Internal;

public class DistributedCacheStoreShould : IMultiTenantStoreTestBase<InMemoryStore<TenantInfo>>
{
    private IMultiTenantStore<TenantInfo> CreateCaseSensitiveTestStore()
    {
        var services = new ServiceCollection();
        services.AddOptions().AddDistributedMemoryCache();
        var sp = services.BuildServiceProvider();

        var store = new DistributedCacheStore<TenantInfo>(sp.GetRequiredService<IDistributedCache>(), Constants.TenantToken, TimeSpan.FromSeconds(5));
        
        var ti1 = new TenantInfo
        {
            Id = "initech",
            Identifier = "initech",
            Name = "initech"
        };
        var ti2 = new TenantInfo
        {
            Id = "lol",
            Identifier = "lol",
            Name = "lol"
        };
        store.TryAddAsync(ti1).Wait();
        store.TryAddAsync(ti2).Wait();

        return store;
    }

    [Fact]
    public void ThrownOnGetAllTenantsFromStoreAsync()
    {
        var store = CreateTestStore();
        Assert.Throws<NotImplementedException>(() => store.GetAllAsync().Wait());
    }

    [Fact]
    public void RemoveDualEntriesOnRemove()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void AddDualEntriesOnAddOrUpdate()
    {
        // Update calls Add so this covers updates as well.
        throw new NotImplementedException();
    }

    [Fact]
    public void RefreshDualEntriesOnAddOrUpdate()
    {
        // Update calls Add so this covers updates as well.
        throw new NotImplementedException();
    }

    [Fact]
    public void ExpireDualEntriesAfterTimespan()
    {
        throw new NotImplementedException();
    }

    // Basic store functionality tested in MultiTenantStoresShould.cs
    
    protected override IMultiTenantStore<TenantInfo> CreateTestStore()
    {
        var store = new InMemoryStore<TenantInfo>(null);

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
}