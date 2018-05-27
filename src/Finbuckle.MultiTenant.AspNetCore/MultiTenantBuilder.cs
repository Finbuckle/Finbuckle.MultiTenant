using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Finbuckle.MultiTenant.Core;
using Finbuckle.MultiTenant.Core.Abstractions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Finbuckle.MultiTenant.AspNetCore
{
    /// <summary>
    /// Configures <c>Finbuckle.MultiTenant.AspNetCore</c> services and configuration.
    /// </summary>
    public class MultiTenantBuilder
    {
        internal readonly IServiceCollection services;

        public MultiTenantBuilder(IServiceCollection services)
        {
            this.services = services;
        }

        /// <summary>
        /// Adds per-tenant configuration for an options class.
        /// </summary>
        /// <param name="tenantConfig">The configuration action to be run for each tenant.</param>
        /// <returns>The same <c>MultiTenantBuilder</c> passed into the method.</returns>
        public MultiTenantBuilder WithPerTenantOptionsConfig<TOptions>(Action<TOptions, TenantContext> tenantConfig) where TOptions : class
        {
            services.TryAddSingleton<IOptionsMonitorCache<TOptions>>(sp =>
                {
                    return (MultiTenantOptionsCache<TOptions>)
                        ActivatorUtilities.CreateInstance(sp, typeof(MultiTenantOptionsCache<TOptions>), new[] { tenantConfig });
                });

            return this;
        }

        /// <summary>
        /// Adds an empty InMemoryMultiTenantStore to the application.
        /// </summary>
        /// <returns>The same <c>MultiTenantBuilder</c> passed into the method.</returns>
        public MultiTenantBuilder WithInMemoryStore(bool ignoreCase = true) => WithInMemoryStore(_ => { }, ignoreCase);

        /// <summary>
        /// Adds and configures <c>InMemoryMultiTenantStore</c> to the application using the provided <c>ConfigurationSeciont</c>.
        /// </summary>
        /// <param name="config">The <c>ConfigurationSection</c> which contains the <c>InMemoryMultiTenantStore</c> configuartion settings.</param>
        /// <returns>The same <c>MultiTenantBuilder</c> passed into the method.</returns>
        public MultiTenantBuilder WithInMemoryStore(IConfigurationSection configurationSection, bool ignoreCase = true) =>
            WithInMemoryStore(o => configurationSection.Bind(o), ignoreCase);

        /// <summary>
        /// Adds and configures <c>InMemoryMultiTenantStore</c> to the application using the provided action.
        /// </summary>
        /// <param name="config">A delegate or lambda for configuring the tenant.</param>
        /// <returns>The same <c>MultiTenantBuilder</c> passed into the method.</returns>
        public MultiTenantBuilder WithInMemoryStore(Action<InMemoryMultiTenantStoreOptions> config, bool ignoreCase = true)
        {
            services.AddOptions();
            services.Configure<InMemoryMultiTenantStoreOptions>(config);
            services.TryAddSingleton<IMultiTenantStore>(sp => StoreFactory(sp, ignoreCase));

            return this;
        }

        /// <summary>
        /// Creates an <c>InMemoryMultiTenantStore</c> from configured <c>InMemoryMultiTenantStoreOptions</c>.
        /// </summary>
        /// <param name="sp"></param>
        /// <returns></returns>
        private InMemoryMultiTenantStore StoreFactory(IServiceProvider sp, bool ignoreCase)
        {
            var optionsAccessor = sp.GetService<IOptions<InMemoryMultiTenantStoreOptions>>();
            var tenantConfigurations = optionsAccessor?.Value.TenantConfigurations ?? new InMemoryMultiTenantStoreOptions.TenantConfiguration[0];
            var logger = sp.GetService<ILogger<InMemoryMultiTenantStore>>();
            var store = new InMemoryMultiTenantStore(ignoreCase, logger);

            try
            {
                foreach (var tenantConfig in tenantConfigurations)
                {
                    if (string.IsNullOrWhiteSpace(tenantConfig.Id) ||
                        string.IsNullOrWhiteSpace(tenantConfig.Identifier))
                        throw new MultiTenantException("Tenant Id and Identifer cannot be null or whitespace.");

                    var tc = new TenantContext(tenantConfig.Id,
                                               tenantConfig.Identifier,
                                               tenantConfig.Name,
                                               tenantConfig.ConnectionString ?? optionsAccessor.Value.DefaultConnectionString,
                                               null,
                                               null);

                    // Add any other items from the config, but don't override
                    // the explicitly named items.
                    foreach (var item in tenantConfig.Items ?? new Dictionary<string, string>())
                    {
                        if (!tc.Items.ContainsKey(item.Key))
                            tc.Items.Add(item.Key, item.Value);
                    }

                    if (!store.TryAdd(tc).Result)
                        throw new MultiTenantException($"Unable to add {tc.Identifier} is already configured.");
                }
            }
            catch (Exception e)
            {
                throw new MultiTenantException
                    ("Unable to add tenant context to store.", e);
            }

            return store;
        }

        /// <summary>
        /// Adds and configures a <c>StaticMultiTenantStrategy</c> to the application.
        /// </summary>
        /// <param name="identifier">The tenant identifier to use for all tenant resolution.</param>
        /// <returns>The same <c>MultiTenantBuilder</c> passed into the method.</returns>
        public MultiTenantBuilder WithStaticStrategy(string identifier)
        {
            services.TryAddSingleton<IMultiTenantStrategy>(sp => new StaticMultiTenantStrategy(identifier, sp.GetService<ILogger<StaticMultiTenantStrategy>>()));

            return this;
        }

        /// <summary>
        /// Adds and configures a <c>BasePathMultiTenantStrategy</c> to the application.
        /// </summary>
        /// <returnsThe same <c>MultiTenantBuilder</c> passed into the method.></returns>
        public MultiTenantBuilder WithBasePathStrategy()
        {
            services.TryAddSingleton<IMultiTenantStrategy>(sp =>
            {
                return new BasePathMultiTenantStrategy(sp.GetService<ILogger<BasePathMultiTenantStrategy>>());
            });

            return this;
        }

        /// <summary>
        /// Adds and configures a <c>RouteMultiTenantStrategy</c> to the application.
        /// </summary>
        /// <param name="tenantParam">The name of the route parameter used to determine the tenant identifier.</param>
        /// <returns>The same <c>MultiTenantBuilder</c> passed into the method.</returns>
        public MultiTenantBuilder WithRouteStrategy(string tenantParam = "__tenant__")
        {
            services.TryAddSingleton<IMultiTenantStrategy>(sp => new RouteMultiTenantStrategy(tenantParam, sp.GetService<ILogger<RouteMultiTenantStrategy>>()));

            return this;
        }

        /// <summary>
        /// Adds and configures a <c>HostMultiTenantStrategy</c> to the application.
        /// </summary>
        /// <returns>The same <c>MultiTenantBuilder</c> passed into the method.</returns>
        public MultiTenantBuilder WithHostStrategy(string template = "__tenant__.*")
        {
            services.TryAddSingleton<IMultiTenantStrategy>(sp =>
            {
                return new HostMultiTenantStrategy(template, sp.GetService<ILogger<HostMultiTenantStrategy>>());
            });

            return this;
        }
    }
}