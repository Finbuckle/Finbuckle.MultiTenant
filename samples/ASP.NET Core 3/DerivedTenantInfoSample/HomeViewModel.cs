using Finbuckle.MultiTenant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DerivedTenantInfoSample
{
    public class HomeViewModel
    {
        public TenantInfo TenantInfo { get; set; }

        public CustomOptions CustomOptions { get; set; }
    }
}
