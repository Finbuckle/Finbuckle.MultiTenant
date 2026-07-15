// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant.Abstractions;

/// <summary>
/// Provides operations for managing the ambient tenant scope for the current execution context.
/// </summary>
public interface ITenantScopeProvider
{
    /// <summary>
    /// Begins a new ambient tenant scope for the current execution context.
    /// </summary>
    public void BeginScope();
}
