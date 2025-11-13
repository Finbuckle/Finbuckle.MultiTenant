// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant.EntityFrameworkCore;

/// <summary>
/// Interface for a DbContext that supports multi-tenancy.
/// </summary>
public interface IMultiTenantDbContext
{
    /// <summary>
    /// Gets the current tenant information for this context.
    /// </summary>
    TenantInfo? TenantInfo { get; }
    
    /// <summary>
    /// Gets the mode used to handle entities where TenantId does not match the current tenant.
    /// </summary>
    TenantMismatchMode TenantMismatchMode { get; }
    
    /// <summary>
    /// Gets the mode used to handle entities where TenantId is not set.
    /// </summary>
    TenantNotSetMode TenantNotSetMode { get; }
}