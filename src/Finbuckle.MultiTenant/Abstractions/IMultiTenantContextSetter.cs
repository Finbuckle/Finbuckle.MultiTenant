namespace Finbuckle.MultiTenant.Internal;

internal interface IMultiTenantContextSetter
{
    void SetMultiTenantContext(IMultiTenantContext multiTenantContext);
}