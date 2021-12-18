// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System.Threading.Tasks;

namespace Finbuckle.MultiTenant
{
    /// <summary>
    /// The interface for determining the tenant idenfitider.
    /// </summary>
    public interface IMultiTenantStrategy
    {
        /// <summary>
        ///  Method for implemenations to control how the identifier is determined.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task<string?> GetIdentifierAsync(object context);
        
        /// <summary>
        ///  Determines strategy execution order. Normally handled in the order registered.
        /// </summary>
        int Priority => 0;
    }
}