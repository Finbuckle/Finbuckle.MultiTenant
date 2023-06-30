// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

// ReSharper disable once CheckNamespace
namespace Finbuckle.MultiTenant;

/// <summary>
/// Non-generic interface for the multitenant context.
/// </summary>
public interface IMultiTenantContext
{
    /// <summary>
    /// Information about the tenant for this context.
    /// </summary>
    ITenantInfo? TenantInfo { get; }
        
    /// <summary>
    /// True if a non-null tenant has been resolved.
    /// </summary>
    bool HasResolvedTenant => TenantInfo != null;
        
    /// <summary>
    /// Information about the multitenant strategies for this context.
    /// </summary>
    StrategyInfo? StrategyInfo { get; }
}

/// <summary>
/// Generic interface for the multitenant context.
/// </summary>
/// <typeparam name="T">The ITenantInfo implementation type.</typeparam>
public interface IMultiTenantContext<T>
    where T : class, ITenantInfo, new()
{
    /// <summary>
    /// Information about the tenant for this context.
    /// </summary>
    T? TenantInfo { get; set; }
    
    /// <summary>
    /// Returns true if a non-null tenant has been resolved.
    /// </summary>
    bool HasResolvedTenant => TenantInfo != null;
        
    /// <summary>
    /// Information about the multitenant strategies for this context.
    /// </summary>
    StrategyInfo? StrategyInfo { get; set; }
        
        
    /// <summary>
    /// Information about the multitenant store(s) for this context.
    /// </summary>
    StoreInfo<T>? StoreInfo { get; set; }
}