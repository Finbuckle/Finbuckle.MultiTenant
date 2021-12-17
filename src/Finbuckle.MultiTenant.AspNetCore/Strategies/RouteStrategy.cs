// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Finbuckle.MultiTenant.Strategies
{
    public class RouteStrategy : IMultiTenantStrategy
    {
        internal readonly string tenantParam;

        public RouteStrategy(string tenantParam)
        {
            if (string.IsNullOrWhiteSpace(tenantParam))
            {
                throw new ArgumentException($"\"{nameof(tenantParam)}\" must not be null or whitespace", nameof(tenantParam));
            }

            this.tenantParam = tenantParam;
        }

        public async Task<string?> GetIdentifierAsync(object context)
        {

            if (!(context is HttpContext httpContext))
                throw new MultiTenantException(null,
                    new ArgumentException($"\"{nameof(context)}\" type must be of type HttpContext", nameof(context)));

            object? identifier;
            httpContext.Request.RouteValues.TryGetValue(tenantParam, out identifier);

            return await Task.FromResult(identifier as string);
        }
    }
}

// #endif