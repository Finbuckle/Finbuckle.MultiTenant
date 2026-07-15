// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant.Abstractions;

/// <summary>
/// Non-generic interface for the tenant context.
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// Information about the tenant for this context.
    /// </summary>
    ITenantInfo? TenantInfo { get; set; }
    /// <summary>
    /// True if a tenant has been resolved and <see cref="ITenantInfo"/> is not null.
    /// </summary>
    bool IsResolved => TenantInfo != null;
}

/// <summary>
/// Generic interface for the tenant context.
/// </summary>
/// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
public interface ITenantContext<TTenantInfo> : ITenantContext
    where TTenantInfo : ITenantInfo
{
    /// <summary>
    /// Information about the tenant for this context.
    /// </summary>
    new TTenantInfo? TenantInfo { get; set; }
}
