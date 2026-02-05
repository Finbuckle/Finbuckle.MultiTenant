// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Microsoft.EntityFrameworkCore.Metadata;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;

/// <summary>
/// Extension methods for <see cref="IEntityType"/>.
/// </summary>
public static class EntityTypeExtensions
{
    /// <summary>
    /// Whether or not the <see cref="IMutableEntityType"/> is configured as multi-tenant.
    /// </summary>
    /// <param name="entityType">The entity type to test for multi-tenant configuration.</param>
    /// <returns>Returns true if the entity type has multi-tenant configuration, false if not.</returns>
    public static bool IsMultiTenant(this IMutableEntityType? entityType)
    {
        while (entityType != null)
        {
            var hasMultiTenantAnnotation =
                (bool?)entityType.FindAnnotation(Constants.MultiTenantAnnotationName)?.Value ?? false;
            if (hasMultiTenantAnnotation)
                return true;
            entityType = entityType.BaseType;
        }

        return false;
    }

    /// <summary>
    /// Whether or not the <see cref="IEntityType"/> is configured as multi-tenant.
    /// </summary>
    /// <param name="entityType">The entity type to test for multi-tenant configuration.</param>
    /// <returns>Returns true if the entity type has multi-tenant configuration, false if not.</returns>
    public static bool IsMultiTenant(this IEntityType? entityType)
    {
        while (entityType != null)
        {
            var hasMultiTenantAnnotation =
                (bool?)entityType.FindAnnotation(Constants.MultiTenantAnnotationName)?.Value ?? false;
            if (hasMultiTenantAnnotation)
                return true;
            entityType = entityType.BaseType;
        }

        return false;
    }
}