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
    ITenantContext IMultiTenantContextAccessor.MultiTenantContext => MultiTenantContext;


    /// <inheritdoc />
    public ITenantContext<TTenantInfo> MultiTenantContext { get; } =
        new TenantContext<TTenantInfo>(tenantInfo: tenantInfo);
}