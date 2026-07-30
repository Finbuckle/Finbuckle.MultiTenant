// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Text;
using System.Text.Json;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.StoreCaches;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Finbuckle.MultiTenant.Test.StoreCaches;

public class DistributedCacheStoreCacheShould
{
    [Fact]
    public async Task RemoveIdEntryOnRemove()
    {
        var cache = await CreateTestCache();

        await cache.RemoveAsync("lol-id");

        var t1 = await cache.GetAsync("lol-id");
        var t2 = await cache.GetByIdentifierAsync("lol");

        Assert.Null(t1);
        Assert.NotNull(t2);
    }

    [Fact]
    public async Task RemoveDualEntriesOnRemoveByIdentifier()
    {
        var cache = await CreateTestCache();

        await cache.RemoveByIdentifierAsync("lol");

        var t1 = await cache.GetAsync("lol-id");
        var t2 = await cache.GetByIdentifierAsync("lol");

        Assert.Null(t1);
        Assert.Null(t2);
    }

    [Fact]
    public async Task AddDualEntriesOnSet()
    {
        var cache = await CreateTestCache();

        var t2 = await cache.GetByIdentifierAsync("lol");
        var t1 = await cache.GetAsync("lol-id");

        Assert.NotNull(t1);
        Assert.NotNull(t2);
        Assert.Equal("lol-id", t1.Id);
        Assert.Equal("lol-id", t2.Id);
        Assert.Equal("lol", t1.Identifier);
        Assert.Equal("lol", t2.Identifier);
    }

    [Fact]
    public async Task RefreshDualEntriesOnGet()
    {
        var distributedCache = new Mock<IDistributedCache>();
        distributedCache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new TenantInfo
            {
                Id = "lol-id",
                Identifier = "lol"
            })));

        var cache = new DistributedCacheStoreCache<TenantInfo, string>(distributedCache.Object, Constants.TenantToken,
            new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromSeconds(1) });

        await cache.GetAsync("lol-id");
        distributedCache.Verify(c => c.RefreshAsync(It.IsAny<string>(), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task RefreshDualEntriesOnGetByIdentifier()
    {
        var distributedCache = new Mock<IDistributedCache>();
        distributedCache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new TenantInfo
                { Id = "lol-id", Identifier = "lol" })));

        var cache = new DistributedCacheStoreCache<TenantInfo, string>(distributedCache.Object, Constants.TenantToken,
            new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromSeconds(1) });

        await cache.GetByIdentifierAsync("lol-id");
        distributedCache.Verify(c => c.RefreshAsync(It.IsAny<string>(), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task SetConfiguredOptionsOnSet()
    {
        var distributedCache = new Mock<IDistributedCache>();
        var options = new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromSeconds(1) };

        distributedCache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>((_, _, opts, _) =>
            {
                Assert.Equal(options.SlidingExpiration, opts.SlidingExpiration);
            })
            .Returns(Task.CompletedTask);

        var cache = new DistributedCacheStoreCache<TenantInfo, string>(distributedCache.Object, Constants.TenantToken, options);

        await cache.SetAsync(new TenantInfo { Id = "test-id", Identifier = "test" });
    }

    [Fact]
    public async Task RoundTripIntTenantId()
    {
        var services = new ServiceCollection();
        services.AddOptions().AddDistributedMemoryCache();
        var sp = services.BuildServiceProvider();

        var cache = new DistributedCacheStoreCache<TenantInfo<int>, int>(sp.GetRequiredService<IDistributedCache>(),
            Constants.TenantToken, new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.MaxValue });

        await cache.SetAsync(new TenantInfo<int> { Id = 42, Identifier = "initech" });

        Assert.Equal(42, (await cache.GetAsync(42))!.Id);
        Assert.Equal(42, (await cache.GetByIdentifierAsync("initech"))!.Id);

        await cache.RemoveAsync(42);
        Assert.Null(await cache.GetAsync(42));
    }

    [Fact]
    public async Task RoundTripGuidTenantId()
    {
        var id = Guid.NewGuid();
        var services = new ServiceCollection();
        services.AddOptions().AddDistributedMemoryCache();
        var sp = services.BuildServiceProvider();

        var cache = new DistributedCacheStoreCache<TenantInfo<Guid>, Guid>(sp.GetRequiredService<IDistributedCache>(),
            Constants.TenantToken, new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.MaxValue });

        await cache.SetAsync(new TenantInfo<Guid> { Id = id, Identifier = "initech" });

        Assert.Equal(id, (await cache.GetAsync(id))!.Id);
        Assert.Equal(id, (await cache.GetByIdentifierAsync("initech"))!.Id);

        await cache.RemoveByIdentifierAsync("initech");
        Assert.Null(await cache.GetAsync(id));
        Assert.Null(await cache.GetByIdentifierAsync("initech"));
    }

    private static async Task<IMultiTenantStoreCache<TenantInfo, string>> CreateTestCache()
    {
        var services = new ServiceCollection();
        services.AddOptions().AddDistributedMemoryCache();
        var sp = services.BuildServiceProvider();

        var cache = new DistributedCacheStoreCache<TenantInfo, string>(sp.GetRequiredService<IDistributedCache>(),
            Constants.TenantToken, new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.MaxValue });

        await cache.SetAsync(new TenantInfo { Id = "initech-id", Identifier = "initech" });
        await cache.SetAsync(new TenantInfo { Id = "lol-id", Identifier = "lol" });

        return cache;
    }
}
