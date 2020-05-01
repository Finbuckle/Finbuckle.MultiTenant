﻿//    Copyright 2020 Andrew White
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
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Finbuckle.MultiTenant.AspNetCore
{
    /// <summary>
    /// Middleware for resolving the TenantContext and storing it in HttpContext.
    /// </summary>
    internal class MultiTenantMiddleware<TTenantInfo>
        where TTenantInfo : class, ITenantInfo, new()
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
                var multiTenantContext = new MultiTenantContext<TTenantInfo>();
                context.Items.Add(Constants.HttpContextMultiTenantContext, multiTenantContext);

                IMultiTenantStrategy strategy = null;
                string identifier = null;

                // Cycle through the strategies
                foreach(var strat in context.RequestServices.GetServices<IMultiTenantStrategy>())
                {
                     // Try the registered strategy.
                        identifier = await strat.GetIdentifierAsync(context);
                        if(identifier != null)
                        {
                            strategy = strat;
                            break;
                        }
                }
               
                var store = context.RequestServices.GetRequiredService<IMultiTenantStore<TTenantInfo>>();
                TTenantInfo tenantInfo = null;
                if (identifier != null)
                {
                    SetStrategyInfo(multiTenantContext, strategy);
                    tenantInfo = await store.TryGetByIdentifierAsync(identifier);
                }

                // Resolve for remote authentication callbacks if applicable.
                if (tenantInfo == null)
                {
                    strategy = context.RequestServices.GetService<RemoteAuthenticationStrategy>();

                    if (strategy != null)
                    {
                        identifier = await strategy.GetIdentifierAsync(context);
                        if (identifier != null)
                        {
                            SetStrategyInfo(multiTenantContext, strategy);
                            tenantInfo = await store.TryGetByIdentifierAsync(identifier);
                        }
                    }
                }

                // Finally try the fallback identifier, if applicable.
                if (tenantInfo == null)
                {
                    strategy = context.RequestServices.GetService<FallbackStrategy>();
                    if (strategy != null)
                    {
                        identifier = await strategy.GetIdentifierAsync(context);
                        SetStrategyInfo(multiTenantContext, strategy);
                        tenantInfo = await store.TryGetByIdentifierAsync(identifier);
                    }
                }

                if (tenantInfo != null)
                {
                    // Set the tenant info.
                    multiTenantContext.TenantInfo = tenantInfo;

                    // Set the store info.
                    var storeInfo = new StoreInfo<TTenantInfo>();
                    storeInfo.Store = store;
                    if (store.GetType().IsGenericType &&
                        store.GetType().GetGenericTypeDefinition() == typeof(MultiTenantStoreWrapper<,>))
                    {
                        storeInfo.Store = (IMultiTenantStore<TTenantInfo>)store.GetType().GetProperty("Store").GetValue(store);
                        storeInfo.StoreType = store.GetType().GetGenericArguments().First();
                    }
                    else
                    {
                        storeInfo.Store = store;
                        storeInfo.StoreType = store.GetType();
                    }
                    multiTenantContext.StoreInfo = storeInfo;
                }
            }

            if (next != null)
            {
                await next(context);
            }
        }

        private static void SetStrategyInfo(MultiTenantContext<TTenantInfo> multiTenantContext, IMultiTenantStrategy strategy)
        {
            var strategyInfo = new StrategyInfo();
            strategyInfo.Strategy = strategy;
            if (strategy.GetType().IsGenericType &&
                strategy.GetType().GetGenericTypeDefinition() == typeof(MultiTenantStrategyWrapper<>))
            {
                strategyInfo.Strategy = (IMultiTenantStrategy)strategy.GetType().GetProperty("Strategy").GetValue(strategy);
                strategyInfo.StrategyType = strategy.GetType().GetGenericArguments().First();
            }
            else
            {
                strategyInfo.Strategy = strategy;
                strategyInfo.StrategyType = strategy.GetType();
            }
            multiTenantContext.StrategyInfo = strategyInfo;
        }
    }
}