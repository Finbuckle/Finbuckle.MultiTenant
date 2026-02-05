// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Microsoft.EntityFrameworkCore.Metadata;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;

/// <summary>
/// Extension methods for <see cref="IModel"/>.
/// </summary>
public static class ModelExtensions
{
    /// <summary>
    /// Gets all multi-tenant entity types defined in the model.
    /// </summary>
    /// <param name="model">The model from which to list entities.</param>
    /// <returns>Multi-tenant entity types.</returns>
    public static IEnumerable<IEntityType> GetMultiTenantEntityTypes(this IModel model)
    {
        return model.GetEntityTypes().Where(et => et.IsMultiTenant());
    }
}