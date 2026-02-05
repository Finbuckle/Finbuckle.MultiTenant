// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;

namespace Finbuckle.MultiTenant.AspNetCore.Extensions;

/// <summary>
/// Extension methods for configuring multi-tenant resolution on endpoints.
/// </summary>
public static class EndpointConventionBuilderExtensions
{
    private static readonly ExcludeFromMultiTenantResolutionAttribute s_excludeFromMultiTenantResolutionAttribute =
        new();

    /// <summary>
    /// Adds the <see cref="IExcludeFromMultiTenantResolutionMetadata"/> to <see cref="EndpointBuilder.Metadata"/> 
    /// for all endpoints produced by the <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IEndpointConventionBuilder"/>.</param>
    /// <returns>A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
    public static TBuilder ExcludeFromMultiTenantResolution<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
        => builder.WithMetadata(s_excludeFromMultiTenantResolutionAttribute);

    /// <summary>
    /// Adds the <see cref="IExcludeFromMultiTenantResolutionMetadata"/> to <see cref="EndpointBuilder.Metadata"/> 
    /// for all endpoints produced by the <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="RouteHandlerBuilder"/>.</param>
    /// <returns>A <see cref="RouteHandlerBuilder"/> that can be used to further customize the endpoint.</returns>
    public static RouteHandlerBuilder ExcludeFromMultiTenantResolution(this RouteHandlerBuilder builder)
        => ExcludeFromMultiTenantResolution<RouteHandlerBuilder>(builder);
}