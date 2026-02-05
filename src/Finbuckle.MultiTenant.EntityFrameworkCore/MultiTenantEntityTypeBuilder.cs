// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Collections.Immutable;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finbuckle.MultiTenant.EntityFrameworkCore;

/// <summary>
/// Builder for configuring multi-tenant entity types.
/// </summary>
public class MultiTenantEntityTypeBuilder
{
    /// <summary>
    /// Gets the underlying <see cref="EntityTypeBuilder"/>.
    /// </summary>
    public EntityTypeBuilder Builder { get; }

    /// <summary>
    /// Initializes a new instance of MultiTenantEntityTypeBuilder.
    /// </summary>
    /// <param name="builder">The <see cref="EntityTypeBuilder"/> to wrap.</param>
    public MultiTenantEntityTypeBuilder(EntityTypeBuilder builder)
    {
        Builder = builder;
    }

    /// <summary>
    /// Adds TenantId to the index.
    /// </summary>
    /// <param name="index">The index to adjust for TenantId.</param>
    /// <returns>The <see cref="MultiTenantEntityTypeBuilder"/> instance.</returns>
    public MultiTenantEntityTypeBuilder AdjustIndex(IMutableIndex index)
    {
        // set the new unique index with TenantId preserving name and database name
        IndexBuilder indexBuilder;
        Builder.Metadata.RemoveIndex(index);
        if (index.Name != null)
            indexBuilder = Builder
                .HasIndex(index.Properties.Select(p => p.Name).Append("TenantId").ToArray(), index.Name)
                .HasDatabaseName(index.GetDatabaseName());
        else
            indexBuilder = Builder.HasIndex(index.Properties.Select(p => p.Name).Append("TenantId").ToArray())
                .HasDatabaseName(index.GetDatabaseName());

        if (index.IsUnique)
            indexBuilder.IsUnique();

        if (index.GetFilter() is string filter)
        {
            indexBuilder.HasFilter(filter);
        }

        foreach (var annotation in index.GetAnnotations())
        {
            indexBuilder.HasAnnotation(annotation.Name, annotation.Value);
        }

        return this;
    }

    /// <summary>
    /// Adds TenantId to the key and adds the TenantId property to any dependent types' foreign keys.
    /// </summary>
    /// <param name="key">The key to adjust for TenantId.</param>
    /// <param name="modelBuilder">The modelBuilder for the DbContext.</param>
    /// <returns>The MultiTenantEntityTypeBuilder&lt;T&gt; instance.</returns>
    public MultiTenantEntityTypeBuilder AdjustKey(IMutableKey key, ModelBuilder modelBuilder)
    {
        var prop = Builder.Metadata.GetProperty("TenantId");
        var props = key.Properties.Append(prop).ToImmutableList();
        var foreignKeys = key.GetReferencingForeignKeys().ToArray();
        var annotations = key.GetAnnotations();
        var newKey = key.IsPrimaryKey() ? Builder.Metadata.SetPrimaryKey(props) : Builder.Metadata.AddKey(props);

        foreach (var fk in foreignKeys)
        {
            var fkEntityBuilder = modelBuilder.Entity(fk.DeclaringEntityType.ClrType);
            var newFkProp = fkEntityBuilder.Property<string>("TenantId").Metadata;
            var fkProps = fk.Properties.Append(newFkProp).ToImmutableList();
            fk.SetProperties(fkProps, newKey!);
        }

        foreach (var annotation in annotations)
        {
            if (newKey?.FindAnnotation(annotation.Name) is null)
            {
                newKey?.AddAnnotation(annotation.Name, annotation.Value);
            }
        }

        // remove key
        Builder.Metadata.RemoveKey(key);

        return this;
    }
}