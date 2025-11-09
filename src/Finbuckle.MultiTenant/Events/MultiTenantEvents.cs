// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant.Events;

/// <summary>
/// Events for successful and failed tenant resolution.
/// </summary>
/// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
public class MultiTenantEvents<TTenantInfo>
    where TTenantInfo : TenantInfo
{
    /// <summary>
    /// Called after each MultiTenantStrategy has run. The resulting identifier can be modified if desired or set to null to advance to the next strategy.
    /// </summary>
    public Func<StrategyResolveCompletedContext, Task> OnStrategyResolveCompleted { get; set; } = context => Task.CompletedTask;

    /// <summary>
    /// Called after each MultiTenantStore has attempted to find the tenant identifier. The resulting TenantInfo can be modified if desired or set to null to advance to the next store.
    /// </summary>
    public Func<StoreResolveCompletedContext<TTenantInfo>, Task> OnStoreResolveCompleted { get; set; } = context => Task.CompletedTask;
    
    /// <summary>
    /// Called after tenant resolution has completed for all strategies and stores. The resulting MultiTenantContext can be modified if desired.
    /// </summary>
    public Func<TenantResolveCompletedContext<TTenantInfo>, Task> OnTenantResolveCompleted { get; set; } = context => Task.CompletedTask;
}