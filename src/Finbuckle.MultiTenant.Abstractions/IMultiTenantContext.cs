// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant.Abstractions;

/// <summary>
/// Non-generic interface for the MultiTenantContext.
/// </summary>
public interface IMultiTenantContext
{
    /// <summary>
    /// Information about the tenant for this context.
    /// </summary>
    ITenantInfo? TenantInfo { get; init; }

    /// <summary>
    /// True if a tenant has been resolved and TenantInfo is not null.
    /// </summary>
    bool IsResolved { get; }

    /// <summary>
    /// Information about the MultiTenant strategies for this context.
    /// </summary>
    StrategyInfo? StrategyInfo { get; init; }
}



/// <summary>
/// Generic interface for the multi-tenant context.
/// </summary>
/// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
public interface IMultiTenantContext<TTenantInfo> : IMultiTenantContext
    where TTenantInfo : class, ITenantInfo, new()
{
    /// <summary>
    /// Information about the tenant for this context.
    /// </summary>
    new TTenantInfo? TenantInfo { get; init; }
    
    /// <summary>
    /// Information about the MultiTenant stores for this context.
    /// </summary>
    StoreInfo<TTenantInfo>? StoreInfo { get; init; }
}