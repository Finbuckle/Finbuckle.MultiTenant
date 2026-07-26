// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant.Abstractions;

/// <summary>
/// Non-generic interface for the tenant context.
/// </summary>
public interface ITenantContext<TId> where TId : IEquatable<TId>
{
    /// <summary>
    /// Information about the tenant for this context.
    /// </summary>
    ITenantInfo<TId>? TenantInfo { get; set; }
    /// <summary>
    /// True if a tenant has been resolved and <see cref="ITenantInfo{TId}"/> is not null.
    /// </summary>
    bool IsResolved => TenantInfo != null;
}

/// <summary>
/// Generic interface for the tenant context.
/// </summary>
/// <typeparam name="TTenantInfo">The <see cref="ITenantInfo{TId}"/> implementation type.</typeparam>
/// <typeparam name="TId">The ID implementation type.</typeparam>
public interface ITenantContext<TTenantInfo, TId> : ITenantContext<TId>
    where TTenantInfo : ITenantInfo<TId> where TId : IEquatable<TId>
{
    /// <summary>
    /// Information about the tenant for this context.
    /// </summary>
    new TTenantInfo? TenantInfo { get; set; }
}
