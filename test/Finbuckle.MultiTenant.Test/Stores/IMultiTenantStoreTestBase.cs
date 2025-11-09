// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Xunit;

#pragma warning disable xUnit1013 // Public method should be marked as test

namespace Finbuckle.MultiTenant.Test.Stores;

// TODO convert these to async

public abstract class MultiTenantStoreTestBase
{
    protected abstract IMultiTenantStore<TenantInfo> CreateTestStore();

    protected virtual IMultiTenantStore<TenantInfo> PopulateTestStore(IMultiTenantStore<TenantInfo> store)
    {
        store.AddAsync(new TenantInfo(Id: "initech-id", Identifier: "initech", Name: "Initech")).Wait();
        store.AddAsync(new TenantInfo(Id: "lol-id", Identifier: "lol", Name: "Lol, Inc.")).Wait();

        return store;
    }

    //[Fact]
    public virtual void GetTenantInfoFromStoreById()
    {
        var store = CreateTestStore();

        Assert.Equal("initech", store.GetAsync("initech-id").Result!.Identifier);
    }

    //[Fact]
    public virtual void ReturnNullWhenGettingByIdIfTenantInfoNotFound()
    {
        var store = CreateTestStore();

        Assert.Null(store.GetAsync("fake123").Result);
    }

    //[Fact]
    public virtual void GetTenantInfoFromStoreByIdentifier()
    {
        var store = CreateTestStore();

        Assert.Equal("initech", store.GetByIdentifierAsync("initech").Result!.Identifier);
    }

    //[Fact]
    public virtual void ReturnNullWhenGettingByIdentifierIfTenantInfoNotFound()
    {
        var store = CreateTestStore();
        Assert.Null(store.GetByIdentifierAsync("fake123").Result);
    }

    //[Fact]
    public virtual void AddTenantInfoToStore()
    {
        var store = CreateTestStore();

        Assert.Null(store.GetByIdentifierAsync("identifier").Result);
        Assert.True(store.AddAsync(new TenantInfo(Id: "id", Identifier: "identifier", Name: "name")).Result);
        Assert.NotNull(store.GetByIdentifierAsync("identifier").Result);
    }

    //[Fact]
    public virtual void UpdateTenantInfoInStore()
    {
        var store = CreateTestStore();

        var result = store.UpdateAsync(new TenantInfo(Id: "initech-id", Identifier: "initech2")).Result;
        Assert.True(result);
    }

    //[Fact]
    public virtual void RemoveTenantInfoFromStore()
    {
        var store = CreateTestStore();
        Assert.NotNull(store.GetByIdentifierAsync("initech").Result);
        Assert.True(store.RemoveAsync("initech").Result);
        Assert.Null(store.GetByIdentifierAsync("initech").Result);
    }

    //[Fact]
    public virtual void GetAllTenantsFromStoreAsync()
    {
        var store = CreateTestStore();
        Assert.Equal(2, store.GetAllAsync().Result.Count());
    }

    //[Fact]
    public virtual void GetAllTenantsFromStoreAsyncSkip1Take1()
    {
        var store = CreateTestStore();
        var tenants = store.GetAllAsync(1, 1).Result.ToList();
        Assert.Single(tenants);

        var tenant = tenants.FirstOrDefault();
        Assert.NotNull(tenant);
        Assert.Equal("lol", tenant.Identifier);
    }
}