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
using Finbuckle.MultiTenant.Strategies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Finbuckle.MultiTenant.AspNetCore
{
    /// <summary>
    /// Middleware for resolving the <c>TenantContext</c> and storing it in <c>HttpContext</c>.
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
            // Set the tenant context (or null) into the Items collections.
            if (!context.Items.ContainsKey(Constants.HttpContextTenantContext))
            {
                context.Items.Add(Constants.HttpContextTenantContext, null);

                // Try the registered strategy.
                var strategy = context.RequestServices.GetRequiredService<IMultiTenantStrategy>();
                var identifier = await strategy.GetIdentifierAsync(context).ConfigureAwait(false);
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
                    strategy = (IMultiTenantStrategy)context.RequestServices.GetRequiredService<IRemoteAuthenticationMultiTenantStrategy>();
                    identifier = await strategy.GetIdentifierAsync(context).ConfigureAwait(false);

                    if (identifier != null)
                    {
                        tenantContext = await store.GetByIdentifierAsync(identifier);
                    }
                }

                if(tenantContext != null)
                {
                    tenantContext.MultiTenantStrategyType = strategy.GetType();
                    tenantContext.MultiTenantStoreType = store.GetType();
                }

                context.Items[Constants.HttpContextTenantContext] = tenantContext;
            }

            if (next != null)
            {
                await next(context);
            }
        }
    }
}