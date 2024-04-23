namespace Finbuckle.MultiTenant.Abstractions;

/// <summary>
/// Interface used to set the MultiTenantContext. This is an implementation detail and not intended for general use.
/// </summary>
public interface IMultiTenantContextSetter
{
    IMultiTenantContext MultiTenantContext { set; }
}