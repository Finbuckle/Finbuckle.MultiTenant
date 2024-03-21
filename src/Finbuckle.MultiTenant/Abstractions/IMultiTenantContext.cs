// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

// ReSharper disable once CheckNamespace

namespace Finbuckle.MultiTenant;

/// <summary>
/// Non-generic interface for the MultiTenantContext.
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
    bool HasResolvedTenant { get; }

    /// <summary>
    /// Information about the MultiTenant strategies for this context.
    /// </summary>
    StrategyInfo? StrategyInfo { get; }
}

/// <summary>
/// Generic interface for the multi-tenant context.
/// </summary>
/// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
public interface IMultiTenantContext<TTenantInfo>
    where TTenantInfo : class, ITenantInfo, new()
{
    /// <summary>
    /// Information about the tenant for this context.
    /// </summary>
    new TTenantInfo? TenantInfo { get; }

    /// <summary>
    /// True if a non-null tenant has been resolved.
    /// </summary>
    bool HasResolvedTenant { get; }
    
    /// <summary>
    /// Information about the MultiTenant stores for this context.
    /// </summary>
    StoreInfo<TTenantInfo>? StoreInfo { get; set; }

    /// <summary>
    /// Information about the MultiTenant strategies for this context.
    /// </summary>
    StrategyInfo? StrategyInfo { get; }
}