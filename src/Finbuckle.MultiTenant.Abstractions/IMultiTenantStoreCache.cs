// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant.Abstractions;

/// <summary>
/// Interface definition for tenant store caches.
/// </summary>
/// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
public interface IMultiTenantStoreCache<TTenantInfo> where TTenantInfo : ITenantInfo
{
    /// <summary>
    /// Retrieve the TTenantInfo for a given tenant Id.
    /// </summary>
    /// <param name="id">TenantId for the tenant to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The found TTenantInfo instance or null if none found.</returns>
    Task<TTenantInfo?> GetAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieve the TTenantInfo for a given identifier.
    /// </summary>
    /// <param name="identifier">Identifier for the tenant to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The found TTenantInfo instance or null if none found.</returns>
    Task<TTenantInfo?> GetByIdentifierAsync(string identifier, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add or replace a tenant in the cache.
    /// </summary>
    /// <param name="tenantInfo">The tenant to cache.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SetAsync(TTenantInfo tenantInfo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove a tenant from the cache by tenant Id.
    /// </summary>
    /// <param name="id">TenantId for the tenant to remove.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove a tenant from the cache by identifier.
    /// </summary>
    /// <param name="identifier">Identifier for the tenant to remove.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveByIdentifierAsync(string identifier, CancellationToken cancellationToken = default);
}
