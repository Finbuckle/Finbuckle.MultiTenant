using System;
using System.Collections.Concurrent;

namespace Finbuckle.MultiTenant.AspNetCore.OptionsCacheReset.Internal
{
    internal class TenantVersionStore
    {
        private readonly ConcurrentDictionary<string, int> _tenantVersionDictionary =
            new ConcurrentDictionary<string, int>(StringComparer.Ordinal);

        /// <summary>
        /// Gets tenant version 
        /// </summary>
        /// <param name="tenantId">Tenant id</param>
        /// <returns> Tenant version </returns>
        public int GetVersion(string tenantId)
        {
            return _tenantVersionDictionary.TryGetValue(tenantId, out var version) ? version : 0;
        }

        /// <summary>
        /// Set tenant version
        /// </summary>
        /// <param name="tenantId">Tenant id</param>
        /// <param name="newTenantVersion"> New tenant version</param>
        /// <returns>New tenant version</returns>
        public int SetVersion(string tenantId, int newTenantVersion)
        {
            return _tenantVersionDictionary.AddOrUpdate(tenantId, newTenantVersion, (s, i) => newTenantVersion);
        }
    }
}