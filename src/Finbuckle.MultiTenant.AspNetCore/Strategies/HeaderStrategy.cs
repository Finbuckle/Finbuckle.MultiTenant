// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Finbuckle.MultiTenant.Strategies
{
    public class HeaderStrategy : IMultiTenantStrategy
    {
        private readonly string _headerKey;
        public HeaderStrategy(string headerKey)
        {
            _headerKey = headerKey;
        }

        public async Task<string?> GetIdentifierAsync(object context)
        {
            if (!(context is HttpContext httpContext))
                throw new MultiTenantException(null,
                    new ArgumentException($"\"{nameof(context)}\" type must be of type HttpContext", nameof(context)));

            return await Task.FromResult(httpContext?.Request.Headers[_headerKey]);
        }
    }
}
