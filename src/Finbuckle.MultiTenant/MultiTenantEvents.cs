// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant;

/// <summary>
/// Events for successful and failed tenant resolution.
/// </summary>
/// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
public class MultiTenantEvents<TTenantInfo>
    where TTenantInfo : ITenantInfo
{
    /// <summary>
    /// Called after each <see cref="IMultiTenantStrategy"/> has run. The resulting identifier can be modified if desired or set to null to advance to the next strategy.
    /// </summary>
    public Func<StrategyResolveCompletedContext, Task> OnStrategyResolveCompleted { get; set; } =
        context => Task.CompletedTask;

    /// <summary>
    /// Called after each <see cref="IMultiTenantStore{TTenantInfo}"/> has attempted to find the tenant identifier. The resulting <see cref="TenantInfo"/> can be modified if desired or set to null to advance to the next store.
    /// </summary>
    public Func<StoreResolveCompletedContext<TTenantInfo>, Task> OnStoreResolveCompleted { get; set; } =
        context => Task.CompletedTask;

    /// <summary>
    /// Called after tenant resolution has completed for all strategies and stores. The resulting <see cref="MultiTenantContext{TTenantInfo}"/> can be modified if desired.
    /// </summary>
    public Func<TenantResolveCompletedContext<TTenantInfo>, Task> OnTenantResolveCompleted { get; set; } =
        context => Task.CompletedTask;
}