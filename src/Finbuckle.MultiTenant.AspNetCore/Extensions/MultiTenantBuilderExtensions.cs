// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System;
using System.Linq;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.AspNetCore;
using Finbuckle.MultiTenant.AspNetCore.Internal;
using Finbuckle.MultiTenant.AspNetCore.Options;
using Finbuckle.MultiTenant.AspNetCore.Strategies;
using Finbuckle.MultiTenant.Internal;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

// ReSharper disable once CheckNamespace
namespace Finbuckle.MultiTenant;

/// <summary>
/// Provides builder methods for Finbuckle.MultiTenant services and configuration.
/// </summary>
public static class MultiTenantBuilderExtensions
{
    /// <summary>
    /// Configures a callback that determines when endpoints should be short circuited
    /// during MultiTenant resolution.
    /// </summary>
    /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
    /// <param name="builder">The <see cref="MultiTenantBuilder&lt;TTenantInfo&gt;"/> instance.</param>
    /// <param name="configureOptions">The short circuit options to configure.</param>
    /// <returns>The <see cref="MultiTenantBuilder&lt;TTenantInfo&gt;"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> ShortCircuitWhen<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder, Action<ShortCircuitWhenOptions> configureOptions)
        where TTenantInfo : class, ITenantInfo, new()
    {
        builder.Services.Configure(configureOptions);

        return builder;
    }

    /// <summary>
    /// Configures endpoints to be short circuited during MultiTenant resolution when
    /// no Tenant was resolved.
    /// </summary>
    /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
    /// <param name="builder">The <see cref="MultiTenantBuilder&lt;TTenantInfo&gt;"/> instance.</param>
    /// <returns>The <see cref="MultiTenantBuilder&lt;TTenantInfo&gt;"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> ShortCircuitWhenTenantNotResolved<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder)
        where TTenantInfo : class, ITenantInfo, new()
    {
        return builder.ShortCircuitWhen(config =>
        {
            config.Predicate = context => !context.IsResolved;
        });
    }

    /// <summary>
    /// Configures endpoints to be short circuited during MultiTenant resolution when
    /// no Tenant was resolved.
    /// </summary>
    /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
    /// <param name="builder">The <see cref="MultiTenantBuilder&lt;TTenantInfo&gt;"/> instance.</param>
    /// <param name="redirectTo">A <see cref="Uri"/> to redirect the request to, if short circuited.</param>
    /// <returns>The <see cref="MultiTenantBuilder&lt;TTenantInfo&gt;"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> ShortCircuitWhenTenantNotResolved<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder, Uri redirectTo)
        where TTenantInfo : class, ITenantInfo, new()
    {
        return builder.ShortCircuitWhen(config =>
        {
            config.Predicate = context => !context.IsResolved;
            config.RedirectTo = redirectTo;
        });
    }

    /// <summary>
    /// Configures authentication options to enable per-tenant behavior.
    /// </summary>
    /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
    /// <returns>The <see cref="MultiTenantBuilder&lt;TTenantInfo&gt;"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithPerTenantAuthentication<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder)
        where TTenantInfo : class, ITenantInfo, new()
    {
        return WithPerTenantAuthentication(builder, _ => { });
    }

    /// <summary>
    /// Configures per-tenant authentication behavior.
    /// </summary>
    /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
    /// <param name="builder">MultiTenantBuilder instance.</param>
    /// <param name="config">Authentication options config.</param>
    /// <returns>The <see cref="MultiTenantBuilder&lt;TTenantInfo&gt;"/> so that additional calls can be chained.</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public static MultiTenantBuilder<TTenantInfo> WithPerTenantAuthentication<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder, Action<MultiTenantAuthenticationOptions> config)
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
    /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
    /// <returns>The <see cref="MultiTenantBuilder&lt;TTenantInfo&gt;"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithPerTenantAuthenticationConventions<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder)
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
                if (context.HttpContext.Items.Keys.Contains(
                        $"{Constants.TenantToken}__bypass_validate_principal__"))
                    return;

                var currentTenant = context.HttpContext.GetMultiTenantContext<TTenantInfo>().TenantInfo?.Identifier;
                string? authTenant = null;
                if (context.Properties.Items.TryGetValue(Constants.TenantToken, out var item))
                {
                    authTenant = item;
                }
                else
                {
                    var loggerFactory = context.HttpContext.RequestServices.GetService<ILoggerFactory>();
                    loggerFactory?.CreateLogger<MultiTenantBuilder<TTenantInfo>>()
                        .LogWarning("No tenant found in authentication properties.");
                }

                // Does the current tenant match the auth property tenant?
                if (!string.Equals(currentTenant, authTenant, StringComparison.OrdinalIgnoreCase))
                    context.RejectPrincipal();

                await origOnValidatePrincipal(context);
            };
        });

        // Set per-tenant cookie options by convention.
        builder.Services.ConfigureAllPerTenant<CookieAuthenticationOptions, TTenantInfo>((options, tc) =>
        {
            if (GetPropertyWithValidValue(tc, "CookieLoginPath") is string loginPath)
                options.LoginPath = loginPath.Replace(Constants.TenantToken, tc.Identifier);

            if (GetPropertyWithValidValue(tc, "CookieLogoutPath") is string logoutPath)
                options.LogoutPath = logoutPath.Replace(Constants.TenantToken, tc.Identifier);

            if (GetPropertyWithValidValue(tc, "CookieAccessDeniedPath") is string accessDeniedPath)
                options.AccessDeniedPath = accessDeniedPath.Replace(Constants.TenantToken, tc.Identifier);
        });

        // Set per-tenant OpenIdConnect options by convention.
        builder.Services.ConfigureAllPerTenant<OpenIdConnectOptions, TTenantInfo>((options, tc) =>
        {
            if (GetPropertyWithValidValue(tc, "OpenIdConnectAuthority") is string authority)
                options.Authority = authority.Replace(Constants.TenantToken, tc.Identifier);

            if (GetPropertyWithValidValue(tc, "OpenIdConnectClientId") is string clientId)
                options.ClientId = clientId.Replace(Constants.TenantToken, tc.Identifier);

            if (GetPropertyWithValidValue(tc, "OpenIdConnectClientSecret") is string clientSecret)
                options.ClientSecret = clientSecret.Replace(Constants.TenantToken, tc.Identifier);
        });

        builder.Services.ConfigureAllPerTenant<AuthenticationOptions, TTenantInfo>((options, tc) =>
        {
            if (GetPropertyWithValidValue(tc, "ChallengeScheme") is string challengeScheme)
                options.DefaultChallengeScheme = challengeScheme;
        });

        return builder;

        object? GetPropertyWithValidValue(TTenantInfo entity, string propertyName)
        {
            var property = entity.GetType().GetProperty(propertyName);
            return property?.GetValue(entity);
        }
    }

    /// <summary>
    /// Configures core functionality for per-tenant authentication.
    /// </summary>
    /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
    /// <param name="builder">MultiTenantBuilder instance.</param>
    /// <param name="config">Authentication options config</param>
    /// <returns>The <see cref="MultiTenantBuilder&lt;TTenantInfo&gt;"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithPerTenantAuthenticationCore<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder, Action<MultiTenantAuthenticationOptions>? config =
            null)
        where TTenantInfo : class, ITenantInfo, new()
    {
        config ??= _ => { };
        builder.Services.Configure(config);

        // We need to "decorate" IAuthenticationService so callbacks so that
        // remote authentication can get the tenant from the authentication
        // properties in the state parameter.
        if (builder.Services.All(s => s.ServiceType != typeof(IAuthenticationService)))
            throw new MultiTenantException(
                "WithPerTenantAuthenticationCore() must be called after AddAuthentication().");
        builder.Services.DecorateService<IAuthenticationService, MultiTenantAuthenticationService<TTenantInfo>>();

        // We need to "decorate" IAuthenticationScheme provider.
        builder.Services.DecorateService<IAuthenticationSchemeProvider, MultiTenantAuthenticationSchemeProvider>();

        return builder;
    }

    /// <summary>
    /// Adds and configures a SessionStrategy to the application.
    /// </summary>
    /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
    /// <param name="builder">MultiTenantBuilder instance.</param>
    /// <returns>The <see cref="MultiTenantBuilder&lt;TTenantInfo&gt;"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithSessionStrategy<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder)
        where TTenantInfo : class, ITenantInfo, new()
        => builder.WithStrategy<SessionStrategy>(ServiceLifetime.Singleton, Constants.TenantToken);

    /// <summary>
    /// Adds and configures a SessionStrategy to the application.
    /// </summary>
    /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
    /// <param name="builder">MultiTenantBuilder instance.</param>
    /// <param name="tenantKey">The session key to use.</param>
    /// <returns>The <see cref="MultiTenantBuilder&lt;TTenantInfo&gt;"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithSessionStrategy<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder, string tenantKey)
        where TTenantInfo : class, ITenantInfo, new()
        => builder.WithStrategy<SessionStrategy>(ServiceLifetime.Singleton, tenantKey);

    /// <summary>
    /// Adds and configures a RemoteAuthenticationCallbackStrategy to the application.
    /// </summary>
    /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
    /// <returns>The <see cref="MultiTenantBuilder&lt;TTenantInfo&gt;"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithRemoteAuthenticationCallbackStrategy<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder)
        where TTenantInfo : class, ITenantInfo, new()
    {
        return builder.WithStrategy<RemoteAuthenticationCallbackStrategy>(ServiceLifetime.Singleton);
    }

    /// <summary>
    /// Adds and configures a BasePathStrategy to the application.
    /// </summary>
    /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
    /// <returns>The same MultiTenantBuilder passed into the method.></returns>
    public static MultiTenantBuilder<TTenantInfo> WithBasePathStrategy<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder)
        where TTenantInfo : class, ITenantInfo, new() => WithBasePathStrategy(builder,
        configureOptions => { configureOptions.RebaseAspNetCorePathBase = false; });

    /// <summary>
    /// Adds and configures a BasePathStrategy to the application.
    /// </summary>
    /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
    /// <returns>The same MultiTenantBuilder passed into the method.></returns>
    public static MultiTenantBuilder<TTenantInfo> WithBasePathStrategy<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder, Action<BasePathStrategyOptions> configureOptions)
        where TTenantInfo : class, ITenantInfo, new()
    {
        builder.Services.Configure(configureOptions);
        builder.Services.Configure<MultiTenantOptions<TTenantInfo>>(options =>
        {
            var origOnTenantResolved = options.Events.OnTenantResolveCompleted;
            options.Events.OnTenantResolveCompleted = resolutionCompletedContext =>
            {
                if (resolutionCompletedContext.MultiTenantContext.StrategyInfo?.StrategyType == typeof(BasePathStrategy) &&
                    resolutionCompletedContext.Context is HttpContext httpContext &&
                    httpContext.RequestServices.GetRequiredService<IOptions<BasePathStrategyOptions>>().Value
                        .RebaseAspNetCorePathBase)
                {
                    httpContext.Request.Path.StartsWithSegments($"/{resolutionCompletedContext.MultiTenantContext.TenantInfo?.Identifier}",
                        out var matched, out var
                            newPath);
                    httpContext.Request.PathBase = httpContext.Request.PathBase.Add(matched);
                    httpContext.Request.Path = newPath;
                }

                return origOnTenantResolved(resolutionCompletedContext);
            };
        });

        return builder.WithStrategy<BasePathStrategy>(ServiceLifetime.Singleton);
    }

    /// <summary>
    /// Adds and configures a RouteStrategy with a route parameter Constants.TenantToken to the application.
    /// </summary>
    /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
    /// <returns>The <see cref="MultiTenantBuilder&lt;TTenantInfo&gt;"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithRouteStrategy<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder)
        where TTenantInfo : class, ITenantInfo, new()
        => builder.WithRouteStrategy(Constants.TenantToken);

    /// <summary>
    /// Adds and configures a RouteStrategy to the application.
    /// </summary>
    /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
    /// <param name="builder">MultiTenantBuilder instance.</param>
    /// <param name="tenantParam">The name of the route parameter used to determine the tenant identifier.</param>
    /// <returns>The <see cref="MultiTenantBuilder&lt;TTenantInfo&gt;"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithRouteStrategy<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder, string tenantParam)
        where TTenantInfo : class, ITenantInfo, new()
    {
        if (string.IsNullOrWhiteSpace(tenantParam))
        {
            throw new ArgumentException("Invalid value for \"tenantParam\"", nameof(tenantParam));
        }

        return builder.WithStrategy<RouteStrategy>(ServiceLifetime.Singleton, tenantParam);
    }

    /// <summary>
    /// Adds and configures a HostStrategy with template "__tenant__.*" to the application.
    /// </summary>
    /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
    /// <returns>The <see cref="MultiTenantBuilder&lt;TTenantInfo&gt;"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithHostStrategy<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder)
        where TTenantInfo : class, ITenantInfo, new()
        => builder.WithHostStrategy($"{Constants.TenantToken}.*");

    /// <summary>
    /// Adds and configures a HostStrategy to the application.
    /// </summary>
    /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
    /// <param name="builder">MultiTenantBuilder instance.</param>
    /// <param name="template">The template for determining the tenant identifier in the host.</param>
    /// <returns>The <see cref="MultiTenantBuilder&lt;TTenantInfo&gt;"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithHostStrategy<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder, string template)
        where TTenantInfo : class, ITenantInfo, new()
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            throw new ArgumentException("Invalid value for \"template\"", nameof(template));
        }

        return builder.WithStrategy<HostStrategy>(ServiceLifetime.Singleton, template);
    }

    /// <summary>
    /// Adds and configures a ClaimStrategy for claim name "__tenant__" to the application. Uses the default authentication handler scheme.
    /// </summary>
    /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
    /// <returns>The <see cref="MultiTenantBuilder&lt;TTenantInfo&gt;"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithClaimStrategy<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder) where TTenantInfo : class, ITenantInfo, new()
    {
        return builder.WithClaimStrategy(Constants.TenantToken);
    }

    /// <summary>
    /// Adds and configures a ClaimStrategy to the application. Uses the default authentication handler scheme.
    /// </summary>
    /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
    /// <param name="builder">MultiTenantBuilder instance.</param>
    /// <param name="tenantKey">Claim name for determining the tenant identifier.</param>
    /// <returns>The <see cref="MultiTenantBuilder&lt;TTenantInfo&gt;"/> so that additional calls can be chained.</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public static MultiTenantBuilder<TTenantInfo> WithClaimStrategy<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder, string tenantKey)
        where TTenantInfo : class, ITenantInfo, new()
    {
        BypassSessionPrincipalValidation(builder);
        return builder.WithStrategy<ClaimStrategy>(ServiceLifetime.Singleton, tenantKey);
    }

    /// <summary>
    /// Adds and configures a ClaimStrategy to the application.
    /// </summary>
    /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
    /// <param name="builder">MultiTenantBuilder instance.</param>
    /// <param name="tenantKey">Claim name for determining the tenant identifier.</param>
    /// <param name="authenticationScheme">The authentication scheme to check for claims.</param>
    /// <returns>The <see cref="MultiTenantBuilder&lt;TTenantInfo&gt;"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithClaimStrategy<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder, string tenantKey, string authenticationScheme)
        where TTenantInfo : class, ITenantInfo, new()
    {
        BypassSessionPrincipalValidation(builder);
        return builder.WithStrategy<ClaimStrategy>(ServiceLifetime.Singleton, tenantKey, authenticationScheme);
    }
    
    /// <summary>
    /// Adds and configures an HttpContext delegate strategy to the application.
    /// </summary>
    /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
    /// <param name="builder">MultiTenantBuilder instance.</param>
    /// <param name="doStrategy">The delegate to execute to determine the tenant identifier.</param>
    /// <returns>The <see cref="MultiTenantBuilder&lt;TTenantInfo&gt;"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithHttpContextStrategy<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder, Func<HttpContext, Task<string?>> doStrategy)
        where TTenantInfo : class, ITenantInfo, new()
    {
        return builder.WithDelegateStrategy<HttpContext, TTenantInfo>(doStrategy);
    }

    private static void BypassSessionPrincipalValidation<TTenantInfo>(
        MultiTenantBuilder<TTenantInfo> builder)
        where TTenantInfo : class, ITenantInfo, new()
    {
        builder.Services.ConfigureAll<CookieAuthenticationOptions>(options =>
        {
            var origOnValidatePrincipal = options.Events.OnValidatePrincipal;
            options.Events.OnValidatePrincipal = async context =>
            {
                // Skip if bypass set (e.g. ClaimStrategy in effect)
                if (context.HttpContext.Items.Keys.Contains(
                        $"{Constants.TenantToken}__bypass_validate_principal__"))
                    return;

                await origOnValidatePrincipal(context);
            };
        });
    }

    /// <summary>
    /// Adds and configures a HeaderStrategy with using HTTP header key "__tenant__" to the application.
    /// </summary>
    /// <returns>The <see cref="MultiTenantBuilder&lt;TTenantInfo&gt;"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithHeaderStrategy<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder) where TTenantInfo : class, ITenantInfo, new()
    {
        return builder.WithStrategy<HeaderStrategy>(ServiceLifetime.Singleton, Constants.TenantToken);
    }

    /// <summary>
    /// Adds and configures a HeaderStrategy to the application with a custom HTTP header key.
    /// </summary>
    /// <param name="builder">MultiTenantBuilder instance.</param>
    /// <param name="tenantKey">The HTTP header key for determining the tenant identifier in the request.</param>
    /// <returns>The <see cref="MultiTenantBuilder&lt;TTenantInfo&gt;"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithHeaderStrategy<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder, string tenantKey)
        where TTenantInfo : class, ITenantInfo, new()
    {
        return builder.WithStrategy<HeaderStrategy>(ServiceLifetime.Singleton, tenantKey);
    }
}