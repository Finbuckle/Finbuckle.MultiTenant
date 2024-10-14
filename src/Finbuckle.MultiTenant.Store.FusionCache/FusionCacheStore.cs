using System.Text.Json;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.Caching.Distributed;
using ZiggyCreatures.Caching.Fusion;

namespace Finbuckle.MultiTenant.Store.FusionCache;

/// <summary>
///  A store for tenant information that uses FusionCache as the backing store.Note that GetAllAsync is not implemented.
/// </summary>
/// <typeparam name="TTenantInfo"> The type of tenant information.</typeparam>
public class FusionCacheStore<TTenantInfo> : IMultiTenantStore<TTenantInfo> where TTenantInfo : class, ITenantInfo, new()
{
    private readonly IFusionCache cache;
    private readonly string keyPrefix;
    private readonly TimeSpan? slidingExpiration;

    /// <summary>
    /// Constructor for FusionCacheStore.
    /// </summary>
    /// <param name="cache">IFusionCache instance for use as the store backing.</param>
    /// <param name="keyPrefix">Prefix string added to cache entries.</param>
    /// <param name="slidingExpiration">Amount of time to slide expiration with every access.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public FusionCacheStore(IFusionCache cache, string keyPrefix, TimeSpan? slidingExpiration)
    {
        this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        this.keyPrefix = keyPrefix ?? throw new ArgumentNullException(nameof(keyPrefix));
        this.slidingExpiration = slidingExpiration;
    }

    /// <inheritdoc />
    public async Task<bool> TryAddAsync(TTenantInfo tenantInfo)
    {
        var options = new FusionCacheEntryOptions()
        {
            Duration = slidingExpiration ?? TimeSpan.FromSeconds(0)
        };
        await cache.SetAsync($"{keyPrefix}id__{tenantInfo.Id}", tenantInfo, options);
        await cache.SetAsync($"{keyPrefix}identifier__{tenantInfo.Identifier}", tenantInfo, options);
        return true;
    }

    /// <inheritdoc />
    public async Task<TTenantInfo?> TryGetAsync(string id)
    {
        
        var result =await cache.GetOrDefaultAsync<TTenantInfo?>($"{keyPrefix}id__{id}");

        // Refresh the identifier version to keep things synced
       if(result!=null)
        {
          await  cache.SetAsync($"{keyPrefix}identifier__{result.Identifier}", result, new FusionCacheEntryOptions()
            {
                Duration = slidingExpiration ?? TimeSpan.FromSeconds(0) //  if slidingExpiration is null, set to 0 and skip cache
            });
        }
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
        var result =await cache.GetOrDefaultAsync<TTenantInfo?>($"{keyPrefix}identifier__{identifier}");
        
        if(result!=null)
        {
          await  cache.SetAsync($"{keyPrefix}identifier__{result.Identifier}", result, new FusionCacheEntryOptions()
            {
                Duration = slidingExpiration ?? TimeSpan.FromSeconds(0) //  if slidingExpiration is null, set to 0 and skip cache
            });
        }
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