namespace Finbuckle.MultiTenant.Abstractions;

/// <summary>
/// A static multi-tenant context accessor that always returns the same tenant info.
/// </summary>
/// <param name="tenantInfo">The tenant info to return.</param>
/// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
public class StaticMultiTenantContextAccessor<TTenantInfo>(TTenantInfo? tenantInfo)
    : IMultiTenantContextAccessor<TTenantInfo>
    where TTenantInfo : ITenantInfo
{
    IMultiTenantContext IMultiTenantContextAccessor.MultiTenantContext => MultiTenantContext;


    /// <inheritdoc />
    public IMultiTenantContext<TTenantInfo> MultiTenantContext { get; } =
        new MultiTenantContext<TTenantInfo>(tenantInfo: tenantInfo);
}