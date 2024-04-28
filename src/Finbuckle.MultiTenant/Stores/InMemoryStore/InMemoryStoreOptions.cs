// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant.Stores.InMemoryStore;

public class InMemoryStoreOptions<TTenantInfo>
    where TTenantInfo : class, ITenantInfo, new()
{
    public bool IsCaseSensitive { get; set; }
    public IList<TTenantInfo> Tenants { get; set; } = new List<TTenantInfo>();
}