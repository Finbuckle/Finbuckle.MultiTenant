//    Copyright 2018 Andrew White
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
using System.Threading.Tasks;
using Finbuckle.MultiTenant.Core;
using Microsoft.Extensions.Logging;

namespace Finbuckle.MultiTenant.Stores
{
    /// <summary>
    /// A basic multitenant Store that runs in local memory. Ignores string case by default.
    /// </summary>
    public class InMemoryStore : IMultiTenantStore
    {
        private readonly ConcurrentDictionary<string, TenantInfo> tenantMap;
        private readonly ILogger<InMemoryStore> logger;

        public InMemoryStore() : this (true, null)
        {
        }

        public InMemoryStore(bool igoreCase) : this (igoreCase, null)
        {
        }

        public InMemoryStore(ILogger<InMemoryStore> logger) : this (true, logger)
        {
        }

        public InMemoryStore(bool ignoreCase, ILogger<InMemoryStore> logger)
        {
            var stringComparerer = StringComparer.OrdinalIgnoreCase;
            if(!ignoreCase)
                stringComparerer = StringComparer.Ordinal;
                
            tenantMap = new ConcurrentDictionary<string, TenantInfo>(stringComparerer);
            this.logger = logger;
        }

        public virtual async Task<TenantInfo> GetByIdentifierAsync(string identifier)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException(nameof(identifier));
            }

            tenantMap.TryGetValue(identifier, out var result);
            Utilities.TryLogInfo(logger, $"Tenant Id \"{result?.Id ?? "<null>"}\" found in store for identifier \"{identifier}\".");
            return await Task.FromResult(result).ConfigureAwait(false);
        }

        public virtual Task<bool> TryAddAsync(TenantInfo tenantInfo)
        {
            if (tenantInfo == null)
            {
                throw new ArgumentNullException(nameof(tenantInfo));
            }

            var result = tenantMap.TryAdd(tenantInfo.Identifier, tenantInfo);

            if(result)
            {
                Utilities.TryLogInfo(logger, $"Tenant \"{tenantInfo.Identifier}\" added to InMemoryStore.");
            }
            else
            {
                Utilities.TryLogInfo(logger, $"Unable to add tenant \"{tenantInfo.Identifier}\" to InMemoryStore.");
            }

            return Task.FromResult(result);
        }

        public virtual Task<bool> TryRemoveAsync(string identifier)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException(nameof(identifier));
            }

            var result = tenantMap.TryRemove(identifier, out var dummy);

            if(result)
            {
                Utilities.TryLogInfo(logger, $"Tenant \"{identifier}\" removed from InMemoryStore.");
            }
            else
            {
                Utilities.TryLogInfo(logger, $"Unable to remove tenant \"{identifier}\" from InMemoryStore.");
            }

            return Task.FromResult(result);
        }
    }
}