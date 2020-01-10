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

public class InMemoryStoreShould : IMultiTenantStoreTestBase<InMemoryStore>
{
    private IMultiTenantStore CreateCaseSensitiveTestStore()
    {
        var store = new InMemoryStore(false);
        store.TryAddAsync(new TenantInfo("initech", "initech", "Initech", null, null)).Wait();
        store.TryAddAsync(new TenantInfo("lol", "lol", "Lol, Inc.", null, null)).Wait();

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
        Assert.False(store.TryAddAsync(new TenantInfo("initech", "initech", "Initech", null, null)).Result);
        Assert.True(store.TryAddAsync(new TenantInfo("iNiTEch", "iNiTEch", "Initech", null, null)).Result);
    }

    // Basic store functionality tested in MultiTenantStoresShould.cs
    
    protected override IMultiTenantStore CreateTestStore()
    {
        var store = new InMemoryStore();

        return PopulateTestStore(store);
    }

    protected override IMultiTenantStore PopulateTestStore(IMultiTenantStore store)
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