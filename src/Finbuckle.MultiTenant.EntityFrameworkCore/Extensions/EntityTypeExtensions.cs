// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using Microsoft.EntityFrameworkCore.Metadata;

namespace Finbuckle.MultiTenant.EntityFrameworkCore
{
    public static class EntityTypeExtensions
    {
        /// <summary>
        /// Whether or not the <see cref="IEntityType"/> is configured as MultiTenant.
        /// </summary>
        /// <param name="entityType">The entity type to test for MultiTenant configuration.</param>
        /// <returns><see cref="true"/> if the entity type has MultiTenant configuration, <see cref="false"/> if not.</returns>
        public static bool IsMultiTenant(this IMutableEntityType? entityType)
        {
            while (entityType != null)
            {
                var hasMultiTenantAnnotation = (bool?) entityType.FindAnnotation(Constants.MultiTenantAnnotationName)?.Value ?? false;
                if (hasMultiTenantAnnotation)
                    return true;
                entityType = entityType.BaseType;
            }

            return false;
        }

        public static bool IsMultiTenant(this IEntityType? entityType)
        {
            while (entityType != null)
            {
                var hasMultiTenantAnnotation = (bool?) entityType.FindAnnotation(Constants.MultiTenantAnnotationName)?.Value ?? false;
                if (hasMultiTenantAnnotation)
                    return true;
                entityType = entityType.BaseType;
            }

            return false;
        }
    }
}
