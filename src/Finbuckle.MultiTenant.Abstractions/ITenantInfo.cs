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
/// In return, implementations must honor two rules. <see cref="Id"/> never changes: it is the tenant's identity, and
/// tenant equality is <see cref="Id"/> equality. <see cref="Identifier"/> may change, but only through the store:
/// create a new instance carrying the new identifier and pass it to the store's update method, which re-keys its
/// lookups. Never change <see cref="Identifier"/> in place on an instance the library already holds (stored, cached,
/// resolved, or assigned to a multi-tenant context), because those are keyed by the value captured when the tenant was
/// added or updated. Built-in stores look up <see cref="Identifier"/> case-insensitively by default. The current
/// tenant of a request is changed only by assigning a new <see cref="IMultiTenantContext"/>, never by mutating the
/// current instance.
/// </para>
/// </remarks>
public interface ITenantInfo
{
    /// <summary>
    /// Gets a unique identifier for the tenant. Typically used as the primary key.
    /// </summary>
    /// <remarks>Must be non-empty and never changes: it is the tenant's identity.</remarks>
    public string Id { get; }

    /// <summary>
    /// Gets an externally facing identifier used for tenant resolution.
    /// </summary>
    /// <remarks>
    /// Must be non-empty. May change over a tenant's lifetime, but only by passing a new instance to the store's
    /// update method, never by changing it in place on an instance the library already holds.
    /// </remarks>
    public string Identifier { get; }
}
