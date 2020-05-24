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
using Microsoft.AspNetCore.Authentication;
using Finbuckle.MultiTenant.AspNetCore;
using Finbuckle.MultiTenant.Strategies;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Routing;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provices builder methods for Finbuckle.MultiTenant services and configuration.
    /// </summary>
    public static class FinbuckleMultiTenantBuilderExtensions
    {
        /// <summary>
        /// Adds and configures a SessionStrategy to the application.
        /// </summary>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithSessionStrategy<TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder)
            where TTenantInfo : class, ITenantInfo, new()
            => builder.WithStrategy<SessionStrategy>(ServiceLifetime.Singleton, "__tenant__");

        /// <summary>
        /// Adds and configures a SessionStrategy to the application.
        /// </summary>
        /// <param name="tenantLey">The session key to use.</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithSessionStrategy<TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder, string tenantKey)
            where TTenantInfo : class, ITenantInfo, new()
            => builder.WithStrategy<SessionStrategy>(ServiceLifetime.Singleton, tenantKey);

        /// <summary>
        /// Adds and configures a RemoteAuthenticationCallbackStrategy to the application.
        /// </summary>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithRemoteAuthenticationCallbackStrategy<TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder)
            where TTenantInfo : class, ITenantInfo, new()
        {
            // Replace needed instead of TryAdd...
            builder.Services.Replace(ServiceDescriptor.Singleton<IAuthenticationSchemeProvider, MultiTenantAuthenticationSchemeProvider>());
            
            // We need to "decorate" IAuthenticationService
            if(!builder.Services.Where(s => s.ServiceType == typeof(IAuthenticationService)).Any())
                throw new MultiTenantException("WithRemoteAuthenticationCallbackStrategy() must be called after AddAutheorization() in ConfigureServices.");
            builder.Services.DecorateService<IAuthenticationService, MultiTenantAuthenticationService<TTenantInfo>>();
            
            return builder.WithStrategy<RemoteAuthenticationCallbackStrategy>(ServiceLifetime.Singleton);
        }

        /// <summary>
        /// Adds and configures a BasePathStrategy to the application.
        /// </summary>
        /// <returns>The same MultiTenantBuilder passed into the method.></returns>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithBasePathStrategy<TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder)
            where TTenantInfo : class, ITenantInfo, new()
            => builder.WithStrategy<BasePathStrategy>(ServiceLifetime.Singleton);

#if NETCOREAPP2_1
        /// <summary>
        /// Adds and configures a RouteStrategy with a route parameter "__tenant__" to the application.
        /// </summary>
        /// <param name="configRoutes">Delegate to configure the routes.</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithRouteStrategy<TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder,
                                                                    Action<IRouteBuilder> configRoutes)
                where TTenantInfo : class, ITenantInfo, new()
            => builder.WithRouteStrategy("__tenant__", configRoutes);

        /// <summary>
        /// Adds and configures a RouteStrategy to the application.
        /// </summary>
        /// <param name="tenantParam">The name of the route parameter used to determine the tenant identifier.</param>
        /// <param name="configRoutes">Delegate to configure the routes.</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithRouteStrategy<TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder,
                                                                    string tenantParam,
                                                                    Action<IRouteBuilder> configRoutes)
            where TTenantInfo : class, ITenantInfo, new()
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
#else
        /// <summary>
        /// Adds and configures a RouteStrategy with a route parameter "__tenant__" to the application.
        /// </summary>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithRouteStrategy<TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder)
            where TTenantInfo : class, ITenantInfo, new()
            => builder.WithRouteStrategy("__tenant__");

        /// <summary>
        /// Adds and configures a RouteStrategy to the application.
        /// </summary>
        /// <param name="tenantParam">The name of the route parameter used to determine the tenant identifier.</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithRouteStrategy<TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder, string tenantParam)
            where TTenantInfo : class, ITenantInfo, new()
        {
            if (string.IsNullOrWhiteSpace(tenantParam))
            {
                throw new ArgumentException("Invalud value for \"tenantParam\"", nameof(tenantParam));
            }

            return builder.WithStrategy<RouteStrategy>(ServiceLifetime.Singleton, new object[] { tenantParam });
        }
#endif

        /// <summary>
        /// Adds and configures a HostStrategy with template "__tenant__.*" to the application.
        /// </summary>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithHostStrategy<TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder)
            where TTenantInfo : class, ITenantInfo, new()
            => builder.WithHostStrategy("__tenant__.*");

        /// <summary>
        /// Adds and configures a HostStrategy to the application.
        /// </summary>
        /// <param name="template">The template for determining the tenant identifier in the host.</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithHostStrategy<TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder, string template)
            where TTenantInfo : class, ITenantInfo, new()
        {
            if (string.IsNullOrWhiteSpace(template))
            {
                throw new ArgumentException("Invalid value for \"template\"", nameof(template));
            }

            return builder.WithStrategy<HostStrategy>(ServiceLifetime.Singleton, new object[] { template });
        }
    }
}