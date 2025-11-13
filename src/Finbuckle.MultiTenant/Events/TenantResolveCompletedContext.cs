using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant.Events;

/// <summary>
/// Context for when tenant resolution has completed.
/// </summary>
/// <typeparam name="TTenantInfo">The <see cref="TenantInfo"/> derived type.</typeparam>
public record TenantResolveCompletedContext<TTenantInfo>
    where TTenantInfo : TenantInfo
{
    /// <summary>
    /// The resolved <see cref="MultiTenantContext{TTenantInfo}"/>.
    /// </summary>
    public required MultiTenantContext<TTenantInfo> MultiTenantContext { get; set; }

    /// <summary>
    /// The context used to resolve the tenant.
    /// </summary>
    public required object Context { get; init; }

    /// <summary>
    /// Returns true if a tenant was resolved.
    /// </summary>
    public bool IsResolved => MultiTenantContext.IsResolved;
}