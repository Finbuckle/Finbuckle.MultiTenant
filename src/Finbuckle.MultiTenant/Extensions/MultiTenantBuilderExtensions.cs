//    Copyright 2018-2020 Finbuckle LLC, Andrew White, and Contributors
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
using System.Threading.Tasks;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Internal;
using Finbuckle.MultiTenant.Options;
using Finbuckle.MultiTenant.Stores;
using Finbuckle.MultiTenant.Strategies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provides builder methods for Finbuckle.MultiTenant services and configuration.
    /// </summary>

    public static class FinbuckleMultiTenantBuilderExtensions
    {
        /// <summary>
        /// Adds a DistributedCacheStore to the application.
        /// </summary>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithDistributedCacheStore<TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder)
            where TTenantInfo : class, ITenantInfo, new()   
            => builder.WithDistributedCacheStore(TimeSpan.MaxValue);
        

        /// <summary>
        /// Adds a DistributedCacheStore to the application.
        /// </summary>
        /// <param name="slidingExiration">The timespan for a cache entry's sliding expiration.</param>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithDistributedCacheStore<TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder, TimeSpan? slidingExpiration)
            where TTenantInfo : class, ITenantInfo, new()
        {
            return builder.WithStore<DistributedCacheStore<TTenantInfo>>(ServiceLifetime.Transient, Constants.TenantToken, slidingExpiration);
        }

        /// <summary>
        /// Adds a HttpRemoteStore to the application.
        /// </summary>
        /// <param name="endpointTemplate">The endpoint URI template.</param>
        /// <param name="clientConfig">An action to configure the underlying HttpClient.</param>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithHttpRemoteStore<TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder, string endpointTemplate)
            where TTenantInfo : class, ITenantInfo, new()
            => builder.WithHttpRemoteStore(endpointTemplate, null);

        /// <summary>
        /// Adds a HttpRemoteStore to the application.
        /// </summary>
        /// <param name="endpointTemplate">The endpoint URI template.</param>
        /// <param name="clientConfig">An action to configure the underlying HttpClient.</param>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithHttpRemoteStore<TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder,
                                                                                                string endpointTemplate,
                                                                                                Action<IHttpClientBuilder> clientConfig) where TTenantInfo : class, ITenantInfo, new()
        {
            var httpClientBuilder = builder.Services.AddHttpClient(typeof(HttpRemoteStoreClient<TTenantInfo>).FullName);
            if (clientConfig != null)
                clientConfig(httpClientBuilder);

            builder.Services.TryAddSingleton<HttpRemoteStoreClient<TTenantInfo>>();

            return builder.WithStore<HttpRemoteStore<TTenantInfo>>(ServiceLifetime.Singleton, endpointTemplate);
        }

        /// <summary>
        /// Adds a ConfigurationStore to the application. Uses the default IConfiguration and section "Finbuckle:MultiTenant:Stores:ConfigurationStore".
        /// </summary>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithConfigurationStore<TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder)
            where TTenantInfo : class, ITenantInfo, new()
            => builder.WithStore<ConfigurationStore<TTenantInfo>>(ServiceLifetime.Singleton);

        /// <summary>
        /// Adds a ConfigurationStore to the application.
        /// </summary>
        /// <param name="configuration">The IConfiguration to load the section from.</param>
        /// <param name="sectionName">The configuration section to load.</param>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithConfigurationStore<TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder,
                                                                                                   IConfiguration configuration,
                                                                                                   string sectionName)
                where TTenantInfo : class, ITenantInfo, new()
            => builder.WithStore<ConfigurationStore<TTenantInfo>>(ServiceLifetime.Singleton, configuration, sectionName);

        /// <summary>
        /// Adds an empty InMemoryStore to the application.
        /// </summary>
        /// <param name="ignoreCase">Whether the store should ignore case.</param>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithInMemoryStore<TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder)
            where TTenantInfo : class, ITenantInfo, new()
            => builder.WithInMemoryStore<TTenantInfo>(_ => {});

        /// <summary>
        /// Adds and configures InMemoryStore to the application using the provided action.
        /// </summary>
        /// <param name="config">A action for configuring the store.</param>
        /// <param name="ignoreCase">Whether the store should ignore case.</param>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithInMemoryStore<TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder,
                                                                                              Action<InMemoryStoreOptions<TTenantInfo>> config)
            where TTenantInfo : class, ITenantInfo, new()
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            builder.Services.Configure<InMemoryStoreOptions<TTenantInfo>>(config);

            return builder.WithStore<InMemoryStore<TTenantInfo>>(ServiceLifetime.Singleton);
        }

        /// <summary>
        /// Adds and configures a StaticStrategy to the application.
        /// </summary>
        /// <param name="identifier">The tenant identifier to use for all tenant resolution.</param>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithStaticStrategy<TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder,
                                                                                               string identifier)
            where TTenantInfo : class, ITenantInfo, new()
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                throw new ArgumentException("Invalid value for \"identifier\"", nameof(identifier));
            }

            return builder.WithStrategy<StaticStrategy>(ServiceLifetime.Singleton, new object[] { identifier }); ;
        }

        /// <summary>
        /// Adds and configures a DelegateStrategy to the application.
        /// </summary>
        /// <param name="doStrategy">The delegate implementing the strategy.</returns>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithDelegateStrategy<TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder,
                                                                                                 Func<object, Task<string>> doStrategy)
            where TTenantInfo : class, ITenantInfo, new()
        {
            if (doStrategy == null)
            {
                throw new ArgumentNullException(nameof(doStrategy));
            }

            return builder.WithStrategy<DelegateStrategy>(ServiceLifetime.Singleton, new object[] { doStrategy });
        }
    }
}