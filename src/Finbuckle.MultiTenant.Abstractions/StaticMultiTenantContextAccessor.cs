namespace Finbuckle.MultiTenant.Abstractions;

/// <summary>
/// A static multi-tenant context accessor that always returns the same tenant info.
/// </summary>
/// <param name="tenantInfo">The tenant info to return.</param>
/// <typeparam name="TTenantInfo">The type of tenant info.</typeparam>
public class StaticMultiTenantContextAccessor<TTenantInfo>(TTenantInfo? tenantInfo)
    : IMultiTenantContextAccessor<TTenantInfo>
    where TTenantInfo : TenantInfo
{
    IMultiTenantContext IMultiTenantContextAccessor.MultiTenantContext => MultiTenantContext;


    /// <inheritdoc />
    public IMultiTenantContext<TTenantInfo> MultiTenantContext { get; } =
        new MultiTenantContext<TTenantInfo>(tenantInfo: tenantInfo);
}