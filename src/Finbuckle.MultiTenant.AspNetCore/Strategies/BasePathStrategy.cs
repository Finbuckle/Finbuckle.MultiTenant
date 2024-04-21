// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Finbuckle.MultiTenant.AspNetCore.Strategies;

public class BasePathStrategy : IMultiTenantStrategy
{
    public Task<string?> GetIdentifierAsync(object context)
    {
            if (!(context is HttpContext httpContext))
                throw new MultiTenantException(null,
                    new ArgumentException($"\"{nameof(context)}\" type must be of type HttpContext", nameof(context)));

            var path = httpContext.Request.Path;

            var pathSegments =
                path.Value?.Split('/', 2, StringSplitOptions.RemoveEmptyEntries);

            if (pathSegments is null || pathSegments.Length == 0)
                return Task.FromResult<string?>(null);

            string identifier = pathSegments[0];

            return Task.FromResult<string?>(identifier);
        }
}