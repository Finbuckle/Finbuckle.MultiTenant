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
            return WithPerTenantAuthentication(builder, _ => { });
        }

        /// <summary>
        /// Configures per-tenant authentication behavior.
        /// </summary>
        /// <param name="config">Authentication options config</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithPerTenantAuthentication<TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder, Action<MultiTenantAuthenticationOptions> config)
             where TTenantInfo : class, ITenantInfo, new()
        {
            builder.WithPerTenantAuthenticationCore(config);
            builder.WithPerTenantAuthenticationConventions();
            builder.WithRemoteAuthenticationCallbackStrategy();

            return builder;
        }

        /// <summary>
        /// Configures conventional functionality for per-tenant authentication.
        /// </summary>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithPerTenantAuthenticationConventions<TTenantInfo>(
            this FinbuckleMultiTenantBuilder<TTenantInfo> builder, Action<MultiTenantAuthenticationOptions> config = null)
            where TTenantInfo : class, ITenantInfo, new()
        {
            // Set events to set and validate tenant for each cookie based authentication principal.
            builder.Services.ConfigureAll<CookieAuthenticationOptions>(options =>
            {
                // Validate that claimed tenant matches current tenant.
                var origOnValidatePrincipal = options.Events.OnValidatePrincipal;
                options.Events.OnValidatePrincipal = async context =>
                {

                    await origOnValidatePrincipal(context);

                    // Skip if no principal or bypass set
                    if(context.Principal == null || context.HttpContext.Items.Keys.Contains($"{Constants.TenantToken}__bypass_validate_principle__"))
                        return;

                    var currentTenant = context.HttpContext.GetMultiTenantContext<TTenantInfo>()?.TenantInfo?.Identifier;

                    // If no current tenant and no tenant claim then OK
                    if(currentTenant == null && !context.Principal.Claims.Any(c => c.Type == Constants.TenantToken))
                        return;

                    // Does a tenant claim for the principal match the current tenant?
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

                    if(currentTenant != null &&
                       !identity.Claims.Where(c => c.Type == Constants.TenantToken && c.Value == currentTenant).Any())
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

            // Set per-tenant OpenIdConnect options by convention.
            builder.WithPerTenantOptions<OpenIdConnectOptions>((options, tc) =>
            {
                var d = (dynamic)tc;
                try { options.Authority = ((string)d.OpenIdConnectAuthority).Replace(Constants.TenantToken, tc.Identifier); } catch { }
                try { options.ClientId = ((string)d.OpenIdConnectClientId).Replace(Constants.TenantToken, tc.Identifier); } catch { }
                try { options.ClientSecret = ((string)d.OpenIdConnectClientSecret).Replace(Constants.TenantToken, tc.Identifier); } catch { }
            });
            
            var challengeSchemeProp = typeof(TTenantInfo).GetProperty("ChallengeScheme");
            if (challengeSchemeProp != null && challengeSchemeProp.PropertyType == typeof(string))
            {
                builder.WithPerTenantOptions<AuthenticationOptions>((options, tc)
                    => options.DefaultChallengeScheme = (string)challengeSchemeProp.GetValue(tc) ?? options.DefaultChallengeScheme);
            }
            
            return builder;
        }

        /// <summary>
        /// Configures core functionality for per-tenant authentication.
        /// </summary>
        /// <param name="config">Authentication options config</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithPerTenantAuthenticationCore<TTenantInfo>(
            this FinbuckleMultiTenantBuilder<TTenantInfo> builder, Action<MultiTenantAuthenticationOptions> config =
                null)
            where TTenantInfo : class, ITenantInfo, new()
        {
#if NETCOREAPP2_1
            config = config ?? new Action<MultiTenantAuthenticationOptions>(_ => { });
#else
            config ??= _ => { };
#endif
            builder.Services.Configure<MultiTenantAuthenticationOptions>(config);
            
            // We need to "decorate" IAuthenticationService so callbacks so that
            // remote authentication can get the tenant from the authentication
            // properties in the state parameter.
            if (builder.Services.All(s => s.ServiceType != typeof(IAuthenticationService)))
                throw new MultiTenantException("WithPerTenantAuthenticationCore() must be called after AddAuthorization() in ConfigureServices.");
            builder.Services.DecorateService<IAuthenticationService, MultiTenantAuthenticationService<TTenantInfo>>();
            
            // Replace IAuthenticationSchemeProvider so that the options aren't
            // cached and can be used per-tenant.
            builder.Services.Replace(ServiceDescriptor.Singleton<IAuthenticationSchemeProvider, MultiTenantAuthenticationSchemeProvider>());

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
        /// <param name="tenantKey">The session key to use.</param>
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
        /// Adds and configures a ClaimStrategy to the application.
        /// </summary>
        /// <param name="tenantKey">The template for determining the tenant identifier in the host.</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithClaimStrategy<TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder, string tenantKey) where TTenantInfo : class, ITenantInfo, new()
        {
            return builder.WithStrategy<ClaimStrategy>(ServiceLifetime.Singleton, tenantKey);
        }

        /// <summary>
        /// Adds and configures a HeaderStrategy with tenantKey "__tenant__" to the application.
        /// </summary>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithHeaderStrategy<TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder) where TTenantInfo : class, ITenantInfo, new()
        {
            return builder.WithStrategy<HeaderStrategy>(ServiceLifetime.Singleton, Constants.TenantToken);
        }

        /// <summary>
        /// Adds and configures a Header to the application.
        /// </summary>
        /// <param name="tenantKey">The template for determining the tenant identifier in the host.</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithHeaderStrategy<TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder, string tenantKey) where TTenantInfo : class, ITenantInfo, new()
        {
            return builder.WithStrategy<HeaderStrategy>(ServiceLifetime.Singleton, tenantKey);
        }
    }
}