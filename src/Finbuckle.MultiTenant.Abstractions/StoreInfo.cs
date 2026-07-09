// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant.Abstractions;

/// <summary>
/// Contains information about the store used for tenant resolution.
/// </summary>
/// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
public class StoreInfo<TTenantInfo> where TTenantInfo : ITenantInfo
{
    /// <summary>
    /// Gets or sets the type of the store used.
    /// </summary>
    public Type? StoreType => Source?.GetType();

    /// <summary>
    /// Gets or sets the source instance used.
    /// </summary>
    public object? Source { get; init; }

    /// <summary>
    /// Gets or sets the primary store instance used.
    /// </summary>
    public IMultiTenantStore<TTenantInfo>? Store => Source as IMultiTenantStore<TTenantInfo>;

    /// <summary>
    /// Gets or sets the store cache instance used.
    /// </summary>
    public IMultiTenantStoreCache<TTenantInfo>? Cache => Source as IMultiTenantStoreCache<TTenantInfo>;
}
