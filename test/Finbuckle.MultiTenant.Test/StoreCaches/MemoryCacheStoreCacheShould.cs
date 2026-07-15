// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.StoreCaches;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace Finbuckle.MultiTenant.Test.StoreCaches;

public class MemoryCacheStoreCacheShould
{
    [Fact]
    public async Task RemoveIdEntryOnRemove()
    {
        var cache = CreateTestCache();

        await cache.RemoveAsync("lol-id");

        var t1 = await cache.GetAsync("lol-id");
        var t2 = await cache.GetByIdentifierAsync("lol");

        Assert.Null(t1);
        Assert.NotNull(t2);
    }

    [Fact]
    public async Task RemoveDualEntriesOnRemoveByIdentifier()
    {
        var cache = CreateTestCache();

        await cache.RemoveByIdentifierAsync("lol");

        var t1 = await cache.GetAsync("lol-id");
        var t2 = await cache.GetByIdentifierAsync("lol");

        Assert.Null(t1);
        Assert.Null(t2);
    }

    [Fact]
    public async Task AddDualEntriesOnSet()
    {
        var cache = CreateTestCache();

        var t1 = await cache.GetAsync("lol-id");
        var t2 = await cache.GetByIdentifierAsync("lol");

        Assert.NotNull(t1);
        Assert.NotNull(t2);
        Assert.Equal("lol-id", t1.Id);
        Assert.Equal("lol-id", t2.Id);
        Assert.Equal("lol", t1.Identifier);
        Assert.Equal("lol", t2.Identifier);
    }

    [Fact]
    public async Task ApplyConfiguredOptionsOnSet()
    {
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var cache = new MemoryCacheStoreCache<TenantInfo>(memoryCache, Constants.TenantToken,
            new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(1) });

        await cache.SetAsync(new TenantInfo { Id = "test-id", Identifier = "test" });
        await Task.Delay(50);

        Assert.Null(await cache.GetAsync("test-id"));
        Assert.Null(await cache.GetByIdentifierAsync("test"));
    }

    private static IMultiTenantStoreCache<TenantInfo> CreateTestCache()
    {
        var cache = new MemoryCacheStoreCache<TenantInfo>(new MemoryCache(new MemoryCacheOptions()),
            Constants.TenantToken, new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.MaxValue });

        cache.SetAsync(new TenantInfo { Id = "initech-id", Identifier = "initech" }).GetAwaiter().GetResult();
        cache.SetAsync(new TenantInfo { Id = "lol-id", Identifier = "lol" }).GetAwaiter().GetResult();

        return cache;
    }
}
