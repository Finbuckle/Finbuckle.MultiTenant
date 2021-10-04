// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Finbuckle.MultiTenant.EntityFrameworkCore
{
    public static class FinbuckleModelBuilderExtensions
    {
        /// <summary>
        /// Configures any entity's with the [MultiTenant] attribute.
        /// </summary>
        public static ModelBuilder ConfigureMultiTenant(this ModelBuilder modelBuilder)
        {
            // Call IsMultiTenant() to configure the types marked with the MultiTenant Data Attribute
            foreach (var clrType in modelBuilder.Model.GetEntityTypes()
                                                 .Where(et => et.ClrType.HasMultiTenantAttribute())
                                                 .Select(et => et.ClrType))
            {
                modelBuilder.Entity(clrType)
                            .IsMultiTenant();
            }

            return modelBuilder;
        }
    }
}