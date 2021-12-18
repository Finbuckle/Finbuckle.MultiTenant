// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Finbuckle.MultiTenant.Stores
{
    public class InMemoryStore<TTenantInfo> : IMultiTenantStore<TTenantInfo>
        where TTenantInfo : class, ITenantInfo, new()
    {
        private readonly ConcurrentDictionary<string, TTenantInfo> tenantMap;
        private readonly InMemoryStoreOptions<TTenantInfo> options;

        public InMemoryStore(IOptions<InMemoryStoreOptions<TTenantInfo>> options)
        {
            this.options = options?.Value ?? new InMemoryStoreOptions<TTenantInfo>();

            var stringComparer = StringComparer.OrdinalIgnoreCase;
            if(this.options.IsCaseSensitive)
                stringComparer = StringComparer.Ordinal;

            tenantMap = new ConcurrentDictionary<string, TTenantInfo>(stringComparer);
            foreach(var tenant in this.options.Tenants)
            {
                if(String.IsNullOrWhiteSpace(tenant.Id))
                    throw new MultiTenantException("Missing tenant id in options.");
                if(String.IsNullOrWhiteSpace(tenant.Identifier))
                    throw new MultiTenantException("Missing tenant identifier in options.");
                if(tenantMap.ContainsKey(tenant.Identifier))
                    throw new MultiTenantException("Duplicate tenant identifier in options.");

                tenantMap.TryAdd(tenant.Identifier, tenant);
            }
        }

        public virtual async Task<TTenantInfo?> TryGetAsync(string id)
        {
            var result = tenantMap.Values.Where(ti => ti.Id == id).SingleOrDefault();

            return await Task.FromResult(result);
        }

        public virtual async Task<TTenantInfo?> TryGetByIdentifierAsync(string identifier)
        {
            tenantMap.TryGetValue(identifier, out var result);

            return await Task.FromResult(result);
        }

        public async Task<IEnumerable<TTenantInfo>> GetAllAsync()
        {
            return await Task.FromResult(tenantMap.Select(x => x.Value).ToList());
        }

        public async Task<bool> TryAddAsync(TTenantInfo tenantInfo)
        {
            var result = tenantInfo.Identifier != null && tenantMap.TryAdd(tenantInfo.Identifier, tenantInfo);

            return await Task.FromResult(result);
        }

        public async Task<bool> TryRemoveAsync(string identifier)
        {
            var result = tenantMap.TryRemove(identifier, out var _);

            return await Task.FromResult(result);
        }

        public async Task<bool> TryUpdateAsync(TTenantInfo tenantInfo)
        {
            var existingTenantInfo = tenantInfo.Id != null ? await TryGetAsync(tenantInfo.Id) : null;

            if (existingTenantInfo?.Identifier != null)
            {
                var result =  tenantMap.TryUpdate(existingTenantInfo.Identifier, tenantInfo, existingTenantInfo);
                return await Task.FromResult(result);
            }

            return await Task.FromResult(false);
        }
    }
}