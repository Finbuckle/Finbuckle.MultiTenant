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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.Core;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Finbuckle.MultiTenant.Stores
{
    /// <summary>
    /// A multitenant store decorator that adds logging.
    /// </summary>
    public class MultiTenantStoreWrapper<TStore> : IMultiTenantStore
        where TStore : IMultiTenantStore
    {
        private readonly TStore store;
        private readonly ILogger<MultiTenantStoreWrapper<TStore>> logger;

        public MultiTenantStoreWrapper(TStore store, ILogger<MultiTenantStoreWrapper<TStore>> logger)
        {
            this.store = store;
            this.logger = logger;
        }

        public async Task<TenantInfo> TryGetAsync(string id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            TenantInfo result = null;

            try
            {
                result = await store.TryGetAsync(id);
            }
            catch (Exception e)
            {
                throw new MultiTenantException($"Exception in {typeof(TStore)}.TryGetAsync. Tenant Id: \"{id}\".", e);
            }

            if (result != null)
            {
                Utilities.TryLogInfo(logger, $"{typeof(TStore)}.TryGetAsync: Tenant Id \"{id}\" found.");
            }
            else
            {
                Utilities.TryLogInfo(logger, $"{typeof(TStore)}.TryGetAsync: Unable to find Tenant Id \"{id}\".");
            }

            return result;
        }

        public async Task<TenantInfo> TryGetByIdentifierAsync(string identifier)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException(nameof(identifier));
            }

            TenantInfo result = null;

            try
            {
                result = await store.TryGetByIdentifierAsync(identifier);
            }
            catch (Exception e)
            {
                throw new MultiTenantException($"Exception in {typeof(TStore)}.TryGetByIdentifierAsync. Tenant Identifier: \"{identifier}\".", e);
            }

            if (result != null)
            {
                Utilities.TryLogInfo(logger, $"{typeof(TStore)}.TryGetByIdentifierAsync: Tenant Identifier \"{identifier}\" found.");
            }
            else
            {
                Utilities.TryLogInfo(logger, $"{typeof(TStore)}.TryGetByIdentifierAsync: Unable to find Tenant Identifier \"{identifier}\".");
            }

            return result;
        }

        public async Task<bool> TryAddAsync(TenantInfo tenantInfo)
        {
            if (tenantInfo == null)
            {
                throw new ArgumentNullException(nameof(tenantInfo));
            }

            if (tenantInfo.Id == null)
            {
                throw new ArgumentNullException(nameof(tenantInfo.Id));
            }

            if (tenantInfo.Identifier == null)
            {
                throw new ArgumentNullException(nameof(tenantInfo.Identifier));
            }

            var result = false;

            try
            {
                var existing = await TryGetAsync(tenantInfo.Id);
                if (existing != null)
                {
                    Utilities.TryLogInfo(logger, $"{typeof(TStore)}.TryAddAsync: Tenant Id: \"{tenantInfo.Id}\" already exists.");
                    return false;
                }

                existing = await TryGetByIdentifierAsync(tenantInfo.Identifier);
                if (existing != null)
                {
                    Utilities.TryLogInfo(logger, $"{typeof(TStore)}.TryAddAsync: Tenant Identifier: \"{tenantInfo.Identifier}\"  already exists.");
                    return false;
                }

                result = await store.TryAddAsync(tenantInfo);

            }
            catch (Exception e)
            {
                Utilities.TryLogError(logger, $"Exception in {typeof(TStore)}.TryAddAsync. Tenant Id: \"{tenantInfo.Id}\" Tenant Identifier: \"{tenantInfo.Identifier}\".", e);
            }

            if (result)
            {
                Utilities.TryLogInfo(logger, $"{typeof(TStore)}.TryAddAsync: Tenant Id: \"{tenantInfo.Id}\" Tenant Identifier: \"{tenantInfo.Identifier}\" added.");
            }
            else
            {
                Utilities.TryLogInfo(logger, $"{typeof(TStore)}.TryAddAsync: Unable to add Tenant Id: \"{tenantInfo.Id}\" Tenant Identifier: \"{tenantInfo.Identifier}\".");
            }

            return result;
        }

        public async Task<bool> TryRemoveAsync(string id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            var result = false;

            try
            {
                result = await store.TryRemoveAsync(id);
            }
            catch (Exception e)
            {
                Utilities.TryLogError(logger, $"Exception in {typeof(TStore)}.TryRemoveAsync for Tenant Id: \"{id}\".", e);
            }

            if (result)
            {
                Utilities.TryLogInfo(logger, $"{typeof(TStore)}.TryRemoveAsync: Tenant Id: \"{id}\" removed.");
            }
            else
            {
                Utilities.TryLogInfo(logger, $"{typeof(TStore)}.TryRemoveAsync: Unable to remove Tenant Id: \"{id}\".");
            }

            return result;
        }

        public async Task<bool> TryUpdateAsync(TenantInfo tenantInfo)
        {
            if (tenantInfo == null)
            {
                throw new ArgumentNullException(nameof(tenantInfo));
            }

            if (tenantInfo.Id == null)
            {
                throw new ArgumentNullException(nameof(tenantInfo.Id));
            }

            var result = false;

            try
            {
                var existing = await TryGetAsync(tenantInfo.Identifier);
                if (existing != null)
                {
                    Utilities.TryLogInfo(logger, $"{typeof(TStore)}.TryUpdateAsync: Tenant Identifier: \"{tenantInfo.Identifier}\" already exists.");
                    return false;
                }

                result = await store.TryUpdateAsync(tenantInfo);

            }
            catch (Exception e)
            {
                Utilities.TryLogError(logger, $"Exception in {typeof(TStore)}.TryUpdateAsync. Tenant Id: \"{tenantInfo.Id}\".", e);
            }

            if (result)
            {
                Utilities.TryLogInfo(logger, $"{typeof(TStore)}.TryUpdateAsync: Tenant Id: \"{tenantInfo.Id}\" updated.");
            }
            else
            {
                Utilities.TryLogInfo(logger, $"{typeof(TStore)}.TryUpdateAsync: Unable to update Tenant Id: \"{tenantInfo.Id}\".");
            }

            return result;
        }
    }
}