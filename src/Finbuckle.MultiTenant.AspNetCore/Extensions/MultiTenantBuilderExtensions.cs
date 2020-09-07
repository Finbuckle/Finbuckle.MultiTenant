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
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Security.Claims;
using Finbuckle.MultiTenant.Internal;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provides builder methods for Finbuckle.MultiTenant services and configuration.
    /// </summary>
    public static class FinbuckleMultiTenantBuilderExtensions
    {
        /// <summary>
        /// Configures authentication options to enable per-tenant behavior.
        /// </summary>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithPerTenantAuthentication<TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder)
            where TTenantInfo : class, ITenantInfo, new()
        {
            builder.Services.ConfigureAll<CookieAuthenticationOptions>(options =>
            {
                // Validate that claimed tenant matches current tenant.
                var origOnValidatePrincipal = options.Events.OnValidatePrincipal;
                options.Events.OnValidatePrincipal = async context =>
                {
                    await origOnValidatePrincipal(context);

                    if(context.Principal == null)
                        return;
                    
                    var currentTenant = context.HttpContext.GetMultiTenantContext<TTenantInfo>()?.TenantInfo?.Identifier;
                    
                    // If no current tenant and no tenant claim then OK
                    if(currentTenant == null && !context.Principal.Claims.Any(c => c.Type == Constants.TenantToken))
                        return;

                    // Does a tenant claim for the principle match the current tenant?
                    if(!context.Principal.Claims.Where(c => c.Type == Constants.TenantToken && String.Equals(c.Value, currentTenant, StringComparison.OrdinalIgnoreCase)).Any())
                        context.RejectPrincipal();
                };

                // Set the tenant claim when signing in.
                var origOnSigningIn = options.Events.OnSigningIn;
                options.Events.OnSigningIn = async context =>
                {
                    await origOnSigningIn(context);

                    if(context.Principal == null)
                        return;

                    var identity = (ClaimsIdentity)context.Principal.Identity;
                    var currentTenant = context.HttpContext.GetMultiTenantContext<TTenantInfo>()?.TenantInfo?.Identifier;

                    if(currentTenant != null)
                        identity.AddClaim(new Claim(Constants.TenantToken, currentTenant));
                };
            });

            // Set per-tenant cookie options by convention.
            builder.WithPerTenantOptions<CookieAuthenticationOptions>((options, tc) =>
            {
                var d = (dynamic)tc;
                try { options.LoginPath = ((string)d.CookieLoginPath).Replace(Constants.TenantToken, tc.Identifier); } catch { }
                try { options.LogoutPath = ((string)d.CookieLogoutPath).Replace(Constants.TenantToken, tc.Identifier); } catch { }
                try { options.AccessDeniedPath = ((string)d.CookieAccessDeniedPath).Replace(Constants.TenantToken, tc.Identifier); } catch { }
            });

            builder.WithRemoteAuthenticationCallbackStrategy();
            
            // We need to "decorate" IAuthenticationService so callbacks so that
            // remote authentication can get the tenant from the authentication
            // properties in the state parameter.
            if (!builder.Services.Where(s => s.ServiceType == typeof(IAuthenticationService)).Any())
                throw new MultiTenantException("WithRemoteAuthenticationCallbackStrategy() must be called after AddAutheorization() in ConfigureServices.");
            builder.Services.DecorateService<IAuthenticationService, MultiTenantAuthenticationService<TTenantInfo>>();
            
            // Set per-tenant OpenIdConnect options by convention.
            builder.WithPerTenantOptions<OpenIdConnectOptions>((options, tc) =>
            {
                var d = (dynamic)tc;
                try { options.Authority = d.OpenIdConnectAuthority; } catch { }
                try { options.ClientId = d.OpenIdConnectClientId; } catch { }
                try { options.ClientSecret = d.OpenIdConnectClientSecret; } catch { }
            });

            // Replace IAuthenticationSchemeProvider so that the options aren't
            // cached and can be used per-tenant.
            builder.Services.Replace(ServiceDescriptor.Singleton<IAuthenticationSchemeProvider, MultiTenantAuthenticationSchemeProvider>());
            var challengeSchemeProp = typeof(TTenantInfo).GetProperty("ChallengeScheme");
            if (challengeSchemeProp != null && challengeSchemeProp.PropertyType == typeof(string))
            {
                builder.WithPerTenantOptions<AuthenticationOptions>((options, tc)
                    => options.DefaultChallengeScheme = (string)challengeSchemeProp.GetValue(tc) ?? options.DefaultChallengeScheme);
            }

            return builder;
        }

        /// <summary>
        /// Adds and configures a SessionStrategy to the application.
        /// </summary>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithSessionStrategy<TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder)
            where TTenantInfo : class, ITenantInfo, new()
            => builder.WithStrategy<SessionStrategy>(ServiceLifetime.Singleton, Constants.TenantToken);

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
        private static FinbuckleMultiTenantBuilder<TTenantInfo> WithRemoteAuthenticationCallbackStrategy<TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder)
            where TTenantInfo : class, ITenantInfo, new()
        {
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
        /// Adds and configures a RouteStrategy with a route parameter Constants.TenantToken to the application.
        /// </summary>
        /// <param name="configRoutes">Delegate to configure the routes.</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithRouteStrategy<TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder,
                                                                    Action<IRouteBuilder> configRoutes)
                where TTenantInfo : class, ITenantInfo, new()
            => builder.WithRouteStrategy(Constants.TenantToken, configRoutes);

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
        /// Adds and configures a RouteStrategy with a route parameter Constants.TenantToken to the application.
        /// </summary>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithRouteStrategy<TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder)
            where TTenantInfo : class, ITenantInfo, new()
            => builder.WithRouteStrategy(Constants.TenantToken);

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
            => builder.WithHostStrategy($"{Constants.TenantToken}.*");

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
        
        /// <summary>
        /// Adds and configures a ClaimStrategy with tenantKey "__tenant__" to the application.
        /// </summary>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithClaimStrategy<TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder) where TTenantInfo : class, ITenantInfo, new()
        {
            return builder.WithStrategy<ClaimStrategy>(ServiceLifetime.Singleton, Constants.TenantToken);
        }

        /// <summary>
        /// Adds and configures a HostStrategy to the application.
        /// </summary>
        /// <param name="tenantKey">The template for determining the tenant identifier in the host.</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithClaimStrategy<TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder, string tenantKey) where TTenantInfo : class, ITenantInfo, new()
        {
            return builder.WithStrategy<ClaimStrategy>(ServiceLifetime.Singleton, tenantKey);
        }
    }
}