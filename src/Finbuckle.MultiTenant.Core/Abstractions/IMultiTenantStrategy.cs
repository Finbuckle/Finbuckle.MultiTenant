using System;
using System.Threading.Tasks;

namespace Finbuckle.MultiTenant.Core.Abstractions
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
        string GetIdentifier(object context);
    }
}