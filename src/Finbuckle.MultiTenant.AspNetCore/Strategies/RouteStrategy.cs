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

using System;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.AspNetCore;
using Finbuckle.MultiTenant.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Finbuckle.MultiTenant.Strategies
{
    public class RouteStrategy : IMultiTenantStrategy
    {
        internal readonly string tenantParam;
        internal IRouter router;
        internal readonly Action<IRouteBuilder> configRoutes;

        public RouteStrategy(string tenantParam, Action<IRouteBuilder> configRoutes)
        {
            if (string.IsNullOrWhiteSpace(tenantParam))
            {
                throw new ArgumentException($"\"{nameof(tenantParam)}\" must not be null or whitespace", nameof(tenantParam));
            }

            if (configRoutes == null)
            {
                throw new ArgumentNullException(nameof(configRoutes));
            }

            this.tenantParam = tenantParam;
            this.configRoutes = configRoutes;
        }

        public async Task<string> GetIdentifierAsync(object context)
        {
            if (!(context is HttpContext))
                throw new MultiTenantException(null,
                    new ArgumentException($"\"{nameof(context)}\" type must be of type HttpContext", nameof(context)));

            var httpContext = context as HttpContext;

            // Create the IRouter if not yet created.
            // Done here rather than at startup so that order doesn't matter in ConfigureServices.
            if (router == null)
            {
                var rb = new MultiTenantRouteBuilder(httpContext.RequestServices);
                // Apply explicit routes.
                configRoutes(rb);
                // Insert attribute based routes.
                rb.Routes.Insert(0, AttributeRouting.CreateAttributeMegaRoute(httpContext.RequestServices));

                router = rb.Build();
            }

            // Check the route.
            var routeContext = new RouteContext(httpContext);
            await router.RouteAsync(routeContext);

            object identifier = null;
            routeContext.RouteData?.Values.TryGetValue(tenantParam, out identifier);

            return identifier as string;
        }
    }
}
