// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant.Abstractions;

/// <summary>
/// Contains information about the store used for tenant resolution.
/// </summary>
/// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
public class StoreInfo<TTenantInfo> where TTenantInfo : class, ITenantInfo, new()
{
    /// <summary>
    /// Gets or sets the type of the store used.
    /// </summary>
    public Type? StoreType { get; internal set; }
    
    /// <summary>
    /// Gets or sets the store instance used.
    /// </summary>
    public IMultiTenantStore<TTenantInfo>? Store { get; internal set; }
}