using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant;

/// <summary>
/// Context for when tenant resolution has completed.
/// </summary>
/// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
public record TenantResolveCompletedContext<TTenantInfo>
    where TTenantInfo : ITenantInfo
{
    /// <summary>
    /// The resolved tenant information, or <see langword="null"/> if no tenant was resolved.
    /// </summary>
    public TTenantInfo? TenantInfo { get; set; }

    /// <summary>
    /// The <see cref="IMultiTenantStore{TTenantInfo}"/> instance that resolved the tenant, if resolved by the primary store.
    /// </summary>
    public IMultiTenantStore<TTenantInfo>? Store { get; init; }

    /// <summary>
    /// The <see cref="IMultiTenantStoreCache{TTenantInfo}"/> instance that resolved the tenant, if resolved by a cache.
    /// </summary>
    public IMultiTenantStoreCache<TTenantInfo>? Cache { get; init; }

    /// <summary>
    /// The <see cref="IMultiTenantStrategy"/> instance that resolved the tenant, if a tenant was resolved.
    /// </summary>
    public IMultiTenantStrategy? Strategy { get; init; }

    /// <summary>
    /// The context used to resolve the tenant.
    /// </summary>
    public required object Context { get; init; }

    /// <summary>
    /// Returns true if a tenant was resolved.
    /// </summary>
    public bool IsResolved => TenantInfo is not null;
}
