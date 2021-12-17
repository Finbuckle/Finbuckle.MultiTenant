// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Finbuckle.MultiTenant.Strategies
{
    public class SessionStrategy : IMultiTenantStrategy
    {
        private readonly string tenantKey;

        public SessionStrategy(string tenantKey)
        {
            if (string.IsNullOrWhiteSpace(tenantKey))
            {
                throw new ArgumentException("message", nameof(tenantKey));
            }

            this.tenantKey = tenantKey;
        }

        public async Task<string?> GetIdentifierAsync(object context)
        {
            if(!(context is HttpContext httpContext))
                throw new MultiTenantException(null,
                    new ArgumentException($"\"{nameof(context)}\" type must be of type HttpContext", nameof(context)));

            var identifier = httpContext.Session.GetString(tenantKey);
            return await Task.FromResult(identifier); // Prevent the compliler warning that no await exists.
        }
    }
}