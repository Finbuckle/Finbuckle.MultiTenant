using System.Threading.Tasks;
using Finbuckle.MultiTenant.Core.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Finbuckle.MultiTenant.AspNetCore
{
    /// <summary>
    /// Used by <c>MultiTenantMiddleware</c> and <c>RouteTenantResolver</c> to determine the tenant identifier.
    /// </summary>
    public class MultiTenantRouteHandler : IRouteHandler, IRouter
    {
        private RequestDelegate _requestDelegate = (HttpContext) => null;

        public RequestDelegate GetRequestHandler(HttpContext httpContext, RouteData routeData)
        {
            return _requestDelegate;
        }

        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            return null;
        }

        public Task RouteAsync(RouteContext context)
        {
            // set the context Handler so route matching will stop
            context.Handler = _requestDelegate;

            // set the HttpContext feature so that a Route based TenantResolver can use it
            // note: this may be overwritten by later middleware
            context.HttpContext.Features[typeof(IRoutingFeature)] = new RoutingFeature()
            {
                RouteData = context.RouteData,
            };

            return Task.CompletedTask;
        }
    }
}