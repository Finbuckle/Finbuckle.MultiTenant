// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Text.Json;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.Caching.Distributed;

namespace Finbuckle.MultiTenant.Stores.DistributedCacheStore;

/// <summary>
/// Basic store that uses an IDistributedCache instance as its backing. Note that GetAllAsync is not implemented.
/// </summary>
/// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
public class DistributedCacheStore<TTenantInfo> : IMultiTenantStore<TTenantInfo>
    where TTenantInfo : class, ITenantInfo, new()
{
    private readonly IDistributedCache cache;
    private readonly string keyPrefix;
    private readonly TimeSpan? slidingExpiration;

    /// <summary>
    /// Constructor for DistributedCacheStore.
    /// </summary>
    /// <param name="cache">IDistributedCache instance for use as the store backing.</param>
    /// <param name="keyPrefix">Prefix string added to cache entries.</param>
    /// <param name="slidingExpiration">Amount of time to slide expiration with every access.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public DistributedCacheStore(IDistributedCache cache, string keyPrefix, TimeSpan? slidingExpiration)
    {
        this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        this.keyPrefix = keyPrefix ?? throw new ArgumentNullException(nameof(keyPrefix));
        this.slidingExpiration = slidingExpiration;
    }

    /// <inheritdoc />
    public async Task<bool> TryAddAsync(TTenantInfo tenantInfo)
    {
        var options = new DistributedCacheEntryOptions { SlidingExpiration = slidingExpiration };
        var bytes = JsonSerializer.Serialize(tenantInfo);

        await cache.SetStringAsync($"{keyPrefix}id__{tenantInfo.Id}", bytes, options).ConfigureAwait(false);
        await cache.SetStringAsync($"{keyPrefix}identifier__{tenantInfo.Identifier}", bytes, options)
            .ConfigureAwait(false);

        return true;
    }

    /// <inheritdoc />
    public async Task<TTenantInfo?> TryGetAsync(string id)
    {
        var bytes = await cache.GetStringAsync($"{keyPrefix}id__{id}").ConfigureAwait(false);
        if (bytes == null)
            return null;

        var result = JsonSerializer.Deserialize<TTenantInfo>(bytes);

        // Refresh the identifier version to keep things synced
        await cache.RefreshAsync($"{keyPrefix}identifier__{result?.Identifier}").ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Not implemented in this implementation.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public Task<IEnumerable<TTenantInfo>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Not implemented in this implementation.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public Task<IEnumerable<TTenantInfo>> GetAllAsync(int take, int skip)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async Task<TTenantInfo?> TryGetByIdentifierAsync(string identifier)
    {
        var bytes = await cache.GetStringAsync($"{keyPrefix}identifier__{identifier}").ConfigureAwait(false);
        if (bytes == null)
            return null;

        var result = JsonSerializer.Deserialize<TTenantInfo>(bytes);

        // Refresh the identifier version to keep things synced
        await cache.RefreshAsync($"{keyPrefix}id__{result?.Id}").ConfigureAwait(false);

        return result;
    }

    /// <inheritdoc />
    public async Task<bool> TryRemoveAsync(string identifier)
    {
        var result = await TryGetByIdentifierAsync(identifier).ConfigureAwait(false);
        if (result == null)
            return false;

        await cache.RemoveAsync($"{keyPrefix}id__{result.Id}").ConfigureAwait(false);
        await cache.RemoveAsync($"{keyPrefix}identifier__{result.Identifier}").ConfigureAwait(false);

        return true;
    }

    /// <inheritdoc />
    public async Task<bool> TryUpdateAsync(TTenantInfo tenantInfo)
    {
        if (tenantInfo.Id is null)
            return false;
        
        var current = await TryGetAsync(tenantInfo.Id).ConfigureAwait(false);

        if (current is null)
            return false;

        return await TryRemoveAsync(current.Identifier!).ConfigureAwait(false) &&
               await TryAddAsync(tenantInfo).ConfigureAwait(false);
    }
}