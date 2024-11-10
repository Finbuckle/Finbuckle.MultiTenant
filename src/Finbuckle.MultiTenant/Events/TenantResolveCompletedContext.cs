using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant.Events;

/// <summary>
/// Context for when tenant resolution has completed.
/// </summary>
/// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
public record TenantResolveCompletedContext<TTenantInfo>
    where TTenantInfo : class, ITenantInfo, new()
{
    /// <summary>
    /// The resolved MultiTenantContext.
    /// </summary>
    public required MultiTenantContext<TTenantInfo> MultiTenantContext { get; set; }
    
    /// <summary>
    /// The context used to resolve the tenant.
    /// </summary>
    public required object Context { get; init; }
    
    /// <summary>
    /// 
    /// </summary>
    public bool IsResolved => MultiTenantContext.IsResolved;
}