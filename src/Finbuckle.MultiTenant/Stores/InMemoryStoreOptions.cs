// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant.Stores;

/// <summary>
/// Options for configuring the <see cref="InMemoryStore{TTenantInfo}"/>.
/// </summary>
/// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
public class InMemoryStoreOptions<TTenantInfo>
    where TTenantInfo : ITenantInfo
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