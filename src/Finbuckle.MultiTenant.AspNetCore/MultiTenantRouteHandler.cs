//    Copyright 2018 Andrew White
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

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
        private RequestDelegate requestDelegate = (HttpContext) => null;

        public RequestDelegate GetRequestHandler(HttpContext httpContext, RouteData routeData)
        {
            return requestDelegate;
        }

        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            return null;
        }

        public Task RouteAsync(RouteContext context)
        {
            // Set the context Handler so route matching will stop.
            context.Handler = requestDelegate;

            // Set the HttpContext feature so that a route based TenantResolver can use it.
            // This may be overwritten by later MVC or other middleware.
            context.HttpContext.Features[typeof(IRoutingFeature)] = new RoutingFeature()
            {
                RouteData = context.RouteData,
            };

            return Task.CompletedTask;
        }
    }
}