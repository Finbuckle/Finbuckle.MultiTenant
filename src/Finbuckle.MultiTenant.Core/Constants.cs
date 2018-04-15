using System;

namespace Finbuckle.MultiTenant.Core
{
    /// <summary>
    /// Contains constant values for <c>Finbuckle.MultiTenant.Core</c>.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// The maximum character length for <c>Id</c> property on a <c>TenantContet</c>.
        /// The property setter will throw a <c>MultiTenantException</c> if the assigned value exceeds this limit.
        /// </summary>
        public const int TenantIdMaxLength = 64;
    }
}