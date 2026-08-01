// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant;

/// <summary>
/// Events for successful and failed tenant resolution.
/// </summary>
/// <typeparam name="TTenantInfo">The <see cref="ITenantInfo{TId}"/> implementation type.</typeparam>
/// <typeparam name="TId">The ID implementation type.</typeparam>
public class MultiTenantEvents<TTenantInfo, TId>
    where TTenantInfo : ITenantInfo<TId> where TId : IEquatable<TId>
{
    /// <summary>
    /// Called after each <see cref="IMultiTenantStrategy"/> has run. The resulting identifier can be modified if desired or set to null to advance to the next strategy.
    /// </summary>
    public Func<StrategyResolveCompletedContext, Task> OnStrategyResolveCompleted { get; set; } =
        context => Task.CompletedTask;

    /// <summary>
    /// Called after each <see cref="IMultiTenantStoreCache{TTenantInfo, TId}"/> has attempted to find the tenant identifier. The resulting <see cref="TenantInfo{Tid}"/> can be modified if desired or set to null to advance to the next cache or store.
    /// </summary>
    public Func<StoreCacheResolveCompletedContext<TTenantInfo, TId>, Task> OnStoreCacheResolveCompleted { get; set; } =
        context => Task.CompletedTask;

    /// <summary>
    /// Called after the primary <see cref="IMultiTenantStore{TTenantInfo, TId}"/> has attempted to find the tenant identifier. The resulting <see cref="TenantInfo{Tid}"/> can be modified if desired or set to null to continue tenant resolution.
    /// </summary>
    public Func<StoreResolveCompletedContext<TTenantInfo, TId>, Task> OnStoreResolveCompleted { get; set; } =
        context => Task.CompletedTask;

    /// <summary>
    /// Called after tenant resolution has completed for all strategies and stores. The resulting tenant information can be modified if desired.
    /// </summary>
    public Func<TenantResolveCompletedContext<TTenantInfo, TId>, Task> OnTenantResolveCompleted { get; set; } =
        context => Task.CompletedTask;
}
