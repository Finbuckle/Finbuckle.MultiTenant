// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant.Events;

/// <summary>
/// Events for successful and failed tenant resolution.
/// </summary>
public class MultiTenantEvents
{
    /// <summary>
    /// Called when a tenant is successfully resolved.
    /// </summary>
    public Func<TenantResolvedContext, Task> OnTenantResolved { get; set; } = context => Task.CompletedTask;
    
    
    /// <summary>
    /// Called when no tenant fails is successfully resolved.
    /// </summary>
    public Func<TenantNotResolvedContext, Task> OnTenantNotResolved { get; set; } = context => Task.CompletedTask;
}