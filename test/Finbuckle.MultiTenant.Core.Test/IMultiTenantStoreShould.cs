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



using System;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Stores;
using Xunit;

public abstract class IMultiTenantStoreTestBase<T> where T : IMultiTenantStore
{
    protected abstract IMultiTenantStore CreateTestStore();

    protected virtual IMultiTenantStore PopulateTestStore(IMultiTenantStore store)
    {
        store.TryAddAsync(new TenantInfo("initech-id", "initech", "Initech", null, null)).Wait();
        store.TryAddAsync(new TenantInfo("lol-id", "lol", "Lol, Inc.", null, null)).Wait();

        return store;
    }

    [Fact]
    public void GetTenantInfoFromStoreById()
    {
        var store = CreateTestStore();
        Assert.Equal("initech", store.TryGetAsync("initech-id").Result.Identifier);
    }

    [Fact]
    public void ReturnNullWhenGettingByIdIfTenantInfoNotFound()
    {
        var store = CreateTestStore();

        Assert.Null(store.TryGetAsync("fake123").Result);
    }

    [Fact]
    public void ThrowWhenGettingByIdIfTenantIdIsNull()
    {
        var store = CreateTestStore();

        var e = Assert.Throws<AggregateException>(() => store.TryGetAsync(null).Result);
        Assert.IsType<ArgumentNullException>(e.InnerException);
    }

    [Fact]
    public void GetTenantInfoFromStoreByIdentifier()
    {
        var store = CreateTestStore();
        Assert.Equal("initech", store.TryGetByIdentifierAsync("initech").Result.Identifier);
    }

    [Fact]
    public void ReturnNullWhenGettingByIdentifierIfTenantInfoNotFound()
    {
        var store = CreateTestStore();

        Assert.Null(store.TryGetByIdentifierAsync("fake123").Result);
    }

    [Fact]
    public void ThrowWhenGettingByIdentifierIfTenantIdentifierIsNull()
    {
        var store = CreateTestStore();

        var e = Assert.Throws<AggregateException>(() => store.TryGetByIdentifierAsync(null).Result);
        Assert.IsType<ArgumentNullException>(e.InnerException);
    }

    [Fact]
    public void AddTenantInfoToStore()
    {
        var store = CreateTestStore();

        Assert.Null(store.TryGetByIdentifierAsync("test-identifier").Result);
        Assert.True(store.TryAddAsync(new TenantInfo("test-id", "test-identifier", "test", null, null)).Result);
        Assert.NotNull(store.TryGetByIdentifierAsync("test-identifier").Result);
    }

    [Fact]
    public void ThrowWhenAddingIfTenantInfoIsNull()
    {
        var store = CreateTestStore();
        var e = Assert.Throws<AggregateException>(() => store.TryAddAsync(null).Result);
        Assert.IsType<ArgumentNullException>(e.InnerException);
    }

    [Fact]
    public void ThrowWhenAddingIfTenantInfoIdIsNull()
    {
        var store = CreateTestStore();
        var e = Assert.Throws<AggregateException>(() => store.TryAddAsync(new TenantInfo(null, null, null, null, null)).Result);
        Assert.IsType<ArgumentNullException>(e.InnerException);
    }

    [Fact]
    public void ReturnFalseWhenAddingIfDuplicateId()
    {
        var store = CreateTestStore();
        // Try to add with duplicate identifier.
        Assert.False(store.TryAddAsync(new TenantInfo("initech-id", "initech123", "Initech", null, null)).Result);
    }

    [Fact]
    public void ReturnFalseWhenAddingIfDuplicateIdentifier()
    {
        var store = CreateTestStore();
        // Try to add with duplicate identifier.
        Assert.False(store.TryAddAsync(new TenantInfo("initech-id123", "initech", "Initech", null, null)).Result);
    }

    [Theory]
    [InlineData("initech-id", true)]
    [InlineData("notFound", false)]
    public void UpdateTenantInfoInStore(string id, bool expected)
    {
        var store = CreateTestStore();

        var result = store.TryUpdateAsync(new TenantInfo(id, "test123", null, null, null)).Result;

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ThrowWhenUpdatingIfTenantInfoIsNull()
    {
        var store = CreateTestStore();

        var e = Assert.Throws<AggregateException>(() => store.TryUpdateAsync(null).Result);
        Assert.IsType<ArgumentNullException>(e.InnerException);
    }

    [Fact]
    public void ThrowWhenUpdatingIfTenantInfoIdIsNull()
    {
        var store = CreateTestStore();
        
        var e = Assert.Throws<AggregateException>(() => store.TryUpdateAsync(new TenantInfo(null, null, null, null, null)).Result);
        Assert.IsType<ArgumentNullException>(e.InnerException);
    }

    [Fact]
    public void RemoveTenantInfoFromStore()
    {
        var store = CreateTestStore();

        Assert.NotNull(store.TryGetByIdentifierAsync("initech").Result);
        Assert.True(store.TryRemoveAsync("initech").Result);
        Assert.Null(store.TryGetByIdentifierAsync("initech").Result);
    }

    [Fact]
    public void ThrowWhenRemovingIfTenantIdentifierIsNull()
    {
        var store = CreateTestStore();
        var e = Assert.Throws<AggregateException>(() => store.TryRemoveAsync(null).Result);
        Assert.IsType<ArgumentNullException>(e.InnerException);
    }

    [Fact]
    public void ReturnFalseWhenRemovingIfTenantInfoNotFound()
    {
        var store = CreateTestStore();
        Assert.False(store.TryRemoveAsync("not-there-identifier").Result);
    }
}