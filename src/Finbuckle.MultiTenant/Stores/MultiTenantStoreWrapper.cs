// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
            this.Store = store ?? throw new ArgumentNullException(nameof(store));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TTenantInfo?> TryGetAsync(string id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            TTenantInfo? result = null;

            try
            {
                result = await Store.TryGetAsync(id);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Exception in TryGetAsync");
            }

            if (result != null)
            {
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug("TryGetAsync: Tenant Id \"{TenantId}\" found.", id);
                }
            }
            else
            {
                logger.LogDebug("TryGetAsync: Unable to find Tenant Id \"{TenantId}\".", id);
            }

            return result;
        }

        public async Task<IEnumerable<TTenantInfo>> GetAllAsync()
        {
            IEnumerable<TTenantInfo> result = new List<TTenantInfo>();

            try
            {
                result = await Store.GetAllAsync();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Exception in GetAllAsync");
            }

            return result;
        }

        public async Task<TTenantInfo?> TryGetByIdentifierAsync(string identifier)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException(nameof(identifier));
            }

            TTenantInfo? result = null;

            try
            {
                result = await Store.TryGetByIdentifierAsync(identifier);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Exception in TryGetByIdentifierAsync");
            }

            if (result != null)
            {
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug("TryGetByIdentifierAsync: Tenant found with identifier \"{TenantIdentifier}\"", identifier);
                }
            }
            else
            {
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug("TryGetByIdentifierAsync: Unable to find Tenant with identifier \"{TenantIdentifier}\"", identifier);
                }
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
                    logger.LogDebug("TryAddAsync: Tenant already exists. Id: \"{TenantId}\", Identifier: \"{TenantIdentifier}\"", tenantInfo.Id, tenantInfo.Identifier);
                    goto end;
                }

                existing = await TryGetByIdentifierAsync(tenantInfo.Identifier);
                if (existing != null)
                {
                    logger.LogDebug("TryAddAsync: Tenant already exists. Id: \"{TenantId}\", Identifier: \"{TenantIdentifier}\"", tenantInfo.Id, tenantInfo.Identifier);
                    goto end;
                }

                result = await Store.TryAddAsync(tenantInfo);

            }
            catch (Exception e)
            {
                logger.LogError(e, "Exception in TryAddAsync");
            }

        end:
            if (result)
            {
                logger.LogDebug("TryAddAsync: Tenant added. Id: \"{TenantId}\", Identifier: \"{TenantIdentifier}\"", tenantInfo.Id, tenantInfo.Identifier);
            }
            else
            {
                logger.LogDebug("TryAddAsync: Unable to add Tenant. Id: \"{TenantId}\", Identifier: \"{TenantIdentifier}\"", tenantInfo.Id, tenantInfo.Identifier);
            }

            return result;
        }

        public async Task<bool> TryRemoveAsync(string identifier)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException(nameof(identifier));
            }

            var result = false;

            try
            {
                result = await Store.TryRemoveAsync(identifier);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Exception in TryRemoveAsync");
            }

            if (result)
            {
                logger.LogDebug("TryRemoveAsync: Tenant Identifier: \"{TenantIdentifier}\" removed", identifier);
            }
            else
            {
                logger.LogDebug("TryRemoveAsync: Unable to remove Tenant Identifier: \"{TenantIdentifier}\"", identifier);
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
                    logger.LogDebug("TryUpdateAsync: Tenant Id: \"{TenantId}\" not found", tenantInfo.Id);
                    goto end;
                }

                result = await Store.TryUpdateAsync(tenantInfo);

            }
            catch (Exception e)
            {
                logger.LogError(e, "Exception in TryUpdateAsync");
            }

        end:
            if (result)
            {
                logger.LogDebug("TryUpdateAsync: Tenant Id: \"{TenantId}\" updated", tenantInfo.Id);
            }
            else
            {
                logger.LogDebug("TryUpdateAsync: Unable to update Tenant Id: \"{TenantId}\"", tenantInfo.Id);
            }

            return result;
        }
    }
}