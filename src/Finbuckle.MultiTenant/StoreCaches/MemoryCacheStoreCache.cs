// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.Caching.Memory;

namespace Finbuckle.MultiTenant.StoreCaches;

/// <summary>
/// Tenant store cache that uses an <see cref="IMemoryCache"/> instance as its backing.
/// </summary>
/// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
public class MemoryCacheStoreCache<TTenantInfo> : IMultiTenantStoreCache<TTenantInfo>
    where TTenantInfo : ITenantInfo
{
    private readonly IMemoryCache cache;
    private readonly string keyPrefix;
    private readonly MemoryCacheEntryOptions cacheEntryOptions;

    /// <summary>
    /// Constructor for MemoryCacheStoreCache.
    /// </summary>
    /// <param name="cache">IMemoryCache instance for use as the cache backing.</param>
    /// <param name="keyPrefix">Prefix string added to cache entries.</param>
    /// <param name="cacheEntryOptions">The cache entry options.</param>
    public MemoryCacheStoreCache(IMemoryCache cache, string keyPrefix, MemoryCacheEntryOptions cacheEntryOptions)
    {
        this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        this.keyPrefix = keyPrefix ?? throw new ArgumentNullException(nameof(keyPrefix));
        this.cacheEntryOptions = cacheEntryOptions ?? throw new ArgumentNullException(nameof(cacheEntryOptions));
    }

    /// <inheritdoc />
    public Task<TTenantInfo?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        cache.TryGetValue($"{keyPrefix}id__{id}", out TTenantInfo? result);
        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task<TTenantInfo?> GetByIdentifierAsync(string identifier, CancellationToken cancellationToken = default)
    {
        cache.TryGetValue($"{keyPrefix}identifier__{identifier}", out TTenantInfo? result);
        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task SetAsync(TTenantInfo tenantInfo, CancellationToken cancellationToken = default)
    {
        cache.Set($"{keyPrefix}id__{tenantInfo.Id}", tenantInfo, cacheEntryOptions);
        cache.Set($"{keyPrefix}identifier__{tenantInfo.Identifier}", tenantInfo, cacheEntryOptions);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RemoveAsync(string id, CancellationToken cancellationToken = default)
    {
        cache.Remove($"{keyPrefix}id__{id}");

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task RemoveByIdentifierAsync(string identifier, CancellationToken cancellationToken = default)
    {
        var result = await GetByIdentifierAsync(identifier, cancellationToken).ConfigureAwait(false);
        if (result is not null)
            await RemoveAsync(result.Id, cancellationToken).ConfigureAwait(false);

        cache.Remove($"{keyPrefix}identifier__{identifier}");
    }
}
