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

using Finbuckle.MultiTenant;
using Xunit;

#pragma warning disable xUnit1013 // Public method should be marked as test

public abstract class IMultiTenantStoreTestBase<T> where T : IMultiTenantStore<TenantInfo>
{
    protected abstract IMultiTenantStore<TenantInfo> CreateTestStore();

    protected virtual IMultiTenantStore<TenantInfo> PopulateTestStore(IMultiTenantStore<TenantInfo> store)
    {
        var r1 = store.TryAddAsync(new TenantInfo { Id = "initech-id", Identifier = "initech", Name = "Initech", ConnectionString = "connstring" }).Result;
        r1 = store.TryAddAsync(new TenantInfo { Id = "lol-id", Identifier = "lol", Name = "Lol, Inc.", ConnectionString = "connstring2" }).Result;

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

        Assert.Null(store.TryGetByIdentifierAsync("identifier").Result);
        Assert.True(store.TryAddAsync(new TenantInfo { Id = "id", Identifier = "identifier", Name = "name", ConnectionString = "cs" }).Result);
        Assert.NotNull(store.TryGetByIdentifierAsync("identifier").Result);
    }

    //[Fact]
    public virtual void UpdateTenantInfoInStore()
    {
        var store = CreateTestStore();

        var result = store.TryUpdateAsync(new TenantInfo { Id = "initech-id", Identifier = "initech2", Name = "Initech2", ConnectionString = "connstring2" }).Result;
        Assert.True(result);
    }

    //[Fact]
    public virtual void RemoveTenantInfoFromStore()
    {
        var store = CreateTestStore();
        // TODO: Change to use Id instead of identifier
        Assert.NotNull(store.TryGetByIdentifierAsync("initech").Result);
        Assert.True(store.TryRemoveAsync("initech").Result);
        Assert.Null(store.TryGetByIdentifierAsync("initech").Result);
    }
}