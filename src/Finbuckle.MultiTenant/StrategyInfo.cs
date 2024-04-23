// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant;

/// <summary>
/// Contextual information about the stategy used to resolve the tenant.
/// </summary>
public class StrategyInfo
{
    /// <summary>
    /// 
    /// </summary>
    public Type? StrategyType { get; internal set; }
    
    /// <summary>
    /// The strategy instance used to resolve the tenant.
    /// </summary>
    public IMultiTenantStrategy? Strategy { get; internal set; }
}