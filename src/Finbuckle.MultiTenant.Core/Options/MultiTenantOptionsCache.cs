//    Copyright 2018-2020 Andrew White
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Options;

namespace Finbuckle.MultiTenant.Options
{
    /// <summary>
    /// Adds, retrieves, and removes instances of TOptions after adjusting them for the current TenantContext.
    /// </summary>
    public class MultiTenantOptionsCache<TOptions, TTenantInfo> : IOptionsMonitorCache<TOptions>
        where TOptions : class
        where TTenantInfo : class, ITenantInfo, new()
    {
        private readonly IMultiTenantContextAccessor<TTenantInfo> multiTenantContextAccessor;

        // The object is just a dummy because there is no ConcurrentSet<T> class.
        //private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, object>> _adjustedOptionsNames =
        //  new ConcurrentDictionary<string, ConcurrentDictionary<string, object>>();

        private readonly ConcurrentDictionary<string, IOptionsMonitorCache<TOptions>> map = new ConcurrentDictionary<string, IOptionsMonitorCache<TOptions>>();

        public MultiTenantOptionsCache(IMultiTenantContextAccessor<TTenantInfo> multiTenantContextAccessor)
        {
            this.multiTenantContextAccessor = multiTenantContextAccessor ?? throw new ArgumentNullException(nameof(multiTenantContextAccessor));
        }

        /// <summary>
        /// Clears all cached options for the current tenant.
        /// </summary>
        public void Clear()
        {
            var tenantId = multiTenantContextAccessor.MultiTenantContext?.TenantInfo?.Id ?? "";
            var cache = map.GetOrAdd(tenantId, new OptionsCache<TOptions>());

            cache.Clear();
        }

        /// <summary>
        /// Clears all cached options for the given tenant.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant which will have its options cleared.</param>
        public void Clear(string tenantId)
        {
            tenantId = tenantId ?? "";
            var cache = map.GetOrAdd(tenantId, new OptionsCache<TOptions>());

            cache.Clear();
        }

        /// <summary>
        /// Clears all cached options for all tenants and no tenant.
        /// </summary>
        public void ClearAll()
        {
            foreach(var cache in map.Values)
                cache.Clear();
        }

        /// <summary>
        /// Gets a named options instance for the current tenant, or adds a new instance created with createOptions.
        /// </summary>
        /// <param name="name">The options name.</param>
        /// <param name="createOptions">The factory function for creating the options instance.</param>
        /// <returns>The existing or new options instance.</returns>
        public TOptions GetOrAdd(string name, Func<TOptions> createOptions)
        {
            if (createOptions == null)
            {
                throw new ArgumentNullException(nameof(createOptions));
            }

            name = name ?? Microsoft.Extensions.Options.Options.DefaultName;
            var tenantId = multiTenantContextAccessor.MultiTenantContext?.TenantInfo?.Id ?? "";
            var cache = map.GetOrAdd(tenantId, new OptionsCache<TOptions>());

            return cache.GetOrAdd(name, createOptions);
        }

        /// <summary>
        /// Tries to adds a new option to the cache for the current tenant.
        /// </summary>
        /// <param name="name">The options name.</param>
        /// <param name="options">The options instance.</param>
        /// <returns>True if the options was added to the cache for the current tenant.</returns>
        public bool TryAdd(string name, TOptions options)
        {
            name = name ?? Microsoft.Extensions.Options.Options.DefaultName;
            var tenantId = multiTenantContextAccessor.MultiTenantContext?.TenantInfo?.Id ?? "";
            var cache = map.GetOrAdd(tenantId, new OptionsCache<TOptions>());

            return cache.TryAdd(name, options);
        }

        /// <summary>
        /// Try to remove an options instance for the current tenant.
        /// </summary>
        /// <param name="name">The options name.</param>
        /// <returns>True if the options was removed from the cache for the current tenant.</returns>
        public bool TryRemove(string name)
        {
            name = name ?? Microsoft.Extensions.Options.Options.DefaultName;
            var tenantId = multiTenantContextAccessor.MultiTenantContext?.TenantInfo?.Id ?? "";
            var cache = map.GetOrAdd(tenantId, new OptionsCache<TOptions>());

            return cache.TryRemove(name);
        }
    }
}