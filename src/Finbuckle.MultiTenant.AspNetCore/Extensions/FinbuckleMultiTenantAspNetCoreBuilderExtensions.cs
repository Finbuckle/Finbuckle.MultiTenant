// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.AspNetCore;
using Finbuckle.MultiTenant.Internal;
using Finbuckle.MultiTenant.Strategies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provides builder methods for Finbuckle.MultiTenant services and configuration.
    /// </summary>
    public static class FinbuckleMultiTenantBuilderExtensionsAspNetCore
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
        /// <param name="builder">MultiTenantBuilder instance.</param>
        /// <param name="config">Authentication options config.</param>
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
        [SuppressMessage("ReSharper", "EmptyGeneralCatchClause")]
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
                    // Skip if bypass set (e.g. ClaimsStrategy in effect)
                    if(context.HttpContext.Items.Keys.Contains($"{Constants.TenantToken}__bypass_validate_principal__"))
                        return;
                    
                    var currentTenant = context.HttpContext.GetMultiTenantContext<TTenantInfo>()?.TenantInfo?.Identifier;
                    string authTenant = null;
                    if (context.Properties.Items.ContainsKey(Constants.TenantToken))
                    {
                        authTenant = context.Properties.Items[Constants.TenantToken];
                    }
                    else
                    {
                        var loggerFactory = context.HttpContext.RequestServices.GetService<ILoggerFactory>();
                        loggerFactory.CreateLogger(typeof(FinbuckleMultiTenantBuilderExtensionsAspNetCore)).LogWarning("No tenant found in authentication properties.");
                    }

                    // Does the current tenant match the auth property tenant?
                    if(!string.Equals(currentTenant, authTenant, StringComparison.OrdinalIgnoreCase))
                        context.RejectPrincipal();
                    
                    if(origOnValidatePrincipal != null)
                        await origOnValidatePrincipal(context);
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
        /// <param name="builder">MultiTenantBuilder instance.</param>
        /// <param name="config">Authentication options config</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithPerTenantAuthenticationCore<TTenantInfo>(
            this FinbuckleMultiTenantBuilder<TTenantInfo> builder, Action<MultiTenantAuthenticationOptions> config =
                null)
            where TTenantInfo : class, ITenantInfo, new()
        {

            config ??= _ => { };
            builder.Services.Configure(config);
            
            // We need to "decorate" IAuthenticationService so callbacks so that
            // remote authentication can get the tenant from the authentication
            // properties in the state parameter.
            if (builder.Services.All(s => s.ServiceType != typeof(IAuthenticationService)))
                throw new MultiTenantException("WithPerTenantAuthenticationCore() must be called after AddAuthentication() in ConfigureServices.");
            builder.Services.DecorateService<IAuthenticationService, MultiTenantAuthenticationService<TTenantInfo>>();
            
            // Replace IAuthenticationSchemeProvider so that the options aren't
            // cached and can be used per-tenant.
            builder.Services.Replace(ServiceDescriptor.Singleton<IAuthenticationSchemeProvider, MultiTenantAuthenticationSchemeProvider>());

            return builder;
        }

        /// <summary>
        /// Adds and configures a SessionStrategy to the application.
        /// </summary>
        /// <param name="builder">MultiTenantBuilder instance.</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithSessionStrategy<TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder)
            where TTenantInfo : class, ITenantInfo, new()
            => builder.WithStrategy<SessionStrategy>(ServiceLifetime.Singleton, Constants.TenantToken);

        /// <summary>
        /// Adds and configures a SessionStrategy to the application.
        /// </summary>
        /// <param name="builder">MultiTenantBuilder instance.</param>
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
        /// <param name="builder">MultiTenantBuilder instance.</param>
        /// <param name="tenantParam">The name of the route parameter used to determine the tenant identifier.</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithRouteStrategy<TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder, string tenantParam)
            where TTenantInfo : class, ITenantInfo, new()
        {
            if (string.IsNullOrWhiteSpace(tenantParam))
            {
                throw new ArgumentException("Invalud value for \"tenantParam\"", nameof(tenantParam));
            }

            return builder.WithStrategy<RouteStrategy>(ServiceLifetime.Singleton, tenantParam);
        }
// #endif

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
        /// <param name="builder">MultiTenantBuilder instance.</param>
        /// <param name="template">The template for determining the tenant identifier in the host.</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithHostStrategy<TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder, string template)
            where TTenantInfo : class, ITenantInfo, new()
        {
            if (string.IsNullOrWhiteSpace(template))
            {
                throw new ArgumentException("Invalid value for \"template\"", nameof(template));
            }

            return builder.WithStrategy<HostStrategy>(ServiceLifetime.Singleton, template);
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
        /// <param name="builder">MultiTenantBuilder instance.</param>
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
        /// <param name="builder">MultiTenantBuilder instance.</param>
        /// <param name="tenantKey">The template for determining the tenant identifier in the host.</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithHeaderStrategy<TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder, string tenantKey) where TTenantInfo : class, ITenantInfo, new()
        {
            return builder.WithStrategy<HeaderStrategy>(ServiceLifetime.Singleton, tenantKey);
        }
    }
}
