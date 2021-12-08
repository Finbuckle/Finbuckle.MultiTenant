// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Finbuckle.MultiTenant.Stores
{
    public class ConfigurationStore<TTenantInfo> : IMultiTenantStore<TTenantInfo> where TTenantInfo : class, ITenantInfo, new()
    {
        private const string defaultSectionName = "Finbuckle:MultiTenant:Stores:ConfigurationStore";
        private readonly IConfigurationSection section;
        private ConcurrentDictionary<string, TTenantInfo>? tenantMap;

        public ConfigurationStore(IConfiguration configuration) : this(configuration, defaultSectionName)
        {
        }

        public ConfigurationStore(IConfiguration configuration, string sectionName)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (string.IsNullOrEmpty(sectionName))
            {
                throw new ArgumentException("Section name provided to the Configuration Store is null or empty.", nameof(sectionName));
            }

            section = configuration.GetSection(sectionName);
            if(!section.Exists())
            {
                throw new MultiTenantException("Section name provided to the Configuration Store is invalid.");
            }

            UpdateTenantMap();
            ChangeToken.OnChange(() => section.GetReloadToken(), UpdateTenantMap);
        }

        private void UpdateTenantMap()
        {
            var newMap = new ConcurrentDictionary<string, TTenantInfo>(StringComparer.OrdinalIgnoreCase);
            var tenants = section.GetSection("Tenants").GetChildren();

            foreach(var tenantSection in tenants)
            {
                var newTenant = section.GetSection("Defaults").Get<TTenantInfo>(options => options.BindNonPublicProperties = true) ?? new TTenantInfo();
                tenantSection.Bind(newTenant, options => options.BindNonPublicProperties = true);

                // Throws an ArgumentNullException if the identifier is null.
                newMap.TryAdd(newTenant.Identifier!, newTenant);
            }

            tenantMap = newMap;
        }

        public Task<bool> TryAddAsync(TTenantInfo tenantInfo)
        {
            throw new NotImplementedException();
        }

        public async Task<TTenantInfo?> TryGetAsync(string id)
        {
            if (id is null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            return await Task.FromResult(tenantMap?.Where(kv => kv.Value.Id == id).SingleOrDefault().Value);
        }

        public async Task<IEnumerable<TTenantInfo>> GetAllAsync()
        {
            return await Task.FromResult(tenantMap?.Select(x => x.Value).ToList() ?? new List<TTenantInfo>());
        }

        public async Task<TTenantInfo?> TryGetByIdentifierAsync(string identifier)
        {
            if (identifier is null)
            {
                throw new ArgumentNullException(nameof(identifier));
            }

            if (tenantMap is null)
            {
                return null;
            }

            return await Task.FromResult(tenantMap.TryGetValue(identifier, out var result) ? result : null);
        }

        public Task<bool> TryRemoveAsync(string identifier)
        {
            throw new NotImplementedException();
        }

        public Task<bool> TryUpdateAsync(TTenantInfo tenantInfo)
        {
            throw new NotImplementedException();
        }
    }
}