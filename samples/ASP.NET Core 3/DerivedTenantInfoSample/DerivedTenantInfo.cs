using Finbuckle.MultiTenant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DerivedTenantInfoSample
{
    public class DerivedTenantInfo : TenantInfo
    {
        public CustomOptions CustomOptions { get; set; }
    }
}
