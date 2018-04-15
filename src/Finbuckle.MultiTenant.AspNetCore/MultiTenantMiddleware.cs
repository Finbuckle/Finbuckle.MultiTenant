using System.Threading.Tasks;
using Finbuckle.MultiTenant.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Finbuckle.MultiTenant.AspNetCore
{
    /// <summary>
    /// Middleware for resolving the <c>TenantContext</c> and storing it in <c>HttpContext</c>.
    /// </summary>
    public class MultiTenantMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IRouter _router;

        public MultiTenantMiddleware(RequestDelegate next)
        {
            this._next = next;
        }

        public MultiTenantMiddleware(RequestDelegate next, IRouter router)
        {
            this._next = next;
            this._router = router;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.Items.ContainsKey(Constants.HttpContextTenantContext))
            {
                var sp = context.RequestServices;
                var resolver = sp.GetRequiredService<TenantResolver>();

                if(_router != null)
                {
                    await _router.RouteAsync(new RouteContext(context)).ConfigureAwait(false);
                }
                
                var tc = await resolver.ResolveAsync(context).ConfigureAwait(false);
                if (tc != null)
                    context.Items.Add(Constants.HttpContextTenantContext, tc);
            }

            if(_next != null)
                await _next(context);
        }
    }
}