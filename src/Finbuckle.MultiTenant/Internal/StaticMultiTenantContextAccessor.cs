using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant.Internal;

internal class StaticMultiTenantContextAccessor<TTenantInfo>(TTenantInfo? tenantInfo)
    : IMultiTenantContextAccessor<TTenantInfo>
    where TTenantInfo : class, ITenantInfo, new()
{
    IMultiTenantContext IMultiTenantContextAccessor.MultiTenantContext => MultiTenantContext;

    public IMultiTenantContext<TTenantInfo> MultiTenantContext { get; } =
        new MultiTenantContext<TTenantInfo> { TenantInfo = tenantInfo };
}