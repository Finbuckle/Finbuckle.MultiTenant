﻿// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant.EntityFrameworkCore;

/// <summary>
/// Interface for a <see cref="Microsoft.EntityFrameworkCore.DbContext"/> that supports multi-tenancy.
/// </summary>
public interface IMultiTenantDbContext
{
    /// <summary>
    /// Gets the current tenant information for this context.
    /// </summary>
    ITenantInfo? TenantInfo { get; }

    /// <summary>
    /// Gets the mode used to handle entities where TenantId does not match the current tenant.
    /// </summary>
    TenantMismatchMode TenantMismatchMode { get; }

    /// <summary>
    /// Gets the mode used to handle entities where TenantId is not set.
    /// </summary>
    TenantNotSetMode TenantNotSetMode { get; }

    /// <summary>
    /// Gets a value indicating whether multi-tenant support is enabled.
    /// </summary>
    bool IsMultiTenantEnabled { get; }
}
