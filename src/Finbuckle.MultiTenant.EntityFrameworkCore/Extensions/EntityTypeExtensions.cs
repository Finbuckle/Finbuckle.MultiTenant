// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Microsoft.EntityFrameworkCore.Metadata;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;

/// <summary>
/// Extension methods for <see cref="IReadOnlyEntityType"/>.
/// </summary>
public static class EntityTypeExtensions
{
    extension<T>(T entityType)
        where T : class, IReadOnlyEntityType
    {

        /// <summary>
        /// Whether or not the entity type is configured as multi-tenant.
        /// </summary>
        public bool IsMultiTenant
        {
            get
            {
                IReadOnlyEntityType? current = entityType;
                while (current != null)
                {
                    var hasMultiTenantAnnotation =
                        (bool?)current.FindAnnotation(Constants.MultiTenantAnnotationName)?.Value ?? false;

                    if (hasMultiTenantAnnotation)
                        return true;

                    current = current.BaseType;
                }

                return false;
            }
        }
    }
}
