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

using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Stores;
using Xunit;

public class InMemoryStoreShould : IMultiTenantStoreTestBase<InMemoryStore<TenantInfo>>
{
    private IMultiTenantStore<TenantInfo> CreateCaseSensitiveTestStore()
    {
        var store = new InMemoryStore<TenantInfo>(false);
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
    public void GetTenantInfoFromStoreCaseInsensitiveByDefault()
    {
        var store = CreateTestStore();
        Assert.Equal("initech", store.TryGetByIdentifierAsync("iNitEch").Result.Identifier);
    }

    [Fact]
    public void GetTenantInfoFromStoreCaseSensitive()
    {
        var store = CreateCaseSensitiveTestStore();
        Assert.Equal("initech", store.TryGetByIdentifierAsync("initech").Result.Identifier);
        Assert.Null(store.TryGetByIdentifierAsync("iNitEch").Result);
    }

    [Fact]
    public void FailIfAddingDuplicateCaseSensitive()
    {
        var store = CreateCaseSensitiveTestStore();
        var ti1 = new TenantInfo
        {
            Id = "initech",
            Identifier = "initech",
            Name = "initech"
        };
        var ti2 = new TenantInfo
        {
            Id = "iNiTEch",
            Identifier = "iNiTEch",
            Name = "Initech"
        };
        Assert.False(store.TryAddAsync(ti1).Result);
        Assert.True(store.TryAddAsync(ti2).Result);
    }

    // Basic store functionality tested in MultiTenantStoresShould.cs
    
    protected override IMultiTenantStore<TenantInfo> CreateTestStore()
    {
        var store = new InMemoryStore<TenantInfo>();

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