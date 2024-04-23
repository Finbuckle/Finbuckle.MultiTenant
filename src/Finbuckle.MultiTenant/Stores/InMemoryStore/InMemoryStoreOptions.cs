// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant.Stores.InMemoryStore;

/// <summary>
/// Options for the InMemoryStore.
/// </summary>
/// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
public class InMemoryStoreOptions<TTenantInfo>
    where TTenantInfo : class, ITenantInfo, new()
{
    /// <summary>
    /// Gets or sets a value indicating whether the InMemoryStore should be case sensitive.
    /// </summary>
    public bool IsCaseSensitive { get; set; }
    
    /// <summary>
    /// Gets or sets the list of tenants to be stored in the InMemoryStore.
    /// </summary>
    public IList<TTenantInfo> Tenants { get; set; } = new List<TTenantInfo>();
}