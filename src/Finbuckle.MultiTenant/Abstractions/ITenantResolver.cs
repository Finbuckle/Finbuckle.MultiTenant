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
    /// <returns>The MultiTenantContext.</returns>
    Task<IMultiTenantContext> ResolveAsync(object context);
    
    public IEnumerable<IMultiTenantStrategy> Strategies { get; set; }
}

/// <summary>
/// Resolves the current tenant.
/// </summary>
/// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
public interface ITenantResolver<TTenantInfo>
    where TTenantInfo : class, ITenantInfo, new()
{
    /// <summary>
    /// Performs tenant resolution within the given context.
    /// </summary>
    /// <param name="context">The context for tenant resolution.</param>
    /// <returns>The MultiTenantContext.</returns>
    Task<IMultiTenantContext<TTenantInfo>> ResolveAsync(object context);
    
    public IEnumerable<IMultiTenantStrategy> Strategies { get; set; }
    
    public IEnumerable<IMultiTenantStore<TTenantInfo>> Stores { get; set; }
}