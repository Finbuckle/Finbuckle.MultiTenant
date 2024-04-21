// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant.Events;

// TODO consider making these setters private

/// <summary>
/// Context for when a tenant is successfully resolved.
/// </summary>
public class TenantResolvedContext
{
    /// <summary>
    /// Gets or sets the context used for tenant resolution.
    /// </summary>
    public object? Context { get; set; }
    
    
    /// <summary>
    /// Gets or sets the resolved TenantInfo.
    /// </summary>
    // TODO probably shouldn't be nullable?
    public ITenantInfo? TenantInfo { get; set; }
    
    
    /// <summary>
    /// Gets or sets the type of the multitenant strategy which resolved the tenant.
    /// </summary>
    public Type? StrategyType { get; set; }
    
    
    /// <summary>
    /// Gets or sets the type of the multitenant store which resolved the tenant.
    /// </summary>
    public Type? StoreType { get; set; }
    // TODO consider refactoring to just MultiTenantContext<T>
}