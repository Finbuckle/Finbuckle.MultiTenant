// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant.Abstractions;

/// <summary>
/// Contains information about the store used for tenant resolution.
/// </summary>
/// <typeparam name="TTenantInfo">The <see cref="ITenantInfo{TId}"/> implementation type.</typeparam>
/// <typeparam name="TId">The ID implementation type.</typeparam>
public class StoreInfo<TTenantInfo, TId> where TTenantInfo : ITenantInfo<TId> where TId : IEquatable<TId>
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
    public IMultiTenantStore<TTenantInfo, TId>? Store => Source as IMultiTenantStore<TTenantInfo, TId>;

    /// <summary>
    /// Gets or sets the store cache instance used.
    /// </summary>
    public IMultiTenantStoreCache<TTenantInfo, TId>? Cache => Source as IMultiTenantStoreCache<TTenantInfo, TId>;
}
