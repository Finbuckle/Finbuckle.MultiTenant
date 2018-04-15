using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.Core.Abstractions;

namespace Finbuckle.MultiTenant.Core
{
    /// <summary>
    /// A basic Tenant Store that runs in local memory. Ignores string case by default.
    /// </summary>
    public class InMemoryMultiTenantStore : IMultiTenantStore
    {
        public InMemoryMultiTenantStore(bool ignoreCase = true)
        {
            var stringComparerer = StringComparer.OrdinalIgnoreCase;
            if(!ignoreCase)
                stringComparerer = StringComparer.Ordinal;
                
            _tenantMap = new ConcurrentDictionary<string, TenantContext>(stringComparerer);
        }

        private readonly ConcurrentDictionary<string, TenantContext> _tenantMap;

        public async Task<TenantContext> GetByIdentifierAsync(string identifier)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException(nameof(identifier));
            }

            _tenantMap.TryGetValue(identifier, out var result);
            return await Task.FromResult(result).ConfigureAwait(false);
        }

        public Task<bool> TryAdd(TenantContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return Task.FromResult(_tenantMap.TryAdd(context.Identifier, context));
        }

        public Task<bool> TryRemove(string identifier)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException(nameof(identifier));
            }

            return Task.FromResult(_tenantMap.TryRemove(identifier, out var dummy));
        }
    }
}