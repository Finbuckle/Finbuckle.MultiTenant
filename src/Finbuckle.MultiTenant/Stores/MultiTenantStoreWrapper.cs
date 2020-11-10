//    Copyright 2018-2020 Finbuckle LLC, Andrew White, and Contributors
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
using System.Collections.Generic;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.Internal;
using Microsoft.Extensions.Logging;

namespace Finbuckle.MultiTenant.Stores
{
    /// <summary>
    /// A multitenant store decorator that handles exception handling and logging.
    /// </summary>
    public class MultiTenantStoreWrapper<TTenantInfo> : IMultiTenantStore<TTenantInfo>
        where TTenantInfo : class, ITenantInfo, new()
    {
        public IMultiTenantStore<TTenantInfo> Store { get; }
        private readonly ILogger logger;

        public MultiTenantStoreWrapper(IMultiTenantStore<TTenantInfo> store, ILogger logger)
        {
            this.Store = store;
            this.logger = logger;
        }

        public async Task<TTenantInfo> TryGetAsync(string id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            TTenantInfo result = null;

            try
            {
                result = await Store.TryGetAsync(id);
            }
            catch (Exception e)
            {
                var errorMessage = $"Exception in {Store.GetType()}.TryGetAsync.";
                Utilities.TryLogError(logger, errorMessage, e);
            }

            if (result != null)
            {
                Utilities.TryLogDebug(logger, $"{Store.GetType()}.TryGetAsync: Tenant Id \"{id}\" found.");
            }
            else
            {
                Utilities.TryLogDebug(logger, $"{Store.GetType()}.TryGetAsync: Unable to find Tenant Id \"{id}\".");
            }

            return result;
        }

        public async Task<IEnumerable<TTenantInfo>> GetAllAsync()
        {
            IEnumerable<TTenantInfo> result = null;

            try
            {
                result = await Store.GetAllAsync();
            }
            catch (Exception e)
            {
                var errorMessage = $"Exception in {Store.GetType()}.GetAllAsync.";
                Utilities.TryLogError(logger, errorMessage, e);
            }

            return result;
        }

        public async Task<TTenantInfo> TryGetByIdentifierAsync(string identifier)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException(nameof(identifier));
            }

            TTenantInfo result = null;

            try
            {
                result = await Store.TryGetByIdentifierAsync(identifier);
            }
            catch (Exception e)
            {
                var errorMessage = $"Exception in {Store.GetType()}.TryGetByIdentifierAsync.";
                Utilities.TryLogError(logger, errorMessage, e);
            }

            if (result != null)
            {
                Utilities.TryLogDebug(logger, $"{Store.GetType()}.TryGetByIdentifierAsync: Tenant found with identifier \"{identifier}\".");
            }
            else
            {
                Utilities.TryLogDebug(logger, $"{Store.GetType()}.TryGetByIdentifierAsync: Unable to find Tenant with identifier \"{identifier}\".");
            }

            return result;
        }

        public async Task<bool> TryAddAsync(TTenantInfo tenantInfo)
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
                    Utilities.TryLogDebug(logger, $"{Store.GetType()}.TryAddAsync: Tenant already exists. Id: \"{tenantInfo.Id}\", Identifier: \"{tenantInfo.Identifier}\"");
                    goto end;
                }

                existing = await TryGetByIdentifierAsync(tenantInfo.Identifier);
                if (existing != null)
                {
                    Utilities.TryLogDebug(logger, $"{Store.GetType()}.TryAddAsync: Tenant already exists. Id: \"{tenantInfo.Id}\", Identifier: \"{tenantInfo.Identifier}\"");
                    goto end;
                }

                result = await Store.TryAddAsync(tenantInfo);

            }
            catch (Exception e)
            {
                var errorMessage = $"Exception in {Store.GetType()}.TryAddAsync.";
                Utilities.TryLogError(logger, errorMessage, e);
            }

        end:
            if (result)
            {
                Utilities.TryLogDebug(logger, $"{Store.GetType()}.TryAddAsync: Tenant added. Id: \"{tenantInfo.Id}\", Identifier: \"{tenantInfo.Identifier}\"");
            }
            else
            {
                Utilities.TryLogDebug(logger, $"{Store.GetType()}.TryAddAsync: Unable to add Tenant. Id: \"{tenantInfo.Id}\", Identifier: \"{tenantInfo.Identifier}\"");
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
                var errorMessage = $"Exception in {Store.GetType()}.TryRemoveAsync.";
                Utilities.TryLogError(logger, errorMessage, e);
            }

            if (result)
            {
                Utilities.TryLogDebug(logger, $"{Store.GetType()}.TryRemoveAsync: Tenant Id: \"{id}\" removed.");
            }
            else
            {
                Utilities.TryLogDebug(logger, $"{Store.GetType()}.TryRemoveAsync: Unable to remove Tenant Id: \"{id}\".");
            }

            return result;
        }

        public async Task<bool> TryUpdateAsync(TTenantInfo tenantInfo)
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
                var existing = await TryGetAsync(tenantInfo.Id);
                if (existing == null)
                {
                    Utilities.TryLogDebug(logger, $"{Store.GetType()}.TryUpdateAsync: Tenant Id: \"{tenantInfo.Id}\" not found.");
                    goto end;
                }

                result = await Store.TryUpdateAsync(tenantInfo);

            }
            catch (Exception e)
            {
                var errorMessage = $"Exception in {Store.GetType()}.TryUpdateAsync.";
                Utilities.TryLogError(logger, errorMessage, e);
            }

        end:
            if (result)
            {
                Utilities.TryLogDebug(logger, $"{Store.GetType()}.TryUpdateAsync: Tenant Id: \"{tenantInfo.Id}\" updated.");
            }
            else
            {
                Utilities.TryLogDebug(logger, $"{Store.GetType()}.TryUpdateAsync: Unable to update Tenant Id: \"{tenantInfo.Id}\".");
            }

            return result;
        }
    }
}