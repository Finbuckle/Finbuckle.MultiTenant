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
    /// The resolved <see cref="ITenantContext{TTenantInfo}"/>.
    /// </summary>
    public required ITenantContext<TTenantInfo> TenantContext { get; set; }
    
    /// <summary>
    /// The <see cref="IMultiTenantStore{TTenantInfo}"/> instance that was run.
    /// </summary>
    public required IMultiTenantStore<TTenantInfo> Store { get; init; }

    /// <summary>
    /// The <see cref="IMultiTenantStrategy"/> instance that was run.
    /// </summary>
    public required IMultiTenantStrategy Strategy { get; init; }

    /// <summary>
    /// The context used to resolve the tenant.
    /// </summary>
    public required object Context { get; init; }

    /// <summary>
    /// Returns true if a tenant was resolved.
    /// </summary>
    public bool IsResolved => TenantContext.IsResolved;
}