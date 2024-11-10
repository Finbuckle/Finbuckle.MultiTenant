// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant.Events;

/// <summary>
/// Context for when a MultiTenantStore has attempted to look up a tenant identifier.
/// </summary>
/// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
public class StoreResolveCompletedContext<TTenantInfo>
    where TTenantInfo : class, ITenantInfo, new()
{
    /// <summary>
    /// The MultiTenantStore instance that was run.
    /// </summary>
    public IMultiTenantStore<TTenantInfo> Store { get; init; }
    
    /// <summary>
    /// The identifier used for tenant resolution by the store.
    /// </summary>
    public required string Identifier { get; init; }
    
    /// <summary>
    /// The resolved TenantInfo. Setting to null will cause the next store to run
    /// </summary>
    public TTenantInfo? TenantInfo { get; set; }
    
    /// <summary>
    /// Returns true if a tenant was found.
    /// </summary>
    public bool TenantFound => TenantInfo != null;
}