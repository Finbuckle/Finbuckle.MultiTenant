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
    public static class FinbuckeMultiTenantBuilderExtensions
    {
        /// <summary>
        /// Adds an empty, case-insensitive InMemoryStore to the application.
        /// </summary>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder WithInMemoryStore(this FinbuckleMultiTenantBuilder builder)
            => builder.WithInMemoryStore(true);

        /// <summary>
        /// Adds an empty InMemoryStore to the application.
        /// </summary>
        /// <param name="ignoreCase">Whether the store should ignore case.</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder WithInMemoryStore(this FinbuckleMultiTenantBuilder builder,
                                                                    bool ignoreCase)
            => builder.WithInMemoryStore(_ => { }, ignoreCase);

        /// <summary>
        /// Adds and configures a case-insensitive InMemoryStore to the application using the provided ConfigurationSeciont.
        /// </summary>
        /// <param name="config">The ConfigurationSection which contains the InMemoryStore configuartion settings.</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder WithInMemoryStore(this FinbuckleMultiTenantBuilder builder,
                                                                    IConfigurationSection configurationSection)
            => builder.WithInMemoryStore(o => configurationSection.Bind(o), true);

        /// <summary>
        /// Adds and configures InMemoryStore to the application using the provided ConfigurationSeciont.
        /// </summary>
        /// <param name="config">The ConfigurationSection which contains the InMemoryStore configuartion settings.</param>
        /// <param name="ignoreCase">Whether the store should ignore case.</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder WithInMemoryStore(this FinbuckleMultiTenantBuilder builder,
                                                                    IConfigurationSection configurationSection,
                                                                    bool ignoreCase)
            => builder.WithInMemoryStore(o => configurationSection.Bind(o), ignoreCase);

        /// <summary>
        /// Adds and configures a case-insensitive InMemoryStore to the application using the provided action.
        /// </summary>
        /// <param name="config">A delegate or lambda for configuring the tenant.</param>
        /// <param name="ignoreCase">Whether the store should ignore case.</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder WithInMemoryStore(this FinbuckleMultiTenantBuilder builder,
                                                                    Action<InMemoryStoreOptions> config)
            => builder.WithInMemoryStore(config, true);

        /// <summary>
        /// Adds and configures InMemoryStore to the application using the provided action.
        /// </summary>
        /// <param name="config">A delegate or lambda for configuring the tenant.</param>
        /// <param name="ignoreCase">Whether the store should ignore case.</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder WithInMemoryStore(this FinbuckleMultiTenantBuilder builder,
                                                                    Action<InMemoryStoreOptions> config,
                                                                    bool ignoreCase)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            return builder.WithStore<InMemoryStore>(ServiceLifetime.Singleton, sp => InMemoryStoreFactory(config, ignoreCase));
        }

        /// <summary>
        /// Creates an InMemoryStore from configured InMemoryMultiTenantStoreOptions.
        /// </summary>
        private static InMemoryStore InMemoryStoreFactory(Action<InMemoryStoreOptions> config, bool ignoreCase)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            var options = new InMemoryStoreOptions();
            config(options);
            var store = new InMemoryStore(ignoreCase);

            try
            {
                foreach (var tenantConfig in options.TenantConfigurations ?? new InMemoryStoreOptions.TenantConfiguration[0])
                {
                    if (string.IsNullOrWhiteSpace(tenantConfig.Id) ||
                        string.IsNullOrWhiteSpace(tenantConfig.Identifier))
                        throw new MultiTenantException("Tenant Id and Identifer cannot be null or whitespace.");

                    var tenantInfo = new TenantInfo(tenantConfig.Id,
                                               tenantConfig.Identifier,
                                               tenantConfig.Name,
                                               tenantConfig.ConnectionString ?? options.DefaultConnectionString,
                                               null);

                    foreach (var item in tenantConfig.Items ?? new Dictionary<string, string>())
                    {
                        tenantInfo.Items.Add(item.Key, item.Value);
                    }

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
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder WithStaticStrategy(this FinbuckleMultiTenantBuilder builder,
                                                                     string identifier)
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
        public static FinbuckleMultiTenantBuilder WithDelegateStrategy(this FinbuckleMultiTenantBuilder builder,
                                                                       Func<object, Task<string>> doStrategy)
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
        public static FinbuckleMultiTenantBuilder WithFallbackStrategy(this FinbuckleMultiTenantBuilder builder,
                                                                       string identifier)
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