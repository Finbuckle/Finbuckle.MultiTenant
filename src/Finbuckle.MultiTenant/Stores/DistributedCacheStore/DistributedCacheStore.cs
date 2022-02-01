// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace Finbuckle.MultiTenant.Stores
{
    public class DistributedCacheStore<TTenantInfo> : IMultiTenantStore<TTenantInfo> where TTenantInfo : class, ITenantInfo, new()
    {
        private readonly IDistributedCache cache;
        private readonly string keyPrefix;
        private readonly TimeSpan? slidingExpiration;

        public DistributedCacheStore(IDistributedCache cache, string keyPrefix, TimeSpan? slidingExpiration)
        {
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
            this.keyPrefix = keyPrefix ?? throw new ArgumentNullException(nameof(keyPrefix));
            this.slidingExpiration = slidingExpiration;
        }

        public async Task<bool> TryAddAsync(TTenantInfo tenantInfo)
        {
            var options = new DistributedCacheEntryOptions { SlidingExpiration = slidingExpiration };
            var bytes = JsonSerializer.Serialize(tenantInfo);

            await cache.SetStringAsync($"{keyPrefix}id__{tenantInfo.Id}", bytes, options);
            await cache.SetStringAsync($"{keyPrefix}identifier__{tenantInfo.Identifier}", bytes, options);

            return true;
        }

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

        public Task<IEnumerable<TTenantInfo>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

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

        public async Task<bool> TryRemoveAsync(string identifier)
        {
            var result = await TryGetByIdentifierAsync(identifier);
            if (result == null)
                return false;

            await cache.RemoveAsync($"{keyPrefix}id__{result.Id}");
            await cache.RemoveAsync($"{keyPrefix}identifier__{result.Identifier}");

            return true;
        }

        public Task<bool> TryUpdateAsync(TTenantInfo tenantInfo)
        {
            // Same as adding for distributed cache.
            return TryAddAsync(tenantInfo);
        }
    }
}