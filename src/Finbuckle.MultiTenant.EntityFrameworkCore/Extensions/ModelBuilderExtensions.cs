// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;

/// <summary>
/// Extension methods for <see cref="ModelBuilder"/>.
/// </summary>
public static class FinbuckleModelBuilderExtensions
{
    /// <summary>
    /// Configures any entity types with the <see cref="Abstractions.MultiTenantAttribute"/> attribute.
    /// </summary>
    /// <param name="modelBuilder">The <see cref="ModelBuilder"/> instance.</param>
    /// <returns>The <see cref="ModelBuilder"/> instance.</returns>
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