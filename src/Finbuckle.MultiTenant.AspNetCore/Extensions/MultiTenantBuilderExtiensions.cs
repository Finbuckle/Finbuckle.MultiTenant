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

#if NETSTANDARD2_0

using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Authentication;
using Finbuckle.MultiTenant.AspNetCore;
using Finbuckle.MultiTenant.Strategies;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provices builder methods for Finbuckle.MultiTenant services and configuration.
    /// </summary>
    public static class FinbuckeMultiTenantBuilderExtensions
    {
        /// <summary>
        /// Configures support for multitenant OAuth and OpenIdConnect.
        /// </summary>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder WithRemoteAuthentication(this FinbuckleMultiTenantBuilder builder)
        {
            // Replace needed instead of TryAdd...
            builder.Services.Replace(ServiceDescriptor.Singleton<IAuthenticationSchemeProvider, MultiTenantAuthenticationSchemeProvider>());
            // builder.Services.Replace(ServiceDescriptor.Scoped<IAuthenticationService, MultiTenantAuthenticationService>());

            builder.Services.DecorateService<IAuthenticationService, MultiTenantAuthenticationService>();
            builder.Services.TryAddSingleton<RemoteAuthenticationStrategy>();

            return builder;
        }

        /// <summary>
        /// Adds and configures a BasePathStrategy to the application.
        /// </summary>
        /// <returns>The same MultiTenantBuilder passed into the method.></returns>
        public static FinbuckleMultiTenantBuilder WithBasePathStrategy(this FinbuckleMultiTenantBuilder builder)
            => builder.WithStrategy<BasePathStrategy>(ServiceLifetime.Singleton);

        /// <summary>
        /// Adds and configures a RouteStrategy with a route parameter "\_\_tenant\_\_" to the application.
        /// </summary>
        /// <param name="configRoutes">Delegate to configure the routes.</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder WithRouteStrategy(this FinbuckleMultiTenantBuilder builder,
                                                                    Action<IRouteBuilder> configRoutes)
            => builder.WithRouteStrategy("__tenant__", configRoutes);

        /// <summary>
        /// Adds and configures a RouteStrategy to the application.
        /// </summary>
        /// <param name="tenantParam">The name of the route parameter used to determine the tenant identifier.</param>
        /// <param name="configRoutes">Delegate to configure the routes.</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder WithRouteStrategy(this FinbuckleMultiTenantBuilder builder,
                                                                    string tenantParam,
                                                                    Action<IRouteBuilder> configRoutes)
        {
            if (string.IsNullOrWhiteSpace(tenantParam))
            {
                throw new ArgumentException("Invalud value for \"tenantParam\"", nameof(tenantParam));
            }

            if (configRoutes == null)
            {
                throw new ArgumentNullException(nameof(configRoutes));
            }

            return builder.WithStrategy<RouteStrategy>(ServiceLifetime.Singleton, new object[] { tenantParam, configRoutes });
        }

        /// <summary>
        /// Adds and configures a HostStrategy with template "\_\_tenant\_\_.*" to the application.
        /// </summary>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder WithHostStrategy(this FinbuckleMultiTenantBuilder builder)
            => builder.WithHostStrategy("__tenant__.*");

        /// <summary>
        /// Adds and configures a HostStrategy to the application.
        /// </summary>
        /// <param name="template">The template for determining the tenant identifier in the host.</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder WithHostStrategy(this FinbuckleMultiTenantBuilder builder,
                                                                   string template)
        {
            if (string.IsNullOrWhiteSpace(template))
            {
                throw new ArgumentException("Invalid value for \"template\"", nameof(template));
            }

            return builder.WithStrategy<HostStrategy>(ServiceLifetime.Singleton, new object[] { template });
        }
    }
}

#else

using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Authentication;
using Finbuckle.MultiTenant.AspNetCore;
using Finbuckle.MultiTenant.Strategies;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provices builder methods for Finbuckle.MultiTenant services and configuration.
    /// </summary>
    public static class FinbuckeMultiTenantBuilderExtensions
    {
        /// <summary>
        /// Configures support for multitenant OAuth and OpenIdConnect.
        /// </summary>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder WithRemoteAuthentication(this FinbuckleMultiTenantBuilder builder)
        {
            // Replace needed instead of TryAdd...
            builder.Services.Replace(ServiceDescriptor.Singleton<IAuthenticationSchemeProvider, MultiTenantAuthenticationSchemeProvider>());
            // builder.Services.Replace(ServiceDescriptor.Scoped<IAuthenticationService, MultiTenantAuthenticationService>());
            builder.Services.DecorateService<IAuthenticationService, MultiTenantAuthenticationService>();
            builder.Services.TryAddSingleton<RemoteAuthenticationStrategy>();

            return builder;
        }

        /// <summary>
        /// Adds and configures a BasePathStrategy to the application.
        /// </summary>
        /// <returns>The same MultiTenantBuilder passed into the method.></returns>
        public static FinbuckleMultiTenantBuilder WithBasePathStrategy(this FinbuckleMultiTenantBuilder builder)
            => builder.WithStrategy<BasePathStrategy>(ServiceLifetime.Singleton);

        /// <summary>
        /// Adds and configures a RouteStrategy with a route parameter "\_\_tenant\_\_" to the application.
        /// </summary>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder WithRouteStrategy(this FinbuckleMultiTenantBuilder builder)
            => builder.WithRouteStrategy("__tenant__");

        /// <summary>
        /// Adds and configures a RouteStrategy to the application.
        /// </summary>
        /// <param name="tenantParam">The name of the route parameter used to determine the tenant identifier.</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder WithRouteStrategy(this FinbuckleMultiTenantBuilder builder,
                                                                    string tenantParam)
        {
            if (string.IsNullOrWhiteSpace(tenantParam))
            {
                throw new ArgumentException("Invalud value for \"tenantParam\"", nameof(tenantParam));
            }

            return builder.WithStrategy<RouteStrategy>(ServiceLifetime.Singleton, new object[] { tenantParam });
        }

        /// <summary>
        /// Adds and configures a HostStrategy with template "\_\_tenant\_\_.*" to the application.
        /// </summary>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder WithHostStrategy(this FinbuckleMultiTenantBuilder builder)
            => builder.WithHostStrategy("__tenant__.*");

        /// <summary>
        /// Adds and configures a HostStrategy to the application.
        /// </summary>
        /// <param name="template">The template for determining the tenant identifier in the host.</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder WithHostStrategy(this FinbuckleMultiTenantBuilder builder,
                                                                   string template)
        {
            if (string.IsNullOrWhiteSpace(template))
            {
                throw new ArgumentException("Invalid value for \"template\"", nameof(template));
            }

            return builder.WithStrategy<HostStrategy>(ServiceLifetime.Singleton, new object[] { template });
        }
    }
}

#endif