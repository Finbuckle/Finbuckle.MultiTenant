using System;
using System.Threading.Tasks;

namespace Finbuckle.MultiTenant.Core.Abstractions
{
    /// <summary>
    /// Interface definition for tenant stores.
    /// </summary>
    public interface IMultiTenantStore
    {
        /// <summary>
        /// Try to add the <c>TenantContext</c> to the store.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task<bool> TryAdd(TenantContext context);

        /// <summary>
        /// Try to remove the <c>TenantContext</c> from the store.
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        Task<bool> TryRemove(string identifier);

        /// <summary>
        /// Retrieve the <c>TenantContext<c> for a given identifier.
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        Task<TenantContext> GetByIdentifierAsync(string identifier);
    }
}