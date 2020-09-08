//    Copyright 2018-2020 Andrew White
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
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public partial class FinbuckleMultiTenantBuilder<TTenantInfo> where TTenantInfo : class, ITenantInfo, new()
    {
        public IServiceCollection Services { get; set; }

        public FinbuckleMultiTenantBuilder(IServiceCollection services)
        {
            Services = services;
        }

        /// <summary>
        /// Adds per-tenant configuration for an options class.
        /// </summary>
        /// <param name="tenantConfigureOptions">The configuration action to be run for each tenant.</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public FinbuckleMultiTenantBuilder<TTenantInfo> WithPerTenantOptions<TOptions>(Action<TOptions, TTenantInfo> tenantConfigureOptions) where TOptions : class, new()
        {
            if (tenantConfigureOptions == null)
            {
                throw new ArgumentNullException(nameof(tenantConfigureOptions));
            }

            // Handles multiplexing cached options.
            Services.TryAddSingleton<IOptionsMonitorCache<TOptions>, MultiTenantOptionsCache<TOptions, TTenantInfo>>();

            // Necessary to apply tenant options in between configuration and postconfiguration
            Services.AddSingleton<ITenantConfigureOptions<TOptions, TTenantInfo>, TenantConfigureOptions<TOptions, TTenantInfo>>(sp => new TenantConfigureOptions<TOptions, TTenantInfo>(tenantConfigureOptions));
            Services.TryAddTransient<IOptionsFactory<TOptions>, MultiTenantOptionsFactory<TOptions, TTenantInfo>>();
            Services.TryAddScoped<IOptionsSnapshot<TOptions>>(sp => BuildOptionsManager<TOptions>(sp));
            Services.TryAddSingleton<IOptions<TOptions>>(sp => BuildOptionsManager<TOptions>(sp));

            return this;
        }


        private static MultiTenantOptionsManager<TOptions> BuildOptionsManager<TOptions>(IServiceProvider sp) where TOptions : class, new()
        {
            var cache = ActivatorUtilities.CreateInstance(sp, typeof(MultiTenantOptionsCache<TOptions, TTenantInfo>));
            return (MultiTenantOptionsManager<TOptions>)
                ActivatorUtilities.CreateInstance(sp, typeof(MultiTenantOptionsManager<TOptions>), new[] { cache });
        }

        /// <summary>
        /// Adds and configures a IMultiTenantStore to the application using default dependency injection.
        /// </summary>>
        /// <param name="lifetime">The service lifetime.</param>
        /// <param name="parameters">a paramter list for any constructor paramaters not covered by dependency injection.</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public FinbuckleMultiTenantBuilder<TTenantInfo> WithStore<TStore>(ServiceLifetime lifetime, params object[] parameters)
            where TStore : IMultiTenantStore<TTenantInfo>
            => WithStore<TStore>(lifetime, sp => ActivatorUtilities.CreateInstance<TStore>(sp, parameters));

        /// <summary>
        /// Adds and configures a IMultiTenantStore to the application using a factory method.
        /// </summary>
        /// <param name="lifetime">The service lifetime.</param>
        /// <param name="factory">A delegate that will create and configure the store.</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public FinbuckleMultiTenantBuilder<TTenantInfo> WithStore<TStore>(ServiceLifetime lifetime, Func<IServiceProvider, TStore> factory)
            where TStore : IMultiTenantStore<TTenantInfo>
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            // Note: can't use TryAddEnumerable here because ServiceDescriptor.Describe with a factory can't set implementation type.
            Services.Add(ServiceDescriptor.Describe(typeof(IMultiTenantStore<TTenantInfo>), sp => factory(sp), lifetime));

            return this;
        }

        /// <summary>
        /// Adds and configures a IMultiTenantStrategy to the applicationusing default dependency injection.
        /// </summary>
        /// <param name="lifetime">The service lifetime.</param>
        /// <param name="parameters">a paramter list for any constructor paramaters not covered by dependency injection.</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public FinbuckleMultiTenantBuilder<TTenantInfo> WithStrategy<TStrategy>(ServiceLifetime lifetime, params object[] parameters) where TStrategy : IMultiTenantStrategy
            => WithStrategy(lifetime, sp => ActivatorUtilities.CreateInstance<TStrategy>(sp, parameters));

        /// <summary>
        /// Adds and configures a IMultiTenantStrategy to the application using a factory method.
        /// </summary>
        /// <param name="lifetime">The service lifetime.</param>
        /// <param name="factory">A delegate that will create and configure the strategy.</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public FinbuckleMultiTenantBuilder<TTenantInfo> WithStrategy<TStrategy>(ServiceLifetime lifetime, Func<IServiceProvider, TStrategy> factory)
            where TStrategy : IMultiTenantStrategy
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            // Potential for multiple entries per service is intended.
            Services.Add(ServiceDescriptor.Describe(typeof(IMultiTenantStrategy), sp => factory(sp), lifetime));

            return this;
        }
    }
}