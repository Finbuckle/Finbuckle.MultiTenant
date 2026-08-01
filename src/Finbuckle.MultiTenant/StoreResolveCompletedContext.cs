// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant;

/// <summary>
/// Context for when the primary tenant store has attempted to look up a tenant identifier.
/// </summary>
/// <typeparam name="TTenantInfo">The <see cref="ITenantInfo{TId}"/> implementation type.</typeparam>
/// <typeparam name="TId">The ID implementation type.</typeparam>
public class StoreResolveCompletedContext<TTenantInfo, TId>
    where TTenantInfo : ITenantInfo<TId> where TId : IEquatable<TId>
{
    /// <summary>
    /// The context used for attempted tenant resolution.
    /// </summary>
    public required object Context { get; init; }

    /// <summary>
    /// The <see cref="IMultiTenantStore{TTenantInfo, TId}"/> instance that was run.
    /// </summary>
    public required IMultiTenantStore<TTenantInfo, TId> Store { get; init; }

    /// <summary>
    /// The <see cref="IMultiTenantStrategy"/> instance that was run.
    /// </summary>
    public required IMultiTenantStrategy Strategy { get; init; }

    /// <summary>
    /// The identifier used for tenant resolution by the store.
    /// </summary>
    public required string Identifier { get; init; }

    /// <summary>
    /// The resolved <see cref="TenantInfo"/>. Setting to null will cause the next store to run.
    /// </summary>
    public TTenantInfo? TenantInfo { get; set; }

    /// <summary>
    /// Returns true if a tenant was found.
    /// </summary>
    public bool TenantFound => TenantInfo != null;
}
