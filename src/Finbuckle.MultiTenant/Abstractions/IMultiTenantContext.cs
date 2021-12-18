// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

namespace Finbuckle.MultiTenant
{
    public interface IMultiTenantContext<T>
        where T : class, ITenantInfo, new()
    {
        T? TenantInfo { get; set; }
        StrategyInfo? StrategyInfo { get; set; }
        StoreInfo<T>? StoreInfo { get; set; }
    }
}