// Copyright 2018-2020 Andrew White
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

using System.Collections.Generic;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.Stores;
using Finbuckle.MultiTenant.Strategies;
using Microsoft.Extensions.Logging;

namespace Finbuckle.MultiTenant
{
    public class TenantResolver<TTenantInfo> : ITenantResolver<TTenantInfo>
        where TTenantInfo : class, ITenantInfo, new()
    {
        private readonly ILoggerFactory loggerFactory;

        public TenantResolver(IEnumerable<IMultiTenantStrategy> strategies, IEnumerable<IMultiTenantStore<TTenantInfo>> stores) :
            this(strategies, stores, null)
        {
        }

        public TenantResolver(IEnumerable<IMultiTenantStrategy> strategies, IEnumerable<IMultiTenantStore<TTenantInfo>> stores, ILoggerFactory loggerFactory)
        {
            Strategies = strategies;
            Stores = stores;
            this.loggerFactory = loggerFactory;
        }

        public IEnumerable<IMultiTenantStrategy> Strategies { get; set; }
        public IEnumerable<IMultiTenantStore<TTenantInfo>> Stores { get; set; }

        public async Task<IMultiTenantContext<TTenantInfo>> ResolveAsync(object context)
        {
            IMultiTenantContext<TTenantInfo> result = null;

            foreach (var strategy in Strategies)
            {
                var _strategy = new MultiTenantStrategyWrapper(strategy, loggerFactory?.CreateLogger(strategy.GetType()));
                var identifier = await _strategy.GetIdentifierAsync(context);

                if (identifier != null)
                {
                    foreach (var store in Stores)
                    {
                        var _store = new MultiTenantStoreWrapper<TTenantInfo>(store, loggerFactory?.CreateLogger(store.GetType()));
                        var tenantInfo = await _store.TryGetByIdentifierAsync(identifier);
                        if (tenantInfo != null)
                        {
                            result = new MultiTenantContext<TTenantInfo>();
                            result.StoreInfo = new StoreInfo<TTenantInfo> { Store = store, StoreType = store.GetType() };
                            result.StrategyInfo = new StrategyInfo { Strategy = strategy, StrategyType = strategy.GetType() };
                            result.TenantInfo = tenantInfo;

                            break;
                        }
                    }

                    if (result != null)
                        break;
                }
            }

            return result;
        }
    }
}