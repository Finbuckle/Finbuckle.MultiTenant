using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.Core.Abstractions;
using Microsoft.Extensions.Logging;

namespace Finbuckle.MultiTenant.Core
{
    /// <summary>
    /// A basic Tenant Store that runs in local memory. Ignores string case by default.
    /// </summary>
    public class InMemoryMultiTenantStore : IMultiTenantStore
    {
        public InMemoryMultiTenantStore(bool ignoreCase = true, ILogger<InMemoryMultiTenantStore> logger = null)
        {
            var stringComparerer = StringComparer.OrdinalIgnoreCase;
            if(!ignoreCase)
                stringComparerer = StringComparer.Ordinal;
                
            _tenantMap = new ConcurrentDictionary<string, TenantContext>(stringComparerer);
            this.logger = logger;
        }

        private readonly ConcurrentDictionary<string, TenantContext> _tenantMap;
        private readonly ILogger<InMemoryMultiTenantStore> logger;

        public async Task<TenantContext> GetByIdentifierAsync(string identifier)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException(nameof(identifier));
            }

            _tenantMap.TryGetValue(identifier, out var result);
            Utilities.TryLogInfo(logger, $"Tenant Id \"{result?.Id ?? "<null>"}\"found in store for identifier \"{identifier}\".");
            return await Task.FromResult(result).ConfigureAwait(false);
        }

        public Task<bool> TryAdd(TenantContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var result = _tenantMap.TryAdd(context.Identifier, context);

            if(result)
            {
                Utilities.TryLogInfo(logger, $"Tenant \"{context.Identifier}\" added to InMemoryMultiTenantStore.");
            }
            else
            {
                Utilities.TryLogInfo(logger, $"Unable to add tenant \"{context.Identifier}\" to InMemoryMultiTenantStore.");
            }

            return Task.FromResult(result);
        }

        public Task<bool> TryRemove(string identifier)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException(nameof(identifier));
            }

            var result = _tenantMap.TryRemove(identifier, out var dummy);

            if(result)
            {
                Utilities.TryLogInfo(logger, $"Tenant \"{identifier}\" removed from InMemoryMultiTenantStore.");
            }
            else
            {
                Utilities.TryLogInfo(logger, $"Unable to remove tenant \"{identifier}\" from InMemoryMultiTenantStore.");
            }

            return Task.FromResult(result);
        }
    }
}