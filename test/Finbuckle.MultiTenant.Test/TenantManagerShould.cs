// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Xunit;

namespace Finbuckle.MultiTenant.Test;

public class TenantManagerShould
{
    [Fact]
    public async Task ReturnTenantFromFirstCacheWithoutQueryingPrimaryStore()
    {
        var tenant = new TenantInfo { Id = "initech-id", Identifier = "initech" };
        var store = new TestStore();
        var cache = new TestCache();
        await cache.SetAsync(tenant);
        var manager = new TenantManager<TenantInfo>(store, [cache]);

        var result = await manager.GetByIdentifierAsync("initech");

        Assert.Same(tenant, result);
        Assert.Equal(0, store.GetByIdentifierCount);
    }

    [Fact]
    public async Task FillEarlierMissedCachesWhenLaterCacheHits()
    {
        var tenant = new TenantInfo { Id = "initech-id", Identifier = "initech" };
        var firstCache = new TestCache();
        var secondCache = new TestCache();
        await secondCache.SetAsync(tenant);
        var manager = new TenantManager<TenantInfo>(new TestStore(), [firstCache, secondCache]);

        var result = await manager.GetByIdentifierAsync("initech");

        Assert.Same(tenant, result);
        Assert.Contains("initech", firstCache.SetIdentifiers);
    }

    [Fact]
    public async Task ReturnTenantFromFirstCacheForGetByIdWithoutQueryingPrimaryStore()
    {
        var tenant = new TenantInfo { Id = "initech-id", Identifier = "initech" };
        var store = new TestStore();
        var cache = new TestCache();
        await cache.SetAsync(tenant);
        var manager = new TenantManager<TenantInfo>(store, [cache]);

        var result = await manager.GetAsync("initech-id");

        Assert.Same(tenant, result);
        Assert.Equal(0, store.GetByIdCount);
    }

    [Fact]
    public async Task FillEarlierMissedCachesForGetByIdWhenLaterCacheHits()
    {
        var tenant = new TenantInfo { Id = "initech-id", Identifier = "initech" };
        var firstCache = new TestCache();
        var secondCache = new TestCache();
        await secondCache.SetAsync(tenant);
        var manager = new TenantManager<TenantInfo>(new TestStore(), [firstCache, secondCache]);

        var result = await manager.GetAsync("initech-id");

        Assert.Same(tenant, result);
        Assert.Contains("initech", firstCache.SetIdentifiers);
    }

    [Fact]
    public async Task FillMissedCachesWhenPrimaryStoreHits()
    {
        var tenant = new TenantInfo { Id = "initech-id", Identifier = "initech" };
        var store = new TestStore(tenant);
        var firstCache = new TestCache();
        var secondCache = new TestCache();
        var manager = new TenantManager<TenantInfo>(store, [firstCache, secondCache]);

        var result = await manager.GetByIdentifierAsync("initech");

        Assert.Same(tenant, result);
        Assert.Contains("initech", firstCache.SetIdentifiers);
        Assert.Contains("initech", secondCache.SetIdentifiers);
    }

    [Fact]
    public async Task FillMissedCachesForGetByIdWhenPrimaryStoreHits()
    {
        var tenant = new TenantInfo { Id = "initech-id", Identifier = "initech" };
        var store = new TestStore(tenant);
        var cache = new TestCache();
        var manager = new TenantManager<TenantInfo>(store, [cache]);

        var result = await manager.GetAsync("initech-id");

        Assert.Same(tenant, result);
        Assert.Contains("initech", cache.SetIdentifiers);
    }

    [Fact]
    public async Task UsePrimaryStoreWithoutCaches()
    {
        var tenant = new TenantInfo { Id = "initech-id", Identifier = "initech" };
        var store = new TestStore(tenant);
        var manager = new TenantManager<TenantInfo>(store, []);

        var result = await manager.GetByIdentifierAsync("initech");

        Assert.Same(tenant, result);
        Assert.Equal(1, store.GetByIdentifierCount);
    }

    [Fact]
    public async Task QueryPrimaryOnlyForGetAll()
    {
        var tenant = new TenantInfo { Id = "initech-id", Identifier = "initech" };
        var store = new TestStore(tenant);
        var cache = new TestCache();
        var manager = new TenantManager<TenantInfo>(store, [cache]);

        var result = await manager.GetAllAsync();

        Assert.Equal("initech", result.Single().Identifier);
        Assert.Equal(0, cache.GetByIdentifierCount);
        Assert.Empty(cache.SetIdentifiers);
    }

    [Fact]
    public async Task InvalidateCachesAfterSuccessfulAdd()
    {
        var tenant = new TenantInfo { Id = "initech-id", Identifier = "initech" };
        var cache = new TestCache();
        var manager = new TenantManager<TenantInfo>(new TestStore(), [cache]);

        var result = await manager.AddAsync(tenant);

        Assert.True(result);
        Assert.Contains("initech-id", cache.RemovedIds);
        Assert.Contains("initech", cache.RemovedIdentifiers);
        Assert.Empty(cache.SetIdentifiers);
    }

    [Fact]
    public async Task NotInvalidateCachesAfterFailedAdd()
    {
        var tenant = new TenantInfo { Id = "initech-id", Identifier = "initech" };
        var store = new TestStore(tenant);
        var cache = new TestCache();
        var manager = new TenantManager<TenantInfo>(store, [cache]);

        var result = await manager.AddAsync(tenant);

        Assert.False(result);
        Assert.Empty(cache.RemovedIds);
        Assert.Empty(cache.RemovedIdentifiers);
    }

    [Fact]
    public async Task InvalidateOldAndNewTenantAfterSuccessfulUpdate()
    {
        var oldTenant = new TenantInfo { Id = "initech-id", Identifier = "initech" };
        var newTenant = new TenantInfo { Id = "initech-id", Identifier = "initech-new" };
        var cache = new TestCache();
        var manager = new TenantManager<TenantInfo>(new TestStore(oldTenant), [cache]);

        var result = await manager.UpdateAsync(newTenant);

        Assert.True(result);
        Assert.Contains("initech-id", cache.RemovedIds);
        Assert.Contains("initech", cache.RemovedIdentifiers);
        Assert.Contains("initech-id", cache.RemovedIds);
        Assert.Contains("initech-new", cache.RemovedIdentifiers);
    }

    [Fact]
    public async Task NotInvalidateCachesAfterFailedUpdate()
    {
        var tenant = new TenantInfo { Id = "initech-id", Identifier = "initech" };
        var cache = new TestCache();
        var manager = new TenantManager<TenantInfo>(new TestStore(), [cache]);

        var result = await manager.UpdateAsync(tenant);

        Assert.False(result);
        Assert.Empty(cache.RemovedIds);
        Assert.Empty(cache.RemovedIdentifiers);
    }

    [Fact]
    public async Task InvalidateExistingTenantAfterSuccessfulRemove()
    {
        var tenant = new TenantInfo { Id = "initech-id", Identifier = "initech" };
        var cache = new TestCache();
        var manager = new TenantManager<TenantInfo>(new TestStore(tenant), [cache]);

        var result = await manager.RemoveAsync("initech-id");

        Assert.True(result);
        Assert.Contains("initech-id", cache.RemovedIds);
        Assert.Contains("initech", cache.RemovedIdentifiers);
    }

    [Fact]
    public async Task InvalidateExistingTenantAfterSuccessfulRemoveByIdentifier()
    {
        var tenant = new TenantInfo { Id = "initech-id", Identifier = "initech" };
        var cache = new TestCache();
        var manager = new TenantManager<TenantInfo>(new TestStore(tenant), [cache]);

        var result = await manager.RemoveByIdentifierAsync("initech");

        Assert.True(result);
        Assert.Contains("initech-id", cache.RemovedIds);
        Assert.Contains("initech", cache.RemovedIdentifiers);
    }

    [Fact]
    public async Task InvalidateIdentifierAfterSuccessfulRemoveWhenPreReadMisses()
    {
        var cache = new TestCache();
        var store = new TestStore();
        store.RemoveResult = true;
        var manager = new TenantManager<TenantInfo>(store, [cache]);

        var result = await manager.RemoveByIdentifierAsync("initech");

        Assert.True(result);
        Assert.Contains("initech", cache.RemovedIdentifiers);
    }

    [Fact]
    public async Task InvalidateIdAfterSuccessfulRemoveWhenPreReadMisses()
    {
        var cache = new TestCache();
        var store = new TestStore { RemoveResult = true };
        var manager = new TenantManager<TenantInfo>(store, [cache]);

        var result = await manager.RemoveAsync("initech-id");

        Assert.True(result);
        Assert.Contains("initech-id", cache.RemovedIds);
        Assert.Empty(cache.RemovedIdentifiers);
    }

    [Fact]
    public async Task NotInvalidateCachesAfterFailedRemove()
    {
        var cache = new TestCache();
        var manager = new TenantManager<TenantInfo>(new TestStore(), [cache]);

        var result = await manager.RemoveByIdentifierAsync("initech");

        Assert.False(result);
        Assert.Empty(cache.RemovedIds);
        Assert.Empty(cache.RemovedIdentifiers);
    }

    [Fact]
    public async Task ContinueToPrimaryStoreWhenCacheThrows()
    {
        var tenant = new TenantInfo { Id = "initech-id", Identifier = "initech" };
        var store = new TestStore(tenant);
        var cache = new TestCache { ThrowOnGetByIdentifier = true };
        var manager = new TenantManager<TenantInfo>(store, [cache]);

        var result = await manager.GetByIdentifierAsync("initech");

        Assert.Same(tenant, result);
        Assert.Equal(1, store.GetByIdentifierCount);
    }

    [Fact]
    public async Task ReturnFalseWhenPrimaryStoreAddThrows()
    {
        var tenant = new TenantInfo { Id = "initech-id", Identifier = "initech" };
        var manager = new TenantManager<TenantInfo>(new TestStore { ThrowOnAdd = true }, []);

        var result = await manager.AddAsync(tenant);

        Assert.False(result);
    }

    [Fact]
    public async Task IgnoreCacheSetExceptionsDuringReadThrough()
    {
        var tenant = new TenantInfo { Id = "initech-id", Identifier = "initech" };
        var store = new TestStore(tenant);
        var cache = new TestCache { ThrowOnSet = true };
        var manager = new TenantManager<TenantInfo>(store, [cache]);

        var result = await manager.GetByIdentifierAsync("initech");

        Assert.Same(tenant, result);
    }

    [Fact]
    public async Task IgnoreCacheRemoveExceptionsDuringInvalidation()
    {
        var tenant = new TenantInfo { Id = "initech-id", Identifier = "initech" };
        var cache = new TestCache { ThrowOnRemove = true };
        var manager = new TenantManager<TenantInfo>(new TestStore(tenant), [cache]);

        var result = await manager.RemoveAsync("initech-id");

        Assert.True(result);
    }

    [Fact]
    public async Task ForwardCancellationTokenToPrimaryStoreAndCaches()
    {
        var tenant = new TenantInfo { Id = "initech-id", Identifier = "initech" };
        var store = new TestStore(tenant);
        var cache = new TestCache();
        var manager = new TenantManager<TenantInfo>(store, [cache]);
        using var cts = new CancellationTokenSource();

        await manager.GetByIdentifierAsync("initech", cts.Token);

        Assert.Equal(cts.Token, cache.LastCancellationToken);
        Assert.Equal(cts.Token, store.LastCancellationToken);
    }

    private class TestStore : IMultiTenantStore<TenantInfo>
    {
        private readonly Dictionary<string, TenantInfo> tenantsByIdentifier = new(StringComparer.OrdinalIgnoreCase);

        public TestStore(params TenantInfo[] tenants)
        {
            foreach (var tenant in tenants)
                tenantsByIdentifier[tenant.Identifier] = tenant;
        }

        public int GetByIdentifierCount { get; private set; }

        public int GetByIdCount { get; private set; }

        public bool? RemoveResult { get; set; }

        public bool ThrowOnAdd { get; init; }

        public CancellationToken LastCancellationToken { get; private set; }

        public Task<bool> AddAsync(TenantInfo tenantInfo, CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;

            if (ThrowOnAdd)
                throw new InvalidOperationException();

            if (tenantsByIdentifier.ContainsKey(tenantInfo.Identifier))
                return Task.FromResult(false);

            tenantsByIdentifier[tenantInfo.Identifier] = tenantInfo;
            return Task.FromResult(true);
        }

        public Task<bool> UpdateAsync(TenantInfo tenantInfo, CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;

            var existing = tenantsByIdentifier.Values.SingleOrDefault(t => t.Id == tenantInfo.Id);
            if (existing is null)
                return Task.FromResult(false);

            tenantsByIdentifier.Remove(existing.Identifier);
            tenantsByIdentifier[tenantInfo.Identifier] = tenantInfo;
            return Task.FromResult(true);
        }

        public Task<bool> RemoveAsync(string id, CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;

            if (RemoveResult.HasValue)
                return Task.FromResult(RemoveResult.Value);

            var existing = tenantsByIdentifier.Values.SingleOrDefault(t => t.Id == id);
            if (existing is null)
                return Task.FromResult(false);

            return RemoveByIdentifierAsync(existing.Identifier, cancellationToken);
        }

        public Task<bool> RemoveByIdentifierAsync(string identifier, CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;

            if (RemoveResult.HasValue)
                return Task.FromResult(RemoveResult.Value);

            return Task.FromResult(tenantsByIdentifier.Remove(identifier));
        }

        public Task<TenantInfo?> GetByIdentifierAsync(string identifier, CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;

            GetByIdentifierCount++;
            tenantsByIdentifier.TryGetValue(identifier, out var tenantInfo);
            return Task.FromResult(tenantInfo);
        }

        public Task<TenantInfo?> GetAsync(string id, CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;

            GetByIdCount++;
            return Task.FromResult(tenantsByIdentifier.Values.SingleOrDefault(t => t.Id == id));
        }

        public Task<IEnumerable<TenantInfo>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;

            return Task.FromResult(tenantsByIdentifier.Values.AsEnumerable());
        }

        public Task<IEnumerable<TenantInfo>> GetAllAsync(int take, int skip,
            CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;

            return Task.FromResult(tenantsByIdentifier.Values.Skip(skip).Take(take).AsEnumerable());
        }
    }

    private class TestCache : IMultiTenantStoreCache<TenantInfo>
    {
        private readonly Dictionary<string, TenantInfo> tenantsByIdentifier = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, TenantInfo> tenantsById = new(StringComparer.OrdinalIgnoreCase);

        public int GetByIdentifierCount { get; private set; }

        public bool ThrowOnGetByIdentifier { get; init; }

        public bool ThrowOnSet { get; init; }

        public bool ThrowOnRemove { get; init; }

        public List<string> SetIdentifiers { get; } = [];

        public List<string> RemovedIds { get; } = [];

        public List<string> RemovedIdentifiers { get; } = [];

        public CancellationToken LastCancellationToken { get; private set; }

        public Task<TenantInfo?> GetAsync(string id, CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;

            tenantsById.TryGetValue(id, out var tenantInfo);
            return Task.FromResult(tenantInfo);
        }

        public Task<TenantInfo?> GetByIdentifierAsync(string identifier, CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;

            if (ThrowOnGetByIdentifier)
                throw new InvalidOperationException();

            GetByIdentifierCount++;
            tenantsByIdentifier.TryGetValue(identifier, out var tenantInfo);
            return Task.FromResult(tenantInfo);
        }

        public Task SetAsync(TenantInfo tenantInfo, CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;

            if (ThrowOnSet)
                throw new InvalidOperationException();

            tenantsByIdentifier[tenantInfo.Identifier] = tenantInfo;
            tenantsById[tenantInfo.Id] = tenantInfo;
            SetIdentifiers.Add(tenantInfo.Identifier);
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string id, CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;

            if (ThrowOnRemove)
                throw new InvalidOperationException();

            tenantsById.Remove(id);
            RemovedIds.Add(id);
            return Task.CompletedTask;
        }

        public Task RemoveByIdentifierAsync(string identifier, CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;

            if (ThrowOnRemove)
                throw new InvalidOperationException();

            tenantsByIdentifier.Remove(identifier);
            RemovedIdentifiers.Add(identifier);
            return Task.CompletedTask;
        }
    }
}
