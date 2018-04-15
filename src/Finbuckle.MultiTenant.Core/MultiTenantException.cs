using System;

namespace Finbuckle.MultiTenant.Core
{
    /// <summary>
    /// A derived <c>Exception</c> class for any exception generated by <c>Finbuckle.MultiTenant</c>.
    /// </summary>
    public class MultiTenantException : Exception
    {
        public MultiTenantException(string message, Exception innerException = null)
            : base(message, innerException) { }
    }
}