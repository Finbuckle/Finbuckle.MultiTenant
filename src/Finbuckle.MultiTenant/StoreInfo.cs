// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant;

/// <summary>
/// Represents the store information for a specific tenant.
/// </summary>
/// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
public class StoreInfo<TTenantInfo> where TTenantInfo : class, ITenantInfo, new()
{
    /// <summary>
    /// Gets or sets the type of the store.
    /// </summary>
    public Type? StoreType { get; internal set; }

    /// <summary>
    /// Gets or sets the multi-tenant store.
    /// </summary>
    public IMultiTenantStore<TTenantInfo>? Store { get; internal set; }
}