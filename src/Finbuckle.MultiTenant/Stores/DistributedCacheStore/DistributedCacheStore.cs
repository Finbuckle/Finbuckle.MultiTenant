// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Finbuckle.MultiTenant.Stores;

/// <summary>
/// Basic store that uses an IDistributedCache instance as its backing. Note that GetAllAsync is not implemented.
/// </summary>
/// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
public class DistributedCacheStore<TTenantInfo> : IMultiTenantStore<TTenantInfo> where TTenantInfo : class, ITenantInfo, new()
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

        await cache.SetStringAsync($"{keyPrefix}id__{tenantInfo.Id}", bytes, options);
        await cache.SetStringAsync($"{keyPrefix}identifier__{tenantInfo.Identifier}", bytes, options);

        return true;
    }

    /// <inheritdoc />
    public async Task<TTenantInfo?> TryGetAsync(string id)
    {
        var bytes = await cache.GetStringAsync($"{keyPrefix}id__{id}");
        if (bytes == null)
            return null;

        var result = JsonSerializer.Deserialize<TTenantInfo>(bytes);

        // Refresh the identifier version to keep things synced
        await cache.RefreshAsync($"{keyPrefix}identifier__{result?.Identifier}");

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

    /// <inheritdoc />
    public async Task<TTenantInfo?> TryGetByIdentifierAsync(string identifier)
    {
        var bytes = await cache.GetStringAsync($"{keyPrefix}identifier__{identifier}");
        if (bytes == null)
            return null;

        var result = JsonSerializer.Deserialize<TTenantInfo>(bytes);

        // Refresh the identifier version to keep things synced
        await cache.RefreshAsync($"{keyPrefix}id__{result?.Id}");

        return result;
    }

    /// <inheritdoc />
    public async Task<bool> TryRemoveAsync(string identifier)
    {
        var result = await TryGetByIdentifierAsync(identifier);
        if (result == null)
            return false;

        await cache.RemoveAsync($"{keyPrefix}id__{result.Id}");
        await cache.RemoveAsync($"{keyPrefix}identifier__{result.Identifier}");

        return true;
    }

    /// <inheritdoc />
    public Task<bool> TryUpdateAsync(TTenantInfo tenantInfo)
    {
        // Same as adding for distributed cache.
        return TryAddAsync(tenantInfo);
    }
}