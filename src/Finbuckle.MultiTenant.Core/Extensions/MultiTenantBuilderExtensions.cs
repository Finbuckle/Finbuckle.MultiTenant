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
using System.Threading.Tasks;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Options;
using Finbuckle.MultiTenant.Stores;
using Finbuckle.MultiTenant.Strategies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provices builder methods for Finbuckle.MultiTenant services and configuration.
    /// </summary>

    public static class FinbuckleMultiTenantBuilderExtensions
    {
        /// <summary>
        /// Adds a HttpRemoteSTore to the application.
        /// </summary>
        /// <param name="endpointTemplate">The endpoint URI template.</param>
        /// <param name="clientConfig">An action to configure the underlying HttpClient.</param>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithHttpRemoteStore<TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder,
                                                                      string endpointTemplate) where TTenantInfo : class, ITenantInfo, new()
        => builder.WithHttpRemoteStore(endpointTemplate, null);

        /// <summary>
        /// Adds a HttpRemoteSTore to the application.
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
        /// Adds an empty, case-insensitive InMemoryStore to the application.
        /// </summary>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithInMemoryStore<TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder)
            where TTenantInfo : class, ITenantInfo, new()
            => builder.WithInMemoryStore<TTenantInfo>(true);

        /// <summary>
        /// Adds an empty InMemoryStore to the application.
        /// </summary>
        /// <param name="ignoreCase">Whether the store should ignore case.</param>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithInMemoryStore<TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder,
                                                                    bool ignoreCase)
            where TTenantInfo : class, ITenantInfo, new()
            => builder.WithInMemoryStore<TTenantInfo>(_ => { }, ignoreCase);

        /// <summary>
        /// Adds and configures a case-insensitive InMemoryStore to the application using the provided ConfigurationSeciont.
        /// </summary>
        /// <param name="config">The ConfigurationSection which contains the InMemoryStore configuartion settings.</param>
        [Obsolete("Consider using ConfigurationStore instead.")]
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithInMemoryStore<TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder,
                                                                    IConfigurationSection configurationSection)
                where TTenantInfo : class, ITenantInfo, new()
            => builder.WithInMemoryStore<TTenantInfo>(o => configurationSection.Bind(o), true);

        /// <summary>
        /// Adds and configures InMemoryStore to the application using the provided ConfigurationSeciont.
        /// </summary>
        /// <param name="config">The ConfigurationSection which contains the InMemoryStore configuartion settings.</param>
        /// <param name="ignoreCase">Whether the store should ignore case.</param>
        [Obsolete("Consider using ConfigurationStore instead.")]
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithInMemoryStore<TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder,
                                                                    IConfigurationSection configurationSection,
                                                                    bool ignoreCase)
                where TTenantInfo : class, ITenantInfo, new()
            => builder.WithInMemoryStore<TTenantInfo>(o => configurationSection.Bind(o), ignoreCase);

        /// <summary>
        /// Adds and configures a case-insensitive InMemoryStore to the application using the provided action.
        /// </summary>
        /// <param name="config">A delegate or lambda for configuring the tenant.</param>
        /// <param name="ignoreCase">Whether the store should ignore case.</param>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithInMemoryStore<TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder,
                                                                               Action<InMemoryStoreOptions> config)
            where TTenantInfo : class, ITenantInfo, new()
            => builder.WithInMemoryStore<TTenantInfo>(config, true);

        /// <summary>
        /// Adds and configures InMemoryStore to the application using the provided action.
        /// </summary>
        /// <param name="config">A delegate or lambda for configuring the tenant.</param>
        /// <param name="ignoreCase">Whether the store should ignore case.</param>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithInMemoryStore<TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder,
                                                                    Action<InMemoryStoreOptions> config,
                                                                    bool ignoreCase)
            where TTenantInfo : class, ITenantInfo, new()
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            return builder.WithStore<InMemoryStore<TTenantInfo>>(ServiceLifetime.Singleton, sp => InMemoryStoreFactory<TTenantInfo>(config, ignoreCase));
        }

        //TODO: Clean up any "Configuration" stuff here once it is no longer supported
        private static InMemoryStore<TTenantInfo> InMemoryStoreFactory<TTenantInfo>(Action<InMemoryStoreOptions> config, bool ignoreCase)
            where TTenantInfo : class, ITenantInfo, new()
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            var options = new InMemoryStoreOptions();
            config(options);
            var store = new InMemoryStore<TTenantInfo>(ignoreCase);

            try
            {
                foreach (var tenantConfig in options.TenantConfigurations ?? new InMemoryStoreOptions.TenantConfiguration[0])
                {
                    if (string.IsNullOrWhiteSpace(tenantConfig.Id) ||
                        string.IsNullOrWhiteSpace(tenantConfig.Identifier))
                        throw new MultiTenantException("Tenant Id and Identifer cannot be null or whitespace.");

                    var tenantInfo = new TTenantInfo
                    {
                        Id = tenantConfig.Id,
                        Identifier = tenantConfig.Identifier,
                        Name = tenantConfig.Name,
                        ConnectionString = tenantConfig.ConnectionString ?? options.DefaultConnectionString
                    };

                    // foreach (var item in tenantConfig.Items ?? new Dictionary<string, string>())
                    // {
                    //     tenantInfo.Items.Add(item.Key, item.Value);
                    // }

                    if (!store.TryAddAsync(tenantInfo).Result)
                        throw new MultiTenantException($"Unable to add {tenantInfo.Identifier} because it is already present.");
                }
            }
            catch (Exception e)
            {
                throw new MultiTenantException("Unable to create ImMemoryStore from configuration.", e);
            }

            return store;
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

        /// <summary>
        /// Adds and configures a fallback strategy for if the main strategy or remote authentication
        /// fail to resolve a tenant.
        /// </summary>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithFallbackStrategy<TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder,
                                                                       string identifier)
            where TTenantInfo : class, ITenantInfo, new()
        {
            if (identifier == null)
            {
                throw new ArgumentNullException(nameof(identifier));
            }

            builder.Services.TryAddSingleton<FallbackStrategy>(sp => new FallbackStrategy(identifier));

            return builder;
        }
    }
}