// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant.Abstractions;

/// <summary>
/// Contains information about the store used for tenant resolution.
/// </summary>
/// <typeparam name="TTenantInfo">The <see cref="TenantInfo"/> derived type.</typeparam>
public class StoreInfo<TTenantInfo> where TTenantInfo : TenantInfo
{
    /// <summary>
    /// Gets or sets the type of the store used.
    /// </summary>
    public Type? StoreType => Store?.GetType();

    /// <summary>
    /// Gets or sets the store instance used.
    /// </summary>
    public IMultiTenantStore<TTenantInfo>? Store { get; init; }
}