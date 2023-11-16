// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.



// ReSharper disable once CheckNamespace
namespace Finbuckle.MultiTenant;

/// <summary>
/// Resolves the current tenant.
/// </summary>
public interface ITenantResolver
{
    /// <summary>
    /// Performs tenant resolution within the given context.
    /// </summary>
    /// <param name="context">The context for tenant resolution.</param>
    /// <returns>The MultiTenantContext or null if none resolved.</returns>
    Task<IMultiTenantContext?> ResolveAsync(object context);
}

/// <summary>
/// Resolves the current tenant.
/// </summary>
public interface ITenantResolver<T>
    where T : class, ITenantInfo, new()
{
    /// <summary>
    /// Gets the multitenant strategies used for tenant resolution.
    /// </summary>
    IEnumerable<IMultiTenantStrategy> Strategies { get; }
    
    
    /// <summary>
    /// Get;s the multitenant stores used for tenant resolution.
    /// </summary>
    IEnumerable<IMultiTenantStore<T>> Stores { get; }

    /// <summary>
    /// Performs tenant resolution within the given context.
    /// </summary>
    /// <param name="context">The context for tenant resolution.</param>
    /// <returns>The MultiTenantContext or null if none resolved.</returns>
    Task<IMultiTenantContext<T>?> ResolveAsync(object context);
}