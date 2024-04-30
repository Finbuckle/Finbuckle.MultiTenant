// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant;

/// <summary>
/// Contains contextual MultiTenant information.
/// </summary>
/// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
public class MultiTenantContext<TTenantInfo> : IMultiTenantContext<TTenantInfo>
    where TTenantInfo : class, ITenantInfo, new()
{
    /// <inheritdoc />
    public TTenantInfo? TenantInfo { get; set; }

    /// <inheritdoc />
    public bool IsResolved => TenantInfo != null;

    /// <inheritdoc />
    public StrategyInfo? StrategyInfo { get; set; }

    /// <inheritdoc />
    public StoreInfo<TTenantInfo>? StoreInfo { get; set; }

    /// <inheritdoc />
    ITenantInfo? IMultiTenantContext.TenantInfo => TenantInfo;
}