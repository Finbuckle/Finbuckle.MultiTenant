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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.Core;
using Finbuckle.MultiTenant.Core.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Finbuckle.MultiTenant.AspNetCore
{
    /// <summary>
    /// Middleware for resolving the <c>TenantContext</c> and storing it in <c>HttpContext</c>.
    /// </summary>
    public class MultiTenantMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IRouter router;

        public MultiTenantMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public MultiTenantMiddleware(RequestDelegate next, IRouter router)
        {
            this.next = next;
            this.router = router;
        }

        public async Task Invoke(HttpContext context)
        {
            // Set the tenant context (or null) into the Items collections.
            if (!context.Items.ContainsKey(Constants.HttpContextTenantContext))
            {
                context.Items.Add(Constants.HttpContextTenantContext, null);
                await HandleRouting(context);

                // Try the registered strategy.
                var strategy = context.RequestServices.GetRequiredService<IMultiTenantStrategy>();
                var identifier = strategy.GetIdentifier(context);
                var store = context.RequestServices.GetRequiredService<IMultiTenantStore>();

                TenantContext tenantContext = null;
                if (identifier != null)
                {
                    tenantContext = await store.GetByIdentifierAsync(identifier);
                }

                // Resolve for remote authentication callbacks if applicable.
                if (tenantContext == null &&
                    context.RequestServices.GetService<IAuthenticationSchemeProvider>() is MultiTenantAuthenticationSchemeProvider)
                {
                    strategy = (IMultiTenantStrategy)context.RequestServices.GetRequiredService<IRemoteAuthenticationStrategy>();
                    identifier = strategy.GetIdentifier(context);

                    if (identifier != null)
                    {
                        tenantContext = await store.GetByIdentifierAsync(identifier);
                    }
                }

                context.Items[Constants.HttpContextTenantContext] = tenantContext;
            }

            if (next != null)
            {
                await next(context);
            }
        }

        private async Task HandleRouting(HttpContext context)
        {
            if (router != null)
            {
                var routeContext = new RouteContext(context);
                await router.RouteAsync(routeContext).ConfigureAwait(false);
                if (routeContext.Handler != null)
                {
                    context.Features[typeof(IRoutingFeature)] = new RoutingFeature()
                    {
                        RouteData = routeContext.RouteData
                    };
                }
            }
        }
    }
}