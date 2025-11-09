// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant.Stores.InMemoryStore;

/// <summary>
/// Options for configuring the InMemoryStore.
/// </summary>
/// <typeparam name="TTenantInfo">The TenantInfo derived type.</typeparam>
public class InMemoryStoreOptions<TTenantInfo>
    where TTenantInfo : TenantInfo
{
    /// <summary>
    /// Gets or sets whether tenant identifier lookups are case-sensitive.
    /// </summary>
    public bool IsCaseSensitive { get; set; }
    
    /// <summary>
    /// Gets or sets the list of tenants to store in memory.
    /// </summary>
    public IList<TTenantInfo> Tenants { get; set; } = new List<TTenantInfo>();
}