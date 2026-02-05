// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant.EntityFrameworkCore;

/// <summary>
/// Contains constant values for Finbuckle.MultiTenant.EntityFrameworkCore.
/// </summary>
public static class Constants
{
    /// <summary>
    /// The annotation name used to mark entity types as multi-tenant.
    /// </summary>
    public static readonly string MultiTenantAnnotationName = "Finbuckle:MultiTenant";
}