//    Copyright 2018-2020 Andrew White
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

#if NETCOREAPP2_1

using System;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Routing;

namespace Finbuckle.MultiTenant.Strategies
{
    public class RouteStrategy : IMultiTenantStrategy
    {
        internal readonly string tenantParam;
        internal IRouter router;
        internal readonly Action<IRouteBuilder> configRoutes;
        private readonly IActionDescriptorCollectionProvider actionDescriptorCollectionProvider;
        private int actionDescriptorsVersion = -1;

        public RouteStrategy(string tenantParam, Action<IRouteBuilder> configRoutes, IActionDescriptorCollectionProvider actionDescriptorCollectionProvider)
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
            this.actionDescriptorCollectionProvider = actionDescriptorCollectionProvider ?? throw new ArgumentNullException(nameof(actionDescriptorCollectionProvider));
        }

        public async Task<string> GetIdentifierAsync(object context)
        {
            if (!(context is HttpContext))
                throw new MultiTenantException(null,
                    new ArgumentException($"\"{nameof(context)}\" type must be of type HttpContext", nameof(context)));

            var httpContext = context as HttpContext;

            // Detect if app model changed (eg Razor Pages file update)
            if (actionDescriptorsVersion != actionDescriptorCollectionProvider.ActionDescriptors.Version)
            {
                actionDescriptorsVersion = actionDescriptorCollectionProvider.ActionDescriptors.Version;
                router = null;
            }

            // Create the IRouter if not yet created.
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

#else

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

        public async Task<string> GetIdentifierAsync(object context)
        {

            if (!(context is HttpContext))
                throw new MultiTenantException(null,
                    new ArgumentException($"\"{nameof(context)}\" type must be of type HttpContext", nameof(context)));

            var httpContext = context as HttpContext;

            object identifier = null;
            httpContext.Request.RouteValues.TryGetValue(tenantParam, out identifier);

            return await Task.FromResult(identifier as string);
        }
    }
}

#endif