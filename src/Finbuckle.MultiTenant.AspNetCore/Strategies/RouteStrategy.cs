// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Finbuckle.MultiTenant.AspNetCore.Strategies;

/// <summary>
/// A strategy that determines the tenant identifier from a route parameter.
/// </summary>
public class RouteStrategy : IMultiTenantStrategy
{
    internal readonly string TenantParam;

    /// <summary>
    /// Initializes a new instance of RouteStrategy.
    /// </summary>
    /// <param name="tenantParam">The name of the route parameter containing the tenant identifier.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="tenantParam"/> is null or whitespace.</exception>
    public RouteStrategy(string tenantParam)
    {
        if (string.IsNullOrWhiteSpace(tenantParam))
        {
            throw new ArgumentException($"\"{nameof(tenantParam)}\" must not be null or whitespace",
                nameof(tenantParam));
        }

        TenantParam = tenantParam;
    }

    /// <inheritdoc />
    public Task<string?> GetIdentifierAsync(object context)
    {
        if (context is not HttpContext httpContext)
            return Task.FromResult<string?>(null);

        httpContext.Request.RouteValues.TryGetValue(TenantParam, out var identifier);

        return Task.FromResult(identifier as string);
    }
}

// #endif