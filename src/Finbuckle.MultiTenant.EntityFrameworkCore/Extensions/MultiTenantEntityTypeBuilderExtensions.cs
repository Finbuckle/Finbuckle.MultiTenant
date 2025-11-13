// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;

/// <summary>
/// Extension methods for configuring multi-tenant entity types.
/// </summary>
public static class MultiTenantEntityTypeBuilderExtensions
{
    /// <summary>
    /// Adds TenantId to all unique indexes.
    /// </summary>
    /// <param name="builder">The <see cref="MultiTenantEntityTypeBuilder"/> instance.</param>
    /// <returns>The <see cref="MultiTenantEntityTypeBuilder"/> instance.</returns>
    public static MultiTenantEntityTypeBuilder AdjustUniqueIndexes(this MultiTenantEntityTypeBuilder builder)
    {
        // Update any unique constraints to include TenantId (unless they already do)
        var indexes = builder.Builder.Metadata.GetIndexes()
            .Where(i => i.IsUnique)
            .Where(i => !i.Properties.Select(p => p.Name).Contains("TenantId"))
            .ToList();

        foreach (var index in indexes.ToArray())
        {
            builder.AdjustIndex(index);
        }

        return builder;
    }

    /// <summary>
    /// Adds TenantId to all indexes.
    /// </summary>
    /// <param name="builder">The <see cref="MultiTenantEntityTypeBuilder"/> instance.</param>
    /// <returns>The <see cref="MultiTenantEntityTypeBuilder"/> instance.</returns>
    public static MultiTenantEntityTypeBuilder AdjustIndexes(this MultiTenantEntityTypeBuilder builder)
    {
        // Update any unique constraints to include TenantId (unless they already do)
        var indexes = builder.Builder.Metadata.GetIndexes()
            .Where(i => !i.Properties.Select(p => p.Name).Contains("TenantId"))
            .ToList();

        foreach (var index in indexes.ToArray())
        {
            builder.AdjustIndex(index);
        }

        return builder;
    }

    // TODO why was this internal?
    // <summary>
    // Adds TenantId to the primary and alternate keys and adds the TenantId property to any dependent types' foreign keys.
    // </summary>
    // <param name="builder">The MultiTenantEntityTypeBuilder instance.</param>
    // <param name="modelBuilder">The modelBuilder for the database context.</param>
    // <returns>The MultiTenantEntityTypeBuilder instance.</returns>
    // internal static MultiTenantEntityTypeBuilder AdjustKeys(this MultiTenantEntityTypeBuilder builder, ModelBuilder modelBuilder)
    // {
    //     var keys = builder.Builder.Metadata.GetKeys();
    //     foreach (var key in keys.ToArray())
    //     {
    //         builder.AdjustKey(key, modelBuilder);
    //     }
    //
    //     return builder;
    // }
}