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
    /// A basic Tenant Store that runs in local memory. Ignores string case by default.
    /// </summary>
    public class InMemoryMultiTenantStore : IMultiTenantStore
    {
        public InMemoryMultiTenantStore() : this (true, null)
        {
        }

        public InMemoryMultiTenantStore(bool igoreCase) : this (igoreCase, null)
        {
        }

        public InMemoryMultiTenantStore(ILogger<InMemoryMultiTenantStore> logger) : this (true, logger)
        {
        }

        public InMemoryMultiTenantStore(bool ignoreCase, ILogger<InMemoryMultiTenantStore> logger)
        {
            var stringComparerer = StringComparer.OrdinalIgnoreCase;
            if(!ignoreCase)
                stringComparerer = StringComparer.Ordinal;
                
            _tenantMap = new ConcurrentDictionary<string, TenantContext>(stringComparerer);
            this.logger = logger;
        }

        private readonly ConcurrentDictionary<string, TenantContext> _tenantMap;
        private readonly ILogger<InMemoryMultiTenantStore> logger;

        public virtual async Task<TenantContext> GetByIdentifierAsync(string identifier)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException(nameof(identifier));
            }

            _tenantMap.TryGetValue(identifier, out var result);
            Utilities.TryLogInfo(logger, $"Tenant Id \"{result?.Id ?? "<null>"}\" found in store for identifier \"{identifier}\".");
            return await Task.FromResult(result).ConfigureAwait(false);
        }

        public virtual Task<bool> TryAdd(TenantContext context)
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

        public virtual Task<bool> TryRemove(string identifier)
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