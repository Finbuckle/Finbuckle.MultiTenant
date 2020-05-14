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
using System.Linq;
using System.Threading.Tasks;

namespace Finbuckle.MultiTenant.Stores
{
    public class InMemoryStore<TTenantInfo> : IMultiTenantStore<TTenantInfo>
        where TTenantInfo : class, ITenantInfo, new()
    {
        private readonly ConcurrentDictionary<string, TTenantInfo> tenantMap;

        public InMemoryStore() : this (true)
        {
        }

        public InMemoryStore(bool ignoreCase)
        {
            var stringComparerer = StringComparer.OrdinalIgnoreCase;
            if(!ignoreCase)
                stringComparerer = StringComparer.Ordinal;
                
            tenantMap = new ConcurrentDictionary<string, TTenantInfo>(stringComparerer);
        }

        public virtual async Task<TTenantInfo> TryGetAsync(string id)
        {
            var result = tenantMap.Values.Where(ti => ti.Id == id).SingleOrDefault();
            
            return await Task.FromResult(result);
        }

        public virtual async Task<TTenantInfo> TryGetByIdentifierAsync(string identifier)
        {
            tenantMap.TryGetValue(identifier, out var result);
            
            return await Task.FromResult(result);
        }

        public async Task<bool> TryAddAsync(TTenantInfo tenantInfo)
        {
            var result = tenantMap.TryAdd(tenantInfo.Identifier, tenantInfo);

            return await Task.FromResult(result);
        }

        public async Task<bool> TryRemoveAsync(string identifier)
        {
            var result = tenantMap.TryRemove(identifier, out var dummy);

            return await Task.FromResult(result);
        }

        public async Task<bool> TryUpdateAsync(TTenantInfo tenantInfo)
        {
            var existingTenantInfo = await TryGetAsync(tenantInfo.Id);

            if(existingTenantInfo != null)
            {
                existingTenantInfo = tenantInfo;
            }

            return existingTenantInfo != null;
        }
    }
}