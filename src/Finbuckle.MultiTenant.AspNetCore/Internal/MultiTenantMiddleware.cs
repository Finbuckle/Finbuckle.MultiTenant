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
            if (!context.Items.ContainsKey(Constants.HttpContextMultiTenantContext))
            {
                context.Items.Add(Constants.HttpContextMultiTenantContext, null);

                // Try the registered strategy.
                var strategy = context.RequestServices.GetRequiredService<IMultiTenantStrategy>();
                var identifier = await strategy.GetIdentifierAsync(context).ConfigureAwait(false);
                var store = context.RequestServices.GetRequiredService<IMultiTenantStore>();

                TenantInfo tenantInfo = null;
                if (identifier != null)
                {
                    tenantInfo = await store.GetByIdentifierAsync(identifier);
                }

                // Resolve for remote authentication callbacks if applicable.
                if (tenantInfo == null &&
                    context.RequestServices.GetService<IAuthenticationSchemeProvider>() is MultiTenantAuthenticationSchemeProvider)
                {
                    strategy = (IMultiTenantStrategy)context.RequestServices.GetRequiredService<IRemoteAuthenticationStrategy>();
                    identifier = await strategy.GetIdentifierAsync(context).ConfigureAwait(false);

                    if (identifier != null)
                    {
                        tenantInfo = await store.GetByIdentifierAsync(identifier);
                    }
                }

                MultiTenantContext multiTenantContext = null;
                if(tenantInfo != null)
                {
                    multiTenantContext = new MultiTenantContext();

                    multiTenantContext.TenantInfo = tenantInfo;
                    tenantInfo.MultiTenantContext = multiTenantContext;

                    var storeInfo = new StoreInfo();
                    storeInfo.MultiTenantContext = multiTenantContext;
                    storeInfo.Store = store;
                    storeInfo.StoreType = store.GetType();
                    multiTenantContext.StoreInfo = storeInfo;

                    var strategyInfo = new StrategyInfo();
                    strategyInfo.MultiTenantContext = multiTenantContext;
                    strategyInfo.Strategy = strategy;
                    strategyInfo.StrategyType = strategy.GetType();
                    multiTenantContext.StrategyInfo = strategyInfo;
                }

                context.Items[Constants.HttpContextMultiTenantContext] = multiTenantContext;
            }

            if (next != null)
            {
                await next(context);
            }
        }
    }
}