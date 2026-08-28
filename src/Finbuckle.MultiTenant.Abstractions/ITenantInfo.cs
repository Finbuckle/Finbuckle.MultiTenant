namespace Finbuckle.MultiTenant.Abstractions;

/// <summary>
/// Represents the basic information for a tenant in a multi-tenant application.
/// </summary>
/// <remarks>
/// <para>
/// This interface is the only contract the library depends on. The library never writes to a tenant info instance
/// after it has been constructed and never forces a particular shape onto implementations: properties may use
/// <c>set</c>, <c>init</c>, or be constructor-only with no setter at all. Implementations do not need to derive from
/// <see cref="TenantInfo"/> or be a <c>record</c>.
/// </para>
/// <para>
/// In return, implementations must honor one rule: <see cref="Id"/> and <see cref="Identifier"/> must not change
/// for the lifetime of an instance once it has been handed to the library (stored, cached, resolved, or assigned to a
/// multi-tenant context). Tenant identity is <see cref="Id"/> equality; built-in stores look up
/// <see cref="Identifier"/> case-insensitively by default. To change a tenant, create a new instance and pass it to
/// the store's update method. The current tenant of a request is changed only by assigning a new
/// <see cref="IMultiTenantContext"/>, never by mutating the current instance.
/// </para>
/// </remarks>
public interface ITenantInfo
{
    /// <summary>
    /// Gets a unique identifier for the tenant. Typically used as the primary key.
    /// </summary>
    /// <remarks>Must be non-empty and must never change for the lifetime of an instance.</remarks>
    public string Id { get; }

    /// <summary>
    /// Gets an externally facing identifier used for tenant resolution.
    /// </summary>
    /// <remarks>
    /// Must be non-empty. Must not change for the lifetime of an instance; use the store's update method with a new
    /// instance to change a tenant's identifier.
    /// </remarks>
    public string Identifier { get; }
}
