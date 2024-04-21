// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Internal;
using Finbuckle.MultiTenant.Stores.DistributedCacheStore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Finbuckle.MultiTenant.Test.Stores;

public class DistributedCacheStoreShould : MultiTenantStoreTestBase
{
    [Fact]
    public void ThrownOnGetAllTenantsFromStoreAsync()
    {
        var store = CreateTestStore();
        Assert.Throws<NotImplementedException>(() => store.GetAllAsync().Wait());
    }

    [Fact]
    public async Task RemoveDualEntriesOnRemove()
    {
        var store = CreateTestStore();

        var r = await store.TryRemoveAsync("lol");
        Assert.True(r);

        var t1 = await store.TryGetAsync("lol-id");
        var t2 = await store.TryGetByIdentifierAsync("lol");

        Assert.Null(t1);
        Assert.Null(t2);
    }

    [Fact]
    public async Task RemoveReturnsFalseWhenNoMatchingIdentifierFound()
    {
        var store = CreateTestStore();

        var r = await store.TryRemoveAsync("DoesNotExist");

        Assert.False(r);
    }

    [Fact]
    public async Task AddDualEntriesOnAddOrUpdate()
    {
        var store = CreateTestStore();

        var t2 = await store.TryGetByIdentifierAsync("lol");
        var t1 = await store.TryGetAsync("lol-id");

        Assert.NotNull(t1);
        Assert.NotNull(t2);
        Assert.Equal("lol-id", t1.Id);
        Assert.Equal("lol-id", t2.Id);
        Assert.Equal("lol", t1.Identifier);
        Assert.Equal("lol", t2.Identifier);
    }

    [Fact]
    public async Task RefreshDualEntriesOnAddOrUpdate()
    {
        var store = CreateTestStore();
        Thread.Sleep(2000);
        var t1 = await store.TryGetAsync("lol-id");
        Thread.Sleep(2000);
        var t2 = await store.TryGetByIdentifierAsync("lol");

        Assert.NotNull(t1);
        Assert.NotNull(t2);
        Assert.Equal("lol-id", t1.Id);
        Assert.Equal("lol-id", t2.Id);
        Assert.Equal("lol", t1.Identifier);
        Assert.Equal("lol", t2.Identifier);
    }

    [Fact]
    public async Task ExpireDualEntriesAfterTimespan()
    {
        var store = CreateTestStore();
        Thread.Sleep(3100);
        var t1 = await store.TryGetAsync("lol-id");
        var t2 = await store.TryGetByIdentifierAsync("lol");

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