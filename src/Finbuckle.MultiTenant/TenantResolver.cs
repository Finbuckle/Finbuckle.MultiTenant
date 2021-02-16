// Copyright 2018-2020 Finbuckle LLC, Andrew White, and Contributors
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.Stores;
using Finbuckle.MultiTenant.Strategies;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Finbuckle.MultiTenant
{
    public class TenantResolver<T> : ITenantResolver<T>, ITenantResolver
        where T : class, ITenantInfo, new()
    {
        private readonly IOptionsMonitor<MultiTenantOptions> options;
        private readonly ILoggerFactory loggerFactory;

        public TenantResolver(IEnumerable<IMultiTenantStrategy> strategies, IEnumerable<IMultiTenantStore<T>> stores, IOptionsMonitor<MultiTenantOptions> options) :
            this(strategies, stores, options, null)
        {
        }

        public TenantResolver(IEnumerable<IMultiTenantStrategy> strategies, IEnumerable<IMultiTenantStore<T>> stores, IOptionsMonitor<MultiTenantOptions> options, ILoggerFactory loggerFactory)
        {
            Stores = stores;
            this.options = options;
            this.loggerFactory = loggerFactory;

#if !NETSTANDARD2_0
            Strategies = strategies.OrderByDescending(s => s.Priority);
#else
            // Can't rely on Priority property so move RemoteAuth and Statics to end.
            var statics = strategies.Where(s => s.GetType() == typeof(StaticStrategy)).ToList();
            var remotes = strategies.Where(s => s.GetType().Name == "RemoteAuthenticationCallbackStrategy").ToList();
            var others = strategies.Where(s => !statics.Contains(s) && !remotes.Contains(s)).ToList();
            others.AddRange(remotes);
            others.AddRange(statics);
            Strategies = others;
#endif
        }

        public IEnumerable<IMultiTenantStrategy> Strategies { get; set; }
        public IEnumerable<IMultiTenantStore<T>> Stores { get; set; }

        public async Task<IMultiTenantContext<T>> ResolveAsync(object context)
        {
            IMultiTenantContext<T> result = null;

            foreach (var strategy in Strategies)
            {
                var _strategy = new MultiTenantStrategyWrapper(strategy, loggerFactory?.CreateLogger(strategy.GetType()));
                var identifier = await _strategy.GetIdentifierAsync(context);

                if (options.CurrentValue.IgnoredIdentifiers.Contains(identifier, StringComparer.OrdinalIgnoreCase))
                {
                    Utilities.TryLoginfo(loggerFactory?.CreateLogger(GetType()), $"Ignored identifier: {identifier}");
                    identifier = null;
                }

                if (identifier != null)
                {
                    foreach (var store in Stores)
                    {
                        var _store = new MultiTenantStoreWrapper<T>(store, loggerFactory?.CreateLogger(store.GetType()));
                        var tenantInfo = await _store.TryGetByIdentifierAsync(identifier);
                        if (tenantInfo != null)
                        {
                            result = new MultiTenantContext<T>();
                            result.StoreInfo = new StoreInfo<T> { Store = store, StoreType = store.GetType() };
                            result.StrategyInfo = new StrategyInfo { Strategy = strategy, StrategyType = strategy.GetType() };
                            result.TenantInfo = tenantInfo;

                            await options.CurrentValue?.Events?.OnTenantResolved(new TenantResolvedContext { Context = context, TenantInfo = tenantInfo });

                            break;
                        }
                    }

                    if (result != null)
                        break;

                    await options.CurrentValue?.Events?.OnTenantNotResolved(new TenantNotResolvedContext { Context = context, Identifier = identifier });
                }
            }

            return result;
        }

        async Task<object> ITenantResolver.ResolveAsync(object context)
        {
            var multiTenantContext = await ResolveAsync(context);
            return multiTenantContext;
        }
    }
}