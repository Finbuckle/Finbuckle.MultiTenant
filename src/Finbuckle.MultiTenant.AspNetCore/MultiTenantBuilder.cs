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

namespace Finbuckle.MultiTenant.AspNetCore
{
    /// <summary>
    /// Configures <c>Finbuckle.MultiTenant.AspNetCore</c> services and configuration.
    /// </summary>
    public class MultiTenantBuilder
    {
        internal readonly IServiceCollection _services;

        public MultiTenantBuilder(IServiceCollection services)
        {
            _services = services;
        }

        /// <summary>
        /// Adds per-tenant configuration for an options class.
        /// </summary>
        /// <param name="tenantConfig">The configuration action to be run for each tenant.</param>
        /// <returns>The same <c>MultiTenantBuilder</c> passed into the method.</returns>
        public MultiTenantBuilder WithPerTenantOptionsConfig<TOptions>(Action<TOptions, TenantContext> tenantConfig) where TOptions : class
        {
            _services.TryAddSingleton<IOptionsMonitorCache<TOptions>>(sp =>
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
        public MultiTenantBuilder WithInMemoryStore() => WithInMemoryStore(_ => {});

        /// <summary>
        /// Adds and configures <c>InMemoryMultiTenantStore</c> to the application using the provided action.
        /// </summary>
        /// <param name="config">A delegate or lambda for configuring the tenant.</param>
        /// <returns>The same <c>MultiTenantBuilder</c> passed into the method.</returns>
        public MultiTenantBuilder WithInMemoryStore(Action<InMemoryMultiTenantStoreOptions> config)
        {
            _services.AddOptions();
            _services.Configure<InMemoryMultiTenantStoreOptions>(config);
            _services.TryAddSingleton<IMultiTenantStore>(StoreFactory);

            return this;
        }

        /// <summary>
        /// Adds and configures <c>InMemoryMultiTenantStore</c> to the application using the provided <c>ConfigurationSeciont</c>.
        /// </summary>
        /// <param name="config">The <c>ConfigurationSection</c> which contains the <c>InMemoryMultiTenantStore</c> configuartion settings.</param>
        /// <returns>The same <c>MultiTenantBuilder</c> passed into the method.</returns>
        public MultiTenantBuilder WithInMemoryStore(IConfigurationSection configurationSection)
        {
            _services.AddOptions();
            _services.Configure<InMemoryMultiTenantStoreOptions>(o => configurationSection.Bind(o));
            _services.TryAddSingleton<IMultiTenantStore>(StoreFactory);

            return this;
        }

        /// <summary>
        /// Creates an <c>InMemoryMultiTenantStore</c> from configured <c>InMemoryMultiTenantStoreOptions</c>.
        /// </summary>
        /// <param name="sp"></param>
        /// <returns></returns>
        private InMemoryMultiTenantStore StoreFactory(IServiceProvider sp)
        {
            var optionsAccessor = sp.GetService<IOptions<InMemoryMultiTenantStoreOptions>>();
            var tenantConfigurations = optionsAccessor?.Value.TenantConfigurations ?? new InMemoryMultiTenantStoreOptions.TenantConfiguration[0];

            var store = new InMemoryMultiTenantStore();

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
            _services.TryAddSingleton<IMultiTenantStrategy>(sp => new StaticMultiTenantStrategy(identifier));

            return this;
        }

        /// <summary>
        /// Adds and configures a <c>BasePathMultiTenantStrategy</c> to the application.
        /// </summary>
        /// <returnsThe same <c>MultiTenantBuilder</c> passed into the method.></returns>
        public MultiTenantBuilder WithBasePathStrategy()
        {
            _services.TryAddSingleton<IMultiTenantStrategy, BasePathMultiTenantStrategy>();

            return this;
        }

        /// <summary>
        /// Adds and configures a <c>RouteMultiTenantStrategy</c> to the application.
        /// </summary>
        /// <param name="tenantParam">The name of the route parameter used to determine the tenant identifier.</param>
        /// <returns>The same <c>MultiTenantBuilder</c> passed into the method.</returns>
        public MultiTenantBuilder WithRouteStrategy(string tenantParam = "__tenant__")
        {
            _services.TryAddSingleton<IMultiTenantStrategy>(sp => new RouteMultiTenantStrategy(tenantParam));

            return this;
        }

        /// <summary>
        /// Adds and configures a <c>HostMultiTenantStrategy</c> to the application.
        /// </summary>
        /// <returns>The same <c>MultiTenantBuilder</c> passed into the method.</returns>
        public MultiTenantBuilder WithHostStrategy()
        {
            _services.TryAddSingleton<IMultiTenantStrategy, HostMultiTenantStrategy>();

            return this;
        }
    }
}