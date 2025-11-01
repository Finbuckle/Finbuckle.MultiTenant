// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Finbuckle.MultiTenant.AspNetCore.Strategies;

/// <summary>
/// A strategy that determines the tenant identifier from the first segment of the request path.
/// </summary>
public class BasePathStrategy : IMultiTenantStrategy
{
    /// <inheritdoc />
    public Task<string?> GetIdentifierAsync(object context)
    {
        if (context is not HttpContext httpContext)
            return Task.FromResult<string?>(null);

        var path = httpContext.Request.Path;

        var pathSegments =
            path.Value?.Split('/', 2, StringSplitOptions.RemoveEmptyEntries);

        if (pathSegments is null || pathSegments.Length == 0)
            return Task.FromResult<string?>(null);

        string identifier = pathSegments[0];

        return Task.FromResult<string?>(identifier);
    }
}