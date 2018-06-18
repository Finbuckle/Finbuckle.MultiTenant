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

                // Resolve normally.
                var tenantContext = await ResolveAsync(context);

                if (tenantContext == null)
                {
                    // Resolve for remote authentication callbacks.
                    tenantContext = await ResolveForRemoteAuthentication(context);
                }

                context.Items[Constants.HttpContextTenantContext] = tenantContext;
            }

            if (next != null)
                await next(context);
        }

        private async Task<TenantContext> ResolveAsync(HttpContext context)
        {
            var sp = context.RequestServices;
            var resolver = sp.GetRequiredService<TenantResolver>();

            if (router != null)
            {
                await router.RouteAsync(new RouteContext(context)).ConfigureAwait(false);
            }

            return await resolver.ResolveAsync(context).ConfigureAwait(false);
        }

        private static async Task<TenantContext> ResolveForRemoteAuthentication(HttpContext context)
        {
            var schemes = context.RequestServices.GetService<IAuthenticationSchemeProvider>();
            var handlers = context.RequestServices.GetService<IAuthenticationHandlerProvider>();

            foreach (var scheme in await schemes.GetRequestHandlerSchemesAsync())
            {
                // Check to see if this handler would apply and resolve tenant context if so.
                // Hanlders have a method, ShouldHandleAsync, which would be nice here, but it causes issues
                // with caching.
                // Workaround is to copy the logic from ShouldHandleAsync which requires instantiating options the hard way.

                var optionType = scheme.HandlerType.GetProperty("Options").PropertyType;
                var optionsFactoryType = typeof(IOptionsFactory<>).MakeGenericType(optionType);
                var optionsFactory = context.RequestServices.GetRequiredService(optionsFactoryType);
                var options = optionsFactoryType.GetMethod("Create").Invoke(optionsFactory, new[] { scheme.Name }) as RemoteAuthenticationOptions;

                if (options.CallbackPath == context.Request.Path)
                {
                    // Skip if this is not a compatible type of authentication.
                    if (!(options is OAuthOptions || options is OpenIdConnectOptions))
                    {
                        continue;
                    }

                    try
                    {
                        string state = null;

                        if (string.Equals(context.Request.Method, "GET", StringComparison.OrdinalIgnoreCase))
                        {
                            state = context.Request.Query["state"];
                        }
                        else if (string.Equals(context.Request.Method, "POST", StringComparison.OrdinalIgnoreCase)
                            && !string.IsNullOrEmpty(context.Request.ContentType)
                            && context.Request.ContentType.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase)
                            && context.Request.Body.CanRead)
                        {
                            var formOptions = new FormOptions { BufferBody = true };
                            var form = await context.Request.ReadFormAsync(formOptions);
                            state = form.Where(i => i.Key.ToLowerInvariant() == "state").Single().Value;
                        }

                        var oAuthOptions = options as OAuthOptions;
                        var openIdConnectOptions = options as OpenIdConnectOptions;

                        var properties = oAuthOptions?.StateDataFormat.Unprotect(state) ??
                                     openIdConnectOptions?.StateDataFormat.Unprotect(state);

                        if (properties.Items.Keys.Contains("tenantIdentifier"))
                        {
                            var tenantIdentifier = properties.Items["tenantIdentifier"];

                            var strategy = new StaticMultiTenantStrategy(tenantIdentifier);
                            var store = context.RequestServices.GetRequiredService<IMultiTenantStore>();
                            var resolver = new TenantResolver(store, strategy);

                            return await resolver.ResolveAsync(context);
                        }
                    }
                    catch (Exception e)
                    {
                        throw new MultiTenantException("Error occurred resolving tenant for remote authentication.", e);
                    }

                }
            }

            return null;
        }
    }
}