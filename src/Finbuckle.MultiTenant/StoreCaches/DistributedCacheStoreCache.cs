// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Text.Json;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.Caching.Distributed;

namespace Finbuckle.MultiTenant.StoreCaches;

/// <summary>
/// Tenant store cache that uses an <see cref="IDistributedCache"/> instance as its backing.
/// </summary>
/// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
public class DistributedCacheStoreCache<TTenantInfo> : IMultiTenantStoreCache<TTenantInfo>
    where TTenantInfo : ITenantInfo
{
    private readonly IDistributedCache cache;
    private readonly string keyPrefix;
    private readonly DistributedCacheEntryOptions cacheEntryOptions;

    /// <summary>
    /// Constructor for DistributedCacheStoreCache.
    /// </summary>
    /// <param name="cache">IDistributedCache instance for use as the cache backing.</param>
    /// <param name="keyPrefix">Prefix string added to cache entries.</param>
    /// <param name="cacheEntryOptions">The cache entry options.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public DistributedCacheStoreCache(IDistributedCache cache, string keyPrefix,
        DistributedCacheEntryOptions cacheEntryOptions)
    {
        this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        this.keyPrefix = keyPrefix ?? throw new ArgumentNullException(nameof(keyPrefix));
        this.cacheEntryOptions = cacheEntryOptions ?? throw new ArgumentNullException(nameof(cacheEntryOptions));
    }

    /// <inheritdoc />
    public async Task<TTenantInfo?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        var bytes = await cache.GetStringAsync($"{keyPrefix}id__{id}", cancellationToken).ConfigureAwait(false);
        if (bytes == null)
            return default;

        var result = JsonSerializer.Deserialize<TTenantInfo>(bytes);

        await cache.RefreshAsync($"{keyPrefix}identifier__{result?.Identifier}", cancellationToken)
            .ConfigureAwait(false);

        return result;
    }

    /// <inheritdoc />
    public async Task<TTenantInfo?> GetByIdentifierAsync(string identifier, CancellationToken cancellationToken = default)
    {
        var bytes = await cache.GetStringAsync($"{keyPrefix}identifier__{identifier}", cancellationToken)
            .ConfigureAwait(false);
        if (bytes == null)
            return default;

        var result = JsonSerializer.Deserialize<TTenantInfo>(bytes);

        await cache.RefreshAsync($"{keyPrefix}id__{result?.Id}", cancellationToken).ConfigureAwait(false);

        return result;
    }

    /// <inheritdoc />
    public async Task SetAsync(TTenantInfo tenantInfo, CancellationToken cancellationToken = default)
    {
        var bytes = JsonSerializer.Serialize(tenantInfo);

        await cache.SetStringAsync($"{keyPrefix}id__{tenantInfo.Id}", bytes, cacheEntryOptions, cancellationToken)
            .ConfigureAwait(false);
        await cache.SetStringAsync($"{keyPrefix}identifier__{tenantInfo.Identifier}", bytes, cacheEntryOptions,
            cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string id, CancellationToken cancellationToken = default)
    {
        await cache.RemoveAsync($"{keyPrefix}id__{id}", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task RemoveByIdentifierAsync(string identifier, CancellationToken cancellationToken = default)
    {
        var result = await GetByIdentifierAsync(identifier, cancellationToken).ConfigureAwait(false);
        if (result is not null)
            await RemoveAsync(result.Id, cancellationToken).ConfigureAwait(false);

        await cache.RemoveAsync($"{keyPrefix}identifier__{identifier}", cancellationToken).ConfigureAwait(false);
    }
}
