// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Finbuckle.MultiTenant.EntityFrameworkCore;

public static class MultiTenantEntityTypeBuilderExtensions
{
    /// <summary>
    /// Adds TenantId to all unique indexes.
    /// </summary>
    /// <param name="builder">Thet MultiTenantEntityTypeBuilder instance.</param>
    /// <returns>The MultiTenantEntityTypeBuilder instance.</returns>
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
    /// <param name="builder">Thet MultiTenantEntityTypeBuilder instance.</param>
    /// <returns>The MultiTenantEntityTypeBuilder instance.</returns>
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

    /// <summary>
    /// Adds TenantId to the primary and alternate keys and adds the TenantId property to any dependent types' foreign keys.
    /// </summary>
    /// <param name="builder">Thet MultiTenantEntityTypeBuilder instance.</param>
    /// <param name="modelBuilder">The modelBuilder for the database ontext.</param>
    /// <returns>The MultiTenantEntityTypeBuilder instance.</returns>
    internal static MultiTenantEntityTypeBuilder AdjustKeys(this MultiTenantEntityTypeBuilder builder, ModelBuilder modelBuilder)
    {
        var keys = builder.Builder.Metadata.GetKeys();
        foreach (var key in keys.ToArray())
        {
            builder.AdjustKey(key, modelBuilder);
        }

        return builder;
    }
}