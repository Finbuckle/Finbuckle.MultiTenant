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

using System.Linq;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.Stores;
using Finbuckle.MultiTenant.Strategies;
using Microsoft.AspNetCore.Authentication;
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
            // Set the initial multitenant context into the Items collections.
            if (!context.Items.ContainsKey(Constants.HttpContextMultiTenantContext))
            {
                var multiTenantContext = new MultiTenantContext();

                var store = context.RequestServices.GetRequiredService<IMultiTenantStore>();
                var storeInfo = new StoreInfo();
                storeInfo.MultiTenantContext = multiTenantContext;
                storeInfo.Store = store;
                if (store.GetType().IsGenericType &&
                    store.GetType().GetGenericTypeDefinition() == typeof(MultiTenantStoreWrapper<>))
                {
                    storeInfo.StoreType = store.GetType().GetGenericArguments().First();
                }
                else
                {
                    storeInfo.StoreType = store.GetType();
                }
                multiTenantContext.StoreInfo = storeInfo;

                var strategy = context.RequestServices.GetRequiredService<IMultiTenantStrategy>();
                var strategyInfo = new StrategyInfo();
                strategyInfo.MultiTenantContext = multiTenantContext;
                strategyInfo.Strategy = strategy;
                if (strategy.GetType().IsGenericType &&
                    strategy.GetType().GetGenericTypeDefinition() == typeof(MultiTenantStrategyWrapper<>))
                {
                    strategyInfo.StrategyType = strategy.GetType().GetGenericArguments().First();
                }
                else
                {
                    strategyInfo.StrategyType = strategy.GetType();
                }
                multiTenantContext.StrategyInfo = strategyInfo;

                context.Items.Add(Constants.HttpContextMultiTenantContext, multiTenantContext);

                // Try the registered strategy.
                var identifier = await strategy.GetIdentifierAsync(context).ConfigureAwait(false);

                TenantInfo tenantInfo = null;
                if (identifier != null)
                {
                    tenantInfo = await store.TryGetByIdentifierAsync(identifier);
                }

                // Resolve for remote authentication callbacks if applicable.
                if (tenantInfo == null &&
                    context.RequestServices.GetService<IAuthenticationSchemeProvider>() is MultiTenantAuthenticationSchemeProvider)
                {
                    strategy = (IMultiTenantStrategy)context.RequestServices.GetRequiredService<IRemoteAuthenticationStrategy>();

                    // Adjust the strategy info in the multitenant context.
                    strategyInfo.Strategy = strategy;
                    strategyInfo.StrategyType = strategy.GetType();

                    identifier = await strategy.GetIdentifierAsync(context).ConfigureAwait(false);

                    if (identifier != null)
                    {
                        tenantInfo = await store.TryGetByIdentifierAsync(identifier);
                    }
                }

                // Set the tenant info in the multitenant context if applicable.
                if (tenantInfo != null)
                {
                    multiTenantContext.TenantInfo = tenantInfo;
                    tenantInfo.MultiTenantContext = multiTenantContext;
                }
            }

            if (next != null)
            {
                await next(context);
            }
        }
    }
}