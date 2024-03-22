namespace Finbuckle.MultiTenant.Abstractions;

internal interface IMultiTenantContextSetter
{
    IMultiTenantContext MultiTenantContext { set; }
}