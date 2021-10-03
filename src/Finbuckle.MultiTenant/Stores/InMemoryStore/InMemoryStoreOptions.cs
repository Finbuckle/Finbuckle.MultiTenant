// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System.Collections.Generic;

namespace Finbuckle.MultiTenant.Stores
{
    public class InMemoryStoreOptions<TTenantInfo>
        where TTenantInfo : class, ITenantInfo, new()
    {
        public bool IsCaseSensitive { get; set; } = false;
        public IList<TTenantInfo> Tenants { get; set; } = new List<TTenantInfo>();
    }
}