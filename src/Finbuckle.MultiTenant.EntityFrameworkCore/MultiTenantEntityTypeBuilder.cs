// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finbuckle.MultiTenant.EntityFrameworkCore
{
    public class MultiTenantEntityTypeBuilder
    {
        public EntityTypeBuilder Builder { get; }

        public MultiTenantEntityTypeBuilder(EntityTypeBuilder builder)
        {
            Builder = builder;
        }

        /// <summary>
        /// Adds TenantId to the index.
        /// </summary>
        /// <param name="index">The index to adjust for TenantId.</param>
        /// <returns>The MultiTenantEntityTypeBuilder instance.</returns>
        public MultiTenantEntityTypeBuilder AdjustIndex(IMutableIndex index)
        {
            // Set the new unique index with TenantId preserving name and database name
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

            return this;
        }

        /// <summary>
        /// Adds TenantId to the key and adds a TenantId shadow property on any dependent types' foreign keys.
        /// </summary>
        /// <param name="key">The key to adjust for TenantId.</param>
        /// <param name="modelBuilder">The modelBuilder for the DbContext.</param>
        /// <returns>The MultiTenantEntityTypeBuilder&lt;T&gt; instance.</returns>
        public MultiTenantEntityTypeBuilder AdjustKey(IMutableKey key, ModelBuilder modelBuilder)
        {
            var propertyNames = key.Properties.Select(p => p.Name).Append("TenantId").ToArray();
            var fks = key.GetReferencingForeignKeys().ToList();

            if (key.IsPrimaryKey())
                // 6.0 - key replaced on entity, fks changed in-place
                Builder.HasKey(propertyNames);
            else
                Builder.HasAlternateKey(propertyNames);

            foreach (var fk in fks)
            {
                var fkEntityBuilder = modelBuilder.Entity(fk.DeclaringEntityType.ClrType);
                // Note 6.0+ will generate a shadow property with the wrong name in the Properties, we will replace.
                var props = fk.Properties.Where(p => !p.Name.EndsWith("TenantId")).Select(p => p.Name).Append("TenantId").ToArray();
                fkEntityBuilder.Property<string>("TenantId");
                fkEntityBuilder.HasOne(fk.PrincipalEntityType.ClrType, fk.DependentToPrincipal?.Name)
                               .WithMany(fk.PrincipalToDependent?.Name)
                               .HasForeignKey(props)
                               .HasPrincipalKey(propertyNames);
            }

            Builder.Metadata.RemoveKey(key.Properties);
            return this;
        }
    }
}