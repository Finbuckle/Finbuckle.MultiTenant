// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant.Abstractions;

/// <summary>
/// Contains contextual multi-tenant information.
/// </summary>
/// <typeparam name="TTenantInfo">The <see cref="TenantInfo"/> derived type.</typeparam>
public class MultiTenantContext<TTenantInfo> : IMultiTenantContext<TTenantInfo>
    where TTenantInfo : ITenantInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MultiTenantContext{TTenantInfo}"/> class
    /// with the specified tenant information.
    /// </summary>
    /// <param name="tenantInfo">The tenant information (may be null if not resolved).</param>
    public MultiTenantContext(TTenantInfo? tenantInfo)
    {
        TenantInfo = tenantInfo;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiTenantContext{TTenantInfo}"/> class
    /// with the specified tenant information, strategy information, and store information.
    /// </summary>
    /// <param name="tenantInfo">The tenant information (may be null if not resolved).</param>
    /// <param name="strategyInfo">The strategy that resolved the tenant (may be null).</param>
    /// <param name="storeInfo">The store that provided the tenant information (may be null).</param>
    public MultiTenantContext(TTenantInfo? tenantInfo, StrategyInfo? strategyInfo, StoreInfo<TTenantInfo>? storeInfo)
    {
        TenantInfo = tenantInfo;
        StrategyInfo = strategyInfo;
        StoreInfo = storeInfo;
    }

    /// <inheritdoc />
    public TTenantInfo? TenantInfo { get; init; }

    /// <inheritdoc />
    public bool IsResolved => TenantInfo != null;

    /// <inheritdoc />
    public StrategyInfo? StrategyInfo { get; init; }

    /// <inheritdoc />
    public StoreInfo<TTenantInfo>? StoreInfo { get; init; }

    /// <inheritdoc />
    ITenantInfo? IMultiTenantContext.TenantInfo
    {
        get => TenantInfo;
        init => TenantInfo = (TTenantInfo?)value;
    }
}