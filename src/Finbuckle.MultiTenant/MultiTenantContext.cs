// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant;

/// <summary>
/// Contains information for the MultiTenant tenant, store, and strategy.
/// </summary>
public class MultiTenantContext<TTenantInfo> : IMultiTenantContext<TTenantInfo>, IMultiTenantContext
    where TTenantInfo : class, ITenantInfo, new()
{
    public TTenantInfo? TenantInfo { get; set; }

    public StrategyInfo? StrategyInfo { get; set; }
    public StoreInfo<TTenantInfo>? StoreInfo { get; set; }

    ITenantInfo? IMultiTenantContext.TenantInfo => TenantInfo;
}