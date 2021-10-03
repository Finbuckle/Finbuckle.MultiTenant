// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

namespace Finbuckle.MultiTenant
{

    /// <summary>
    /// Determines how entities where TenantId does not match the TenantContext are handled
    /// when SaveChanges or SaveChangesAsync is called.
    /// </summary>
    public enum TenantMismatchMode
    {
        Throw,
        Ignore,
        Overwrite
    }

    /// <summary>
    /// Determines how entities with null TenantId are handled
    /// when SaveChanges or SaveChangesAsync is called.
    /// </summary>
    public enum TenantNotSetMode
    {
        Throw,
        Overwrite
    }
}