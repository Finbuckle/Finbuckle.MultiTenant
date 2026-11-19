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
    /// Thrown when no per-request ambient tenant scope is configured with <c>AddHttpAmbientTenantScope</c>, or when
    /// <see cref="ITenantScopeProvider"/> is not registered as a singleton.
    /// </exception>
    public static IApplicationBuilder UseMultiTenant<TId>(this IApplicationBuilder builder) where TId : IEquatable<TId>
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (builder.ApplicationServices.GetService<HttpAmbientTenantScopeMarker>() is null)
            throw new MultiTenantException(HttpAmbientTenantScopeMarker.NotConfiguredMessage);

        EnsureSingletonTenantScopeProvider(builder.ApplicationServices);

        return builder.UseMiddleware<MultiTenantMiddleware<TId>>();
    }

    // TenantScopeStartupFilter is a singleton, so it gets ITenantScopeProvider from the root provider, while the
    // middleware gets it from the request's services. Both must be the same instance. Resolve it from two scopes, and
    // twice from one, to catch scoped and transient registrations without resolving from the root, which would fail
    // scope validation for a scoped registration before this clearer error could be thrown.
    private static void EnsureSingletonTenantScopeProvider(IServiceProvider services)
    {
        using var firstScope = services.CreateScope();
        using var secondScope = services.CreateScope();
        var first = firstScope.ServiceProvider.GetRequiredService<ITenantScopeProvider>();

        if (!ReferenceEquals(first, firstScope.ServiceProvider.GetRequiredService<ITenantScopeProvider>()) ||
            !ReferenceEquals(first, secondScope.ServiceProvider.GetRequiredService<ITenantScopeProvider>()))
            throw new MultiTenantException(HttpAmbientTenantScopeMarker.NotSingletonMessage);
    }
}
