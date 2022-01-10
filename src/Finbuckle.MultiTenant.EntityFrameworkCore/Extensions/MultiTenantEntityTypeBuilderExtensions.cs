// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System.Linq;

namespace Finbuckle.MultiTenant.EntityFrameworkCore
{
    public static class MultiTenantEntityTypeBuilderExtensions
    {
        /// <summary>
        /// Adds TenantId to all unique indexes.
        /// </summary>
        /// <returns>The MultiTenantEntityTypeBuilder&lt;T&gt; instance.</returns>
        public static MultiTenantEntityTypeBuilder AdjustUniqueIndexes(this MultiTenantEntityTypeBuilder builder)
        {
            // Update any unique constraints to include TenantId (unless they already do)
            var indexes = builder.Builder.Metadata.GetIndexes()
                                                  .Where(i => i.IsUnique)
                                                  .Where(i => !i.Properties.Select(p => p.Name).Contains("TenantId"))
                                                  .ToList();

            foreach (var index in indexes)
            {
                builder.AdjustIndex(index);
            }

            return builder;
        }

        /// <summary>
        /// Adds TenantId to all indexes.
        /// </summary>
        /// <returns>The MultiTenantEntityTypeBuilder&lt;T&gt; instance.</returns>
        public static MultiTenantEntityTypeBuilder AdjustIndexes(this MultiTenantEntityTypeBuilder builder)
        {
            // Update any unique constraints to include TenantId (unless they already do)
            var indexes = builder.Builder.Metadata.GetIndexes()
                .Where(i => !i.Properties.Select(p => p.Name).Contains("TenantId"))
                .ToList();

            foreach (var index in indexes)
            {
                builder.AdjustIndex(index);
            }

            return builder;
        }
    }
}