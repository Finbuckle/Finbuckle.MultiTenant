// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Text;
using System.Text.Json;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Internal;
using Finbuckle.MultiTenant.Stores.DistributedCacheStore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Finbuckle.MultiTenant.Test.Stores;

public class DistributedCacheStoreShould : MultiTenantStoreTestBase
{
    [Fact]
    public async Task ThrowOnGetAllTenantsFromStoreAsync()
    {
        var store = CreateTestStore();
        await Assert.ThrowsAsync<NotImplementedException>(async () => await store.GetAllAsync());
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
    public async Task AddDualEntriesOnAdd()
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
    public async Task RefreshDualEntriesOnTryGet()
    {
        var cache = new Mock<IDistributedCache>();
        cache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new TenantInfo
                { Id = "lol-id", Identifier = "lol" })));
        
        var store = new DistributedCacheStore<TenantInfo>(cache.Object, Constants.TenantToken, TimeSpan.FromSeconds(1));

        var t1 = await store.TryGetAsync("lol-id");
        cache.Verify(c => c.RefreshAsync(It.IsAny<string>(), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task RefreshDualEntriesOnTryGetByIdentifier()
    {
        var cache = new Mock<IDistributedCache>();
        cache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new TenantInfo
                { Id = "lol-id", Identifier = "lol" })));
        
        var store = new DistributedCacheStore<TenantInfo>(cache.Object, Constants.TenantToken, TimeSpan.FromSeconds(1));

        var t1 = await store.TryGetByIdentifierAsync("lol-id");
        cache.Verify(c => c.RefreshAsync(It.IsAny<string>(), CancellationToken.None), Times.Once);
    }
    
    [Fact]
    public async Task SetSlidingExpirationOnAdd()
    {
        var cache = new Mock<IDistributedCache>();
        var options = new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromSeconds(1) };

        cache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>((key, value, opts, token) =>
            {
                Assert.Equal(options.SlidingExpiration, opts.SlidingExpiration);
            })
            .Returns(Task.CompletedTask);

        var store = new DistributedCacheStore<TenantInfo>(cache.Object, Constants.TenantToken, TimeSpan.FromSeconds(1));

        await store.TryAddAsync(new TenantInfo { Id = "test-id", Identifier = "test", Name = "Test Tenant" });
    }
    
    [Fact]
    public async Task SetSlidingExpirationOnUpdate()
    {
        var cache = new Mock<IDistributedCache>();
        var options = new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromSeconds(1) };

        cache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>((key, value, opts, token) =>
            {
                Assert.Equal(options.SlidingExpiration, opts.SlidingExpiration);
            })
            .Returns(Task.CompletedTask);

        var store = new DistributedCacheStore<TenantInfo>(cache.Object, Constants.TenantToken, TimeSpan.FromSeconds(1));

        await store.TryAddAsync(new TenantInfo { Id = "test-id", Identifier = "test", Name = "Test Tenant" });
    }

    // Basic store functionality tested in MultiTenantStoresShould.cs

    protected override IMultiTenantStore<TenantInfo> CreateTestStore()
    {
        var services = new ServiceCollection();
        services.AddOptions().AddDistributedMemoryCache();
        var sp = services.BuildServiceProvider();

        var store = new DistributedCacheStore<TenantInfo>(sp.GetRequiredService<IDistributedCache>(),
            Constants.TenantToken, TimeSpan.MaxValue);

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