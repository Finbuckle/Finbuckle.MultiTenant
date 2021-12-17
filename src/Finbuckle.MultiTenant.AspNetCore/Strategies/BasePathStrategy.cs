// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Finbuckle.MultiTenant.Strategies
{
    public class BasePathStrategy : IMultiTenantStrategy
    {
        public async Task<string?> GetIdentifierAsync(object context)
        {
            if(!(context is HttpContext httpContext))
                throw new MultiTenantException(null,
                    new ArgumentException($"\"{nameof(context)}\" type must be of type HttpContext", nameof(context)));

            var path = httpContext.Request.Path;

            var pathSegments =
                path.Value?.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (pathSegments is null || pathSegments.Length == 0)
                return null;

            string identifier = pathSegments[0];

            return await Task.FromResult(identifier); // Prevent the compliler warning that no await exists.
        }
    }
}