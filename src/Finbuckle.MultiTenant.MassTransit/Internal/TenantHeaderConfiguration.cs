using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finbuckle.MultiTenant.MassTransit.Internal
{
    /// <inheritdoc cref="ITenantHeaderConfiguration"/>
    public class TenantHeaderConfiguration : ITenantHeaderConfiguration
    {
        public string TenantIdentifierHeaderKey { get; private set; }

        public TenantHeaderConfiguration( string tenantIdentifierHeaderKey)
        {
            
            TenantIdentifierHeaderKey = tenantIdentifierHeaderKey;
        }
    }
}
