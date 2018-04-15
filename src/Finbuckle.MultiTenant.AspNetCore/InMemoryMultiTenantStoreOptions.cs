using System.Collections;
using System.Collections.Generic;

namespace Finbuckle.MultiTenant.AspNetCore
{
    /// <summary>
    /// Holds data for configuring an <c>InMemoryMultiTenantStore</c>.
    /// </summary>
    public class InMemoryMultiTenantStoreOptions
    {
        public string DefaultConnectionString { get; set; }
        public TenantConfiguration[] TenantConfigurations { get; set; }

        public class TenantConfiguration
        {
            public string Id { get; set; }
            public string Identifier { get; set; }
            public string Name { get; set; }
            public string ConnectionString { get; set; }
            public Dictionary<string, string> Items { get; set; }
        }
    }
}