// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant.EntityFrameworkCore;

/// <summary>
/// Determines how entities where TenantId does not match the TenantContext are handled
/// when SaveChanges or SaveChangesAsync is called.
/// </summary>
public enum TenantMismatchMode
{
    /// <summary>
    /// Throw an exception when a mismatch is detected.
    /// </summary>
    Throw,

    /// <summary>
    /// Ignore the mismatch and save the entity as-is.
    /// </summary>
    Ignore,

    /// <summary>
    /// Overwrite the entity's TenantId with the current tenant's Id.
    /// </summary>
    Overwrite
}

/// <summary>
/// Determines how entities with null TenantId are handled
/// when SaveChanges or SaveChangesAsync is called.
/// </summary>
public enum TenantNotSetMode
{
    /// <summary>
    /// Throw an exception when TenantId is not set.
    /// </summary>
    Throw,

    /// <summary>
    /// Overwrite the entity's TenantId with the current tenant's Id.
    /// </summary>
    Overwrite
}