// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

namespace Finbuckle.MultiTenant
{
    /// <summary>
    /// Contains information for the multitenant tenant, store, and strategy.
    /// </summary>
    public class MultiTenantContext<T> : IMultiTenantContext<T>
        where T : class, ITenantInfo, new()
    {
        public T? TenantInfo { get; set; }
        public StrategyInfo? StrategyInfo { get; set; }
        public StoreInfo<T>? StoreInfo { get; set; }
    }
}