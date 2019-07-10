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
    /// A multitenant store decorator that handles exception handling and logging.
    /// </summary>
    public class MultiTenantStoreWrapper<TStore> : IMultiTenantStore
        where TStore : IMultiTenantStore
    {
        public TStore Store { get; }
        private readonly ILogger logger;

        public MultiTenantStoreWrapper(TStore store, ILogger<TStore> logger)
        {
            this.Store = store;
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
                result = await Store.TryGetAsync(id);
            }
            catch (Exception e)
            {
                var errorMessage = $"Exception in {typeof(TStore)}.TryGetAsync.";
                Utilities.TryLogError(logger, errorMessage, e);
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
                result = await Store.TryGetByIdentifierAsync(identifier);
            }
            catch (Exception e)
            {
                var errorMessage = $"Exception in {typeof(TStore)}.TryGetByIdentifierAsync.";
                Utilities.TryLogError(logger, errorMessage, e);
            }

            if (result != null)
            {
                Utilities.TryLogInfo(logger, $"{typeof(TStore)}.TryGetByIdentifierAsync: Tenant found with identifier \"{identifier}\".");
            }
            else
            {
                Utilities.TryLogInfo(logger, $"{typeof(TStore)}.TryGetByIdentifierAsync: Unable to find Tenant with identifier \"{identifier}\".");
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
                    Utilities.TryLogInfo(logger, $"{typeof(TStore)}.TryAddAsync: Tenant already exists. Id: \"{tenantInfo.Id}\", Identifier: \"{tenantInfo.Identifier}\"");
                    goto end;
                }

                existing = await TryGetByIdentifierAsync(tenantInfo.Identifier);
                if (existing != null)
                {
                    Utilities.TryLogInfo(logger, $"{typeof(TStore)}.TryAddAsync: Tenant already exists. Id: \"{tenantInfo.Id}\", Identifier: \"{tenantInfo.Identifier}\"");
                    goto end;
                }

                result = await Store.TryAddAsync(tenantInfo);

            }
            catch (Exception e)
            {
                var errorMessage = $"Exception in {typeof(TStore)}.TryAddAsync.";
                Utilities.TryLogError(logger, errorMessage, e);
            }

        end:
            if (result)
            {
                Utilities.TryLogInfo(logger, $"{typeof(TStore)}.TryAddAsync: Tenant added. Id: \"{tenantInfo.Id}\", Identifier: \"{tenantInfo.Identifier}\"");
            }
            else
            {
                Utilities.TryLogInfo(logger, $"{typeof(TStore)}.TryAddAsync: Unable to add Tenant. Id: \"{tenantInfo.Id}\", Identifier: \"{tenantInfo.Identifier}\"");
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
                result = await Store.TryRemoveAsync(id);
            }
            catch (Exception e)
            {
                var errorMessage = $"Exception in {typeof(TStore)}.TryRemoveAsync.";
                Utilities.TryLogError(logger, errorMessage, e);
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
                    goto end;
                }

                result = await Store.TryUpdateAsync(tenantInfo);

            }
            catch (Exception e)
            {
                var errorMessage = $"Exception in {typeof(TStore)}.TryUpdateAsync.";
                Utilities.TryLogError(logger, errorMessage, e);
            }

        end:
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