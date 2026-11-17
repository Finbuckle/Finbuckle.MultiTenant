// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Finbuckle.MultiTenant.AspNetCore.Extensions;

/// <summary>
/// Extension methods for using Finbuckle.MultiTenant.AspNetCore.
/// </summary>
public static class FinbuckleMultiTenantApplicationBuilderExtensions
{
    /// <summary>
    /// Use Finbuckle.MultiTenant middleware in processing the request.
    /// </summary>
    /// <param name="builder">The <see cref="IApplicationBuilder"/> instance the extension method applies to.</param>
    /// <returns>The same <see cref="IApplicationBuilder"/> passed into the method.</returns>
    /// <exception cref="MultiTenantException">
    /// Thrown when no per-request ambient tenant scope is configured with <c>AddHttpAmbientTenantScope</c>.
    /// </exception>
    public static IApplicationBuilder UseMultiTenant<TId>(this IApplicationBuilder builder) where TId : IEquatable<TId>
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (builder.ApplicationServices.GetService<HttpAmbientTenantScopeMarker>() is null)
            throw new MultiTenantException(HttpAmbientTenantScopeMarker.NotConfiguredMessage);

        return builder.UseMiddleware<MultiTenantMiddleware<TId>>();
    }
}
