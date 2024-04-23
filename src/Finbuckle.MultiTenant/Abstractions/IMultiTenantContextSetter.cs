namespace Finbuckle.MultiTenant.Abstractions;

/// <summary>
/// Interface used to set the MultiTenantContext. This is an implementation detail and not intended for general use.
/// </summary>
public interface IMultiTenantContextSetter
{
    /// <summary>
    /// Sets the MultiTenantContext.
    /// </summary>
    /// <value>
    /// The MultiTenantContext to be set.
    /// </value>
    IMultiTenantContext MultiTenantContext { set; }
}