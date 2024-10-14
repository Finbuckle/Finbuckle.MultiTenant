using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Internal;

using Finbuckle.MultiTenant.Test.Stores;

using Microsoft.Extensions.DependencyInjection;
using ZiggyCreatures.Caching.Fusion;


namespace Finbuckle.MultiTenant.Store.FusionCache.Test;

public class FusionCacheStoreShould : MultiTenantStoreTestBase
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
        services.AddOptions().AddFusionCache();
        var sp = services.BuildServiceProvider();

        
        var store = new FusionCacheStore<TenantInfo>(sp.GetRequiredService<IFusionCache>(), Constants.TenantToken, TimeSpan.FromSeconds(3));

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