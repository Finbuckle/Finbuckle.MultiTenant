// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Microsoft.EntityFrameworkCore.Metadata;

// ReSharper disable once CheckNamespace
namespace Finbuckle.MultiTenant;

/// <summary>
/// Extension methods for IModel.
/// </summary>
public static class ModelExtensions
{
    /// <summary>
    /// Gets all MultiTenant entity types defined in the model.
    /// </summary>
    /// <param name="model">The model from which to list entities.</param>
    /// <returns>MultiTenant entity types.</returns>
    public static IEnumerable<IEntityType> GetMultiTenantEntityTypes(this IModel model)
    {
            return model.GetEntityTypes().Where(et => et.IsMultiTenant());
        }
}