// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Finbuckle.MultiTenant
{
    /// <summary>
    /// Interface definition for tenant stores.
    /// </summary>
    public interface IMultiTenantStore<TTenantInfo> where TTenantInfo : class, ITenantInfo, new()
    {
        /// <summary>
        /// Try to add the TTenantInfo to the store.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task<bool> TryAddAsync(TTenantInfo tenantInfo);

        /// <summary>
        /// Try to update the TTenantInfo in the store.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task<bool> TryUpdateAsync(TTenantInfo tenantInfo);

        /// <summary>
        /// Try to remove the TTenantInfo from the store.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> TryRemoveAsync(string identifier);

        /// <summary>
        /// Retrieve the TTenantInfo for a given identifier.
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        Task<TTenantInfo?> TryGetByIdentifierAsync(string identifier);

        /// <summary>
        /// Retrieve the TTenantInfo for a given tenant Id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<TTenantInfo?> TryGetAsync(string id);


        /// <summary>
        /// Retrieve all the TTenantInfo's from the store.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<TTenantInfo>> GetAllAsync();
    }
}