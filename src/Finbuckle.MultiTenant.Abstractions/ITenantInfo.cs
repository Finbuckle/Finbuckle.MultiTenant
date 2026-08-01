namespace Finbuckle.MultiTenant.Abstractions;

/// <summary>
/// Represents the basic information for a tenant in a multi-tenant application.
/// </summary>
public interface ITenantInfo<out TId> where TId : IEquatable<TId>
{
    /// <summary>
    /// Gets a unique identifier for the tenant. Typically used as the primary key.
    /// </summary>
    public TId Id { get; }

    /// <summary>
    /// Gets an externally facing identifier used for tenant resolution.
    /// </summary>
    public string Identifier { get; }
}