﻿// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant.EntityFrameworkCore;

/// <summary>
/// Interface for a <see cref="Microsoft.EntityFrameworkCore.DbContext"/> that supports multi-tenancy.
/// </summary>
public interface IMultiTenantDbContext<TId> where TId : IEquatable<TId>
{
    /// <summary>
    /// Gets or sets the current tenant information for this context.
    /// </summary>
    /// <remarks>
    /// Setting the <see cref="ITenantInfo{TId}"/> may cause conflicts for entities already being tracked. Use with caution.
    /// </remarks>
    ITenantInfo<TId>? TenantInfo { get; set; }

    /// <summary>
    /// Gets the mode used to handle entities where TenantId does not match the current tenant.
    /// </summary>
    TenantMismatchMode TenantMismatchMode { get; }

    /// <summary>
    /// Gets the mode used to handle entities where TenantId is not set.
    /// </summary>
    TenantNotSetMode TenantNotSetMode { get; }
}