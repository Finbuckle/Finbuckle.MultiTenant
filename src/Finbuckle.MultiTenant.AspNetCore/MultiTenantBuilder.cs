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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication;
using Finbuckle.MultiTenant.AspNetCore;
using Finbuckle.MultiTenant.Strategies;
using Finbuckle.MultiTenant.Stores;
using Microsoft.AspNetCore.Routing;
using Finbuckle.MultiTenant;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provices builder methods for Finbuckle.MultiTenant services and configuration.
    /// </summary>
    public class FinbuckeMultiTenantBuilder
    {
        internal readonly IServiceCollection services;

        public FinbuckeMultiTenantBuilder(IServiceCollection services)
        {
            this.services = services;
        }
        
        /// <summary>
        /// Adds per-tenant configuration for an options class.
        /// </summary>
        /// <param name="tenantInfo">The configuration action to be run for each tenant.</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public FinbuckeMultiTenantBuilder WithPerTenantOptions<TOptions>(Action<TOptions, TenantInfo> tenantInfo) where TOptions : class, new()
        {
            if (tenantInfo == null)
            {
                throw new ArgumentNullException(nameof(tenantInfo));
            }

            // Handles multiplexing cached options.
            services.TryAddSingleton<IOptionsMonitorCache<TOptions>>(sp =>
                {
                    return (MultiTenantOptionsCache<TOptions>)
                        ActivatorUtilities.CreateInstance(sp, typeof(MultiTenantOptionsCache<TOptions>));
                });

            // Necessary to apply tenant options in between configuration and postconfiguration
            services.TryAddTransient<IOptionsFactory<TOptions>>(sp =>
                {
                    return (IOptionsFactory<TOptions>)ActivatorUtilities.
                        CreateInstance(sp, typeof(MultiTenantOptionsFactory<TOptions>), new[] { tenantInfo });
                });

            services.TryAddScoped<IOptionsSnapshot<TOptions>>(sp => BuildOptionsManager<TOptions>(sp));

            services.TryAddSingleton<IOptions<TOptions>>(sp => BuildOptionsManager<TOptions>(sp));

            return this;
        }

        private static MultiTenantOptionsManager<TOptions> BuildOptionsManager<TOptions>(IServiceProvider sp) where TOptions : class, new()
        {
            var cache = ActivatorUtilities.CreateInstance(sp, typeof(MultiTenantOptionsCache<TOptions>));
            return (MultiTenantOptionsManager<TOptions>)
                ActivatorUtilities.CreateInstance(sp, typeof(MultiTenantOptionsManager<TOptions>), new[] { cache });
        }

        /// <summary>
        /// Configures support for multitenant OAuth and OpenIdConnect.
        /// </summary>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public FinbuckeMultiTenantBuilder WithRemoteAuthentication()
        {
            // Replace needed instead of TryAdd...
            services.Replace(ServiceDescriptor.Singleton<IAuthenticationSchemeProvider, MultiTenantAuthenticationSchemeProvider>());
            services.Replace(ServiceDescriptor.Scoped<IAuthenticationService, MultiTenantAuthenticationService>());

            services.TryAddSingleton<IRemoteAuthenticationStrategy, RemoteAuthenticationStrategy>();

            return this;
        }

        /// <summary>
        /// Adds and configures a IMultiTenantStrategy to the application using using default dependency injection.
        /// </summary>>
        /// <param name="lifetime">The service lifetime.</param>
        /// <param name="parameters">a paramter list for any constructor paramaters not covered by dependency injection.</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public FinbuckeMultiTenantBuilder WithStore<T>(ServiceLifetime lifetime, params object[] parameters) where T : IMultiTenantStore
            => WithStore(lifetime, sp => ActivatorUtilities.CreateInstance<T>(sp, parameters));

        /// <summary>
        /// Adds and configures a IMultiTenantStrategy to the application using a factory method.
        /// </summary>
        /// <param name="lifetime">The service lifetime.</param>
        /// <param name="factory">A delegate that will create and configure the strategy.</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public FinbuckeMultiTenantBuilder WithStore(ServiceLifetime lifetime, Func<IServiceProvider, IMultiTenantStore> factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            services.TryAdd(ServiceDescriptor.Describe(typeof(IMultiTenantStore), factory, lifetime));

            return this;
        }

        /// <summary>
        /// Adds an EFCore based multitenant store to the application. Will also add the database context service unless it is already added.
        /// </summary>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public FinbuckeMultiTenantBuilder WithEFCoreStore<TEFCoreStoreDbContext, TTenantInfo>()
            where TEFCoreStoreDbContext : EFCoreStoreDbContext<TTenantInfo>
            where TTenantInfo : class, IEFCoreStoreTenantInfo, new()
        {
            this.services.AddDbContext<TEFCoreStoreDbContext>(); // Note, will not override existing context if already added.
            return WithStore<EFCoreStore<TEFCoreStoreDbContext, TTenantInfo>>(ServiceLifetime.Scoped);
        }

        /// <summary>
        /// Adds an empty, case-insensitive InMemoryStore to the application.
        /// </summary>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public FinbuckeMultiTenantBuilder WithInMemoryStore() => WithInMemoryStore(true);

        /// <summary>
        /// Adds an empty InMemoryStore to the application.
        /// </summary>
        /// <param name="ignoreCase">Whether the store should ignore case.</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public FinbuckeMultiTenantBuilder WithInMemoryStore(bool ignoreCase) => WithInMemoryStore(_ => { }, ignoreCase);

        /// <summary>
        /// Adds and configures a case-insensitive InMemoryStore to the application using the provided ConfigurationSeciont.
        /// </summary>
        /// <param name="config">The ConfigurationSection which contains the InMemoryStore configuartion settings.</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public FinbuckeMultiTenantBuilder WithInMemoryStore(IConfigurationSection configurationSection) =>
            WithInMemoryStore(o => configurationSection.Bind(o), true);

        /// <summary>
        /// Adds and configures InMemoryStore to the application using the provided ConfigurationSeciont.
        /// </summary>
        /// <param name="config">The ConfigurationSection which contains the InMemoryStore configuartion settings.</param>
        /// <param name="ignoreCase">Whether the store should ignore case.</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public FinbuckeMultiTenantBuilder WithInMemoryStore(IConfigurationSection configurationSection, bool ignoreCase) =>
            WithInMemoryStore(o => configurationSection.Bind(o), ignoreCase);

        /// <summary>
        /// Adds and configures a case-insensitive InMemoryStore to the application using the provided action.
        /// </summary>
        /// <param name="config">A delegate or lambda for configuring the tenant.</param>
        /// <param name="ignoreCase">Whether the store should ignore case.</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public FinbuckeMultiTenantBuilder WithInMemoryStore(Action<InMemoryStoreOptions> config)
            => WithInMemoryStore(config, true);

        /// <summary>
        /// Adds and configures InMemoryStore to the application using the provided action.
        /// </summary>
        /// <param name="config">A delegate or lambda for configuring the tenant.</param>
        /// <param name="ignoreCase">Whether the store should ignore case.</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public FinbuckeMultiTenantBuilder WithInMemoryStore(Action<InMemoryStoreOptions> config, bool ignoreCase)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            return WithStore(ServiceLifetime.Singleton, sp => InMemoryStoreFactory(config, ignoreCase, sp.GetService<ILogger<MultiTenantStoreWrapper<InMemoryStore>>>()));
        }

        /// <summary>
        /// Creates an InMemoryStore from configured InMemoryMultiTenantStoreOptions.
        /// </summary>
        private IMultiTenantStore InMemoryStoreFactory(Action<InMemoryStoreOptions> config, bool ignoreCase, ILogger<MultiTenantStoreWrapper<InMemoryStore>> logger)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            var options = new InMemoryStoreOptions();
            config(options);
            var store = new MultiTenantStoreWrapper<InMemoryStore>(new InMemoryStore(ignoreCase), logger);

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
                        throw new MultiTenantException($"Unable to add {tenantInfo.Identifier} is already configured.");
                }
            }
            catch (Exception e)
            {
                throw new MultiTenantException
                    ("Unable to add tenant to store.", e);
            }

            return store;
        }

        /// <summary>
        /// Adds and configures a StaticMultiTenantStrategy to the application.
        /// </summary>
        /// <param name="identifier">The tenant identifier to use for all tenant resolution.</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public FinbuckeMultiTenantBuilder WithStaticStrategy(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                throw new ArgumentException("Invalid value for \"identifier\"", nameof(identifier));
            }

            return WithStrategy(ServiceLifetime.Singleton, sp => new StaticStrategy(identifier, sp.GetService<ILogger<StaticStrategy>>()));
        }

        /// <summary>
        /// Adds and configures a BasePathMultiTenantStrategy to the application.
        /// </summary>
        /// <returnsThe same MultiTenantBuilder passed into the method.></returns>
        public FinbuckeMultiTenantBuilder WithBasePathStrategy()
            => WithStrategy(ServiceLifetime.Singleton, sp => new BasePathStrategy(sp.GetService<ILogger<BasePathStrategy>>()));

        /// <summary>
        /// Adds and configures a RouteMultiTenantStrategy with a route parameter "\_\_tenant\_\_" to the application.
        /// </summary>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public FinbuckeMultiTenantBuilder WithRouteStrategy(Action<IRouteBuilder> configRoutes)
            => WithRouteStrategy("__tenant__", configRoutes);

        /// <summary>
        /// Adds and configures a RouteMultiTenantStrategy to the application.
        /// </summary>
        /// <param name="tenantParam">The name of the route parameter used to determine the tenant identifier.</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public FinbuckeMultiTenantBuilder WithRouteStrategy(string tenantParam, Action<IRouteBuilder> configRoutes)
        {
            if (string.IsNullOrWhiteSpace(tenantParam))
            {
                throw new ArgumentException("Invalud value for \"tenantParam\"", nameof(tenantParam));
            }

            if (configRoutes == null)
            {
                throw new ArgumentNullException(nameof(configRoutes));
            }

            return WithStrategy(ServiceLifetime.Singleton, sp => new RouteStrategy(tenantParam, configRoutes, sp.GetService<ILogger<RouteStrategy>>()));
        }

        /// <summary>
        /// Adds and configures a HostMultiTenantStrategy with template "\_\_tenant\_\_.*" to the application.
        /// </summary>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public FinbuckeMultiTenantBuilder WithHostStrategy()
            => WithHostStrategy("__tenant__.*");

        /// <summary>
        /// Adds and configures a HostMultiTenantStrategy to the application.
        /// </summary>
        /// <param name="template">The template for determining the tenant identifier in the host.</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public FinbuckeMultiTenantBuilder WithHostStrategy(string template)
        {
            if (string.IsNullOrWhiteSpace(template))
            {
                throw new ArgumentException("Invalid value for \"template\"", nameof(template));
            }

            return WithStrategy(ServiceLifetime.Singleton, sp => new HostStrategy(template, sp.GetService<ILogger<HostStrategy>>()));
        }

        /// <summary>
        /// Adds and configures a IMultiTenantStrategy to the applicationusing default dependency injection.
        /// </summary>
        /// <param name="lifetime">The service lifetime.</param>
        /// <param name="parameters">a paramter list for any constructor paramaters not covered by dependency injection.</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public FinbuckeMultiTenantBuilder WithStrategy<T>(ServiceLifetime lifetime, params object[] parameters) where T : IMultiTenantStrategy
            => WithStrategy(lifetime, sp => ActivatorUtilities.CreateInstance<T>(sp, parameters));

        /// <summary>
        /// Adds and configures a IMultiTenantStrategy to the application using a factory method.
        /// </summary>
        /// <param name="lifetime">The service lifetime.</param>
        /// <param name="factory">A delegate that will create and configure the strategy.</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public FinbuckeMultiTenantBuilder WithStrategy(ServiceLifetime lifetime, Func<IServiceProvider, IMultiTenantStrategy> factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            services.TryAdd(ServiceDescriptor.Describe(typeof(IMultiTenantStrategy), factory, lifetime));
            
            return this;
        }
    }
}