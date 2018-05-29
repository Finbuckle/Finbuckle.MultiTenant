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
using Finbuckle.MultiTenant.Core.Abstractions;
using Microsoft.Extensions.Logging;

namespace Finbuckle.MultiTenant.Core
{
    /// <summary>
    /// Resolves the <c>TenantContext</c> from the configured <c>ITenantStore</c> using the configured <c>ITenantResolverStrategy</c>.
    /// </summary>
    public class TenantResolver
    {
        public readonly IMultiTenantStore multiTenantStore;
        private readonly IMultiTenantStrategy multiTenantStrategy;
        private readonly ILogger<TenantResolver> logger;

        public TenantResolver(IMultiTenantStore multiTenantStore, IMultiTenantStrategy multiTenantStrategy, ILogger<TenantResolver> logger = null)
        {
            this.multiTenantStore = multiTenantStore ??
                throw new MultiTenantException(null, new ArgumentNullException(nameof(multiTenantStore)));
            
            this.multiTenantStrategy = multiTenantStrategy ??
                throw new ArgumentNullException(nameof(TenantResolver.multiTenantStrategy));
            
            this.logger = logger;
        }

        /// <summary>
        /// Resolves the <c>TenantContext</c> from the configured <c>ITenantStore</c> using the configured <c>ITenantResolverStrategy</c>.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<TenantContext> ResolveAsync(object context)
        {
            Utilities.TryLogInfo(logger, $"Resolving tenant using \"{multiTenantStrategy.GetType()}\".");

            string identifier = multiTenantStrategy.GetIdentifier(context);

            Utilities.TryLogInfo(logger, $"Tenant identifier \"{identifier ?? "<null>"}\" detected.");

            if (string.IsNullOrWhiteSpace(identifier))
                return null;

            Utilities.TryLogInfo(logger, $"Retrieving TenantContext using \"{multiTenantStore.GetType()}\".");

            var storeResult = await multiTenantStore.GetByIdentifierAsync(identifier).ConfigureAwait(false);
            
            Utilities.TryLogInfo(logger, $"TenantContext for Tenant Id \"{storeResult?.Id ?? "<null>"}\" was retrieved.");

            if (storeResult == null)
                return null;

            var result = new TenantContext(storeResult);
            result.MultiTenantStrategyType = multiTenantStrategy.GetType();
            result.MultiTenantStoreType = multiTenantStore.GetType();

            return result;
        }
    }
}