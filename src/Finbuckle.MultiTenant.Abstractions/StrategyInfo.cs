// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant.Abstractions;

/// <summary>
/// Contains information about the strategy used for tenant resolution.
/// </summary>
public class StrategyInfo
{
    /// <summary>
    /// Gets or sets the type of the strategy used.
    /// </summary>
    public Type? StrategyType { get; internal set; }
    
    /// <summary>
    /// Gets or sets the strategy instance used.
    /// </summary>
    public IMultiTenantStrategy? Strategy { get; internal set; }
}