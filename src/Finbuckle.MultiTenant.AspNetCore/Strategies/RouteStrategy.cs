// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Finbuckle.MultiTenant.AspNetCore.Strategies;

public class RouteStrategy : IMultiTenantStrategy
{
    internal readonly string TenantParam;

    public RouteStrategy(string tenantParam)
    {
            if (string.IsNullOrWhiteSpace(tenantParam))
            {
                throw new ArgumentException($"\"{nameof(tenantParam)}\" must not be null or whitespace", nameof(tenantParam));
            }

            this.TenantParam = tenantParam;
        }

    public Task<string?> GetIdentifierAsync(object context)
    {

            if (context is not HttpContext httpContext)
                return Task.FromResult<string?>(null);

            httpContext.Request.RouteValues.TryGetValue(TenantParam, out var identifier);

            return Task.FromResult(identifier as string);
        }
}

// #endif