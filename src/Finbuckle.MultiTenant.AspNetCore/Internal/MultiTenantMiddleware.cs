// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Finbuckle.MultiTenant.AspNetCore
{
    /// <summary>
    /// Middleware for resolving the TenantContext and storing it in HttpContext.
    /// </summary>
    internal class MultiTenantMiddleware
    {
        private readonly RequestDelegate next;

        public MultiTenantMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var accessor = context.RequestServices.GetRequiredService<IMultiTenantContextAccessor>();

            if (accessor.MultiTenantContext == null)
            {
                var resolver = context.RequestServices.GetRequiredService<ITenantResolver>();
                var multiTenantContext = await resolver.ResolveAsync(context);
                accessor.MultiTenantContext = multiTenantContext;
            }

            await next(context);
        }
    }
}