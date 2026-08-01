// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant.Abstractions;

/// <summary>
/// Resolves the current tenant.
/// </summary>
public interface ITenantResolver<TId> where TId : IEquatable<TId>
{
    /// <summary>
    /// Performs tenant resolution within the given context.
    /// </summary>
    /// <param name="context">The context for tenant resolution.</param>
    /// <returns>The resolved <see cref="ITenantInfo{TId}"/>, or <see langword="null"/> if no tenant was resolved.</returns>
    Task<ITenantInfo<TId>?> ResolveAsync(object context);

    /// <summary>
    /// Contains a list of <see cref="IMultiTenantStrategy"/> instances used for tenant resolution.
    /// </summary>
    public IEnumerable<IMultiTenantStrategy> Strategies { get; set; }
}

/// <summary>
/// Resolves the current tenant.
/// </summary>
/// <typeparam name="TTenantInfo">The <see cref="ITenantInfo{TId}"/> implementation type.</typeparam>
/// <typeparam name="TId">The ID implementation type.</typeparam>
public interface ITenantResolver<TTenantInfo, TId> : ITenantResolver<TId>
    where TTenantInfo : ITenantInfo<TId> where TId : IEquatable<TId>
{
    /// <summary>
    /// Performs tenant resolution within the given context.
    /// </summary>
    /// <param name="context">The context for tenant resolution.</param>
    /// <returns>The resolved tenant information, or <see langword="null"/> if no tenant was resolved.</returns>
    new Task<TTenantInfo?> ResolveAsync(object context);

    /// <summary>
    /// The primary <see cref="IMultiTenantStore{TTenantInfo, TId}"/> instance used for tenant resolution.
    /// </summary>
    public IMultiTenantStore<TTenantInfo, TId> Store { get; }

    /// <summary>
    /// Contains a list of <see cref="IMultiTenantStoreCache{TTenantInfo, TId}"/> instances used for tenant resolution.
    /// </summary>
    public IEnumerable<IMultiTenantStoreCache<TTenantInfo, TId>> StoreCaches { get; }
}
