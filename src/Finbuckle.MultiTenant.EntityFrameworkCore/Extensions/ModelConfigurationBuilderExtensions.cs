// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;

/// <summary>
/// Extension methods for <see cref="ModelConfigurationBuilder"/>.
/// </summary>
public static class ModelConfigurationBuilderExtensions
{
    /// <summary>
    /// Adds a convention that throws a <see cref="MultiTenantException"/> when the model is built if a multi-tenant
    /// entity type has a TenantId property whose type is not exactly <typeparamref name="TId"/>.
    /// </summary>
    /// <typeparam name="TId">The tenant ID type of the database context.</typeparam>
    /// <param name="configurationBuilder">The <see cref="ModelConfigurationBuilder"/> instance.</param>
    /// <returns>The <see cref="ModelConfigurationBuilder"/> instance.</returns>
    /// <remarks>
    /// <see cref="MultiTenantDbContext{TId}"/> and <c>MultiTenantIdentityDbContext</c> call this from their
    /// <c>ConfigureConventions</c> override, so a derived context that overrides <c>ConfigureConventions</c> must call the
    /// base implementation to keep the check. Call it from <c>ConfigureConventions</c> in a database context that
    /// implements <see cref="IMultiTenantDbContext{TId}"/> directly. Entity types that are not multi-tenant are not checked.
    /// </remarks>
    public static ModelConfigurationBuilder ValidateMultiTenantIdType<TId>(
        this ModelConfigurationBuilder configurationBuilder) where TId : IEquatable<TId>
    {
        ArgumentNullException.ThrowIfNull(configurationBuilder);
        configurationBuilder.Conventions.Add(_ => new TenantIdTypeValidationConvention(typeof(TId)));
        return configurationBuilder;
    }
}
