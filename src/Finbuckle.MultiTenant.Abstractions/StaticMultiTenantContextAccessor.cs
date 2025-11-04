namespace Finbuckle.MultiTenant.Abstractions;

// TODO rethink internal

internal class StaticMultiTenantContextAccessor<TTenantInfo>(TTenantInfo? tenantInfo)
    : IMultiTenantContextAccessor<TTenantInfo>
    where TTenantInfo : class, ITenantInfo, new()
{
    IMultiTenantContext IMultiTenantContextAccessor.MultiTenantContext => MultiTenantContext;

    public IMultiTenantContext<TTenantInfo> MultiTenantContext { get; } =
        new MultiTenantContext<TTenantInfo>(tenantInfo: tenantInfo);
}