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
using Xunit;

#pragma warning disable xUnit1013 // Public method should be marked as test

public abstract class IMultiTenantStoreTestBase<T> where T : IMultiTenantStore
{
    protected abstract IMultiTenantStore CreateTestStore();

    protected virtual IMultiTenantStore PopulateTestStore(IMultiTenantStore store)
    {
        store.TryAddAsync(new TenantInfo("initech-id", "initech", "Initech", "connstring", null)).Wait();
        store.TryAddAsync(new TenantInfo("lol-id", "lol", "Lol, Inc.", "connstring2", null)).Wait();

        return store;
    }

    //[Fact]
    public virtual void GetTenantInfoFromStoreById()
    {
        var store = CreateTestStore();

        Assert.Equal("initech", store.TryGetAsync("initech-id").Result.Identifier);
    }

    //[Fact]
    public virtual void ReturnNullWhenGettingByIdIfTenantInfoNotFound()
    {
        var store = CreateTestStore();

        Assert.Null(store.TryGetAsync("fake123").Result);
    }

    //[Fact]
    public virtual void GetTenantInfoFromStoreByIdentifier()
    {
        var store = CreateTestStore();

        Assert.Equal("initech", store.TryGetByIdentifierAsync("initech").Result.Identifier);
    }

    //[Fact]
    public virtual void ReturnNullWhenGettingByIdentifierIfTenantInfoNotFound()
    {
        var store = CreateTestStore();
        Assert.Null(store.TryGetByIdentifierAsync("fake123").Result);
    }

    //[Fact]
    public virtual void AddTenantInfoToStore()
    {
        var store = CreateTestStore();

        Assert.Null(store.TryGetByIdentifierAsync("test-identifier").Result);
        Assert.True(store.TryAddAsync(new TenantInfo("test-id", "test-identifier", "test", "connstring", null)).Result);
        Assert.NotNull(store.TryGetByIdentifierAsync("test-identifier").Result);
    }

    //[Fact]
    public virtual void UpdateTenantInfoInStore()
    {
        var store = CreateTestStore();

        var result = store.TryUpdateAsync(new TenantInfo("initech-id", "test123", "name", "connstring", null)).Result;
        Assert.Equal(true, result);
    }

    //[Fact]
    public virtual void RemoveTenantInfoFromStore()
    {
        var store = CreateTestStore();

        Assert.NotNull(store.TryGetByIdentifierAsync("initech").Result);
        Assert.True(store.TryRemoveAsync("initech").Result);
        Assert.Null(store.TryGetByIdentifierAsync("initech").Result);
    }
}