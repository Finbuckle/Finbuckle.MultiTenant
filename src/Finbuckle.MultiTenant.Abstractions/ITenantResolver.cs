// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant.Abstractions;

/// <summary>
/// Resolves the current tenant.
/// </summary>
public interface ITenantResolver
{
    /// <summary>
    /// Performs tenant resolution within the given context.
    /// </summary>
    /// <param name="context">The context for tenant resolution.</param>
    /// <returns>The MultiTenantContext.</returns>
    Task<IMultiTenantContext> ResolveAsync(object context);
    
    /// <summary>
    /// Contains a list of MultiTenant strategies used for tenant resolution.
    /// </summary>
    public IEnumerable<IMultiTenantStrategy> Strategies { get; set; }
}

/// <summary>
/// Resolves the current tenant.
/// </summary>
/// <typeparam name="TTenantInfo">The TenantInfo derived type.</typeparam>
public interface ITenantResolver<TTenantInfo> : ITenantResolver
    where TTenantInfo : TenantInfo
{
    /// <summary>
    /// Performs tenant resolution within the given context.
    /// </summary>
    /// <param name="context">The context for tenant resolution.</param>
    /// <returns>The MultiTenantContext.</returns>
    new Task<IMultiTenantContext<TTenantInfo>> ResolveAsync(object context);
    
    /// <summary>
    /// Contains a list of MultiTenant stores used for tenant resolution.
    /// </summary>
    public IEnumerable<IMultiTenantStore<TTenantInfo>> Stores { get; set; }
}