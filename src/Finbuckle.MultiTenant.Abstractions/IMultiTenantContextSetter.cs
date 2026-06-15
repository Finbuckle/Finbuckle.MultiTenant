namespace Finbuckle.MultiTenant.Abstractions;

/// <summary>
/// Interface used to set the <see cref="ITenantContext"/>. This is an implementation detail and not intended for general use.
/// </summary>
public interface IMultiTenantContextSetter
{
    /// <summary>
    /// Sets the current <see cref="ITenantContext"/>.
    /// </summary>
    ITenantContext MultiTenantContext { set; }
}