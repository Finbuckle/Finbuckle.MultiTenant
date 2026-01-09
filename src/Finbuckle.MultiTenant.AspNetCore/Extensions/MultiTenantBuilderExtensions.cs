// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.AspNetCore.Authentication;
using Finbuckle.MultiTenant.AspNetCore.Options;
using Finbuckle.MultiTenant.AspNetCore.Strategies;
using Finbuckle.MultiTenant.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Finbuckle.MultiTenant.AspNetCore.Extensions;

/// <summary>
/// Provides builder methods for Finbuckle.MultiTenant services and configuration.
/// </summary>
public static class MultiTenantBuilderExtensions
{
    /// <summary>
    /// Configures a callback that determines when endpoints should be short circuited
    /// during multi-tenant resolution.
    /// </summary>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    /// <param name="builder">The <see cref="MultiTenantBuilder{TTenantInfo}"/> instance.</param>
    /// <param name="configureOptions">The short circuit options to configure.</param>
    /// <returns>The <see cref="MultiTenantBuilder{TTenantInfo}"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> ShortCircuitWhen<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder, Action<ShortCircuitWhenOptions> configureOptions)
        where TTenantInfo : ITenantInfo
    {
        builder.Services.Configure(configureOptions);
        return builder;
    }

    /// <summary>
    /// Configures endpoints to be short circuited during multi-tenant resolution when
    /// no tenant was resolved.
    /// </summary>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    /// <param name="builder">The <see cref="MultiTenantBuilder{TTenantInfo}"/> instance.</param>
    /// <returns>The <see cref="MultiTenantBuilder{TTenantInfo}"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> ShortCircuitWhenTenantNotResolved<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder)
        where TTenantInfo : ITenantInfo
    {
        return builder.ShortCircuitWhen(config => { config.Predicate = context => !context.IsResolved; });
    }

    /// <summary>
    /// Configures endpoints to be short circuited during multi-tenant resolution when
    /// no tenant was resolved.
    /// </summary>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    /// <param name="builder">The <see cref="MultiTenantBuilder{TTenantInfo}"/> instance.</param>
    /// <param name="redirectTo">A <see cref="Uri"/> to redirect the request to, if short circuited.</param>
    /// <returns>The <see cref="MultiTenantBuilder{TTenantInfo}"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> ShortCircuitWhenTenantNotResolved<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder, Uri redirectTo)
        where TTenantInfo : ITenantInfo
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
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    /// <param name="builder">The <see cref="MultiTenantBuilder{TTenantInfo}"/> instance.</param>
    /// <returns>The <see cref="MultiTenantBuilder{TTenantInfo}"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithPerTenantAuthentication<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder)
        where TTenantInfo : ITenantInfo
    {
        return builder.WithPerTenantAuthentication(_ => { });
    }

    /// <summary>
    /// Configures per-tenant authentication behavior.
    /// </summary>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    /// <param name="builder"><see cref="MultiTenantBuilder{TTenantInfo}"/> instance.</param>
    /// <param name="config">Authentication options config.</param>
    /// <returns>The <see cref="MultiTenantBuilder{TTenantInfo}"/> so that additional calls can be chained.</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public static MultiTenantBuilder<TTenantInfo> WithPerTenantAuthentication<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder, Action<MultiTenantAuthenticationOptions> config)
        where TTenantInfo : ITenantInfo
    {
        builder.WithPerTenantAuthenticationCore(config);
        builder.WithPerTenantAuthenticationConventions();
        builder.WithRemoteAuthenticationCallbackStrategy();

        return builder;
    }

    /// <summary>
    /// Configures conventional functionality for per-tenant authentication.
    /// </summary>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    /// <param name="builder">The <see cref="MultiTenantBuilder{TTenantInfo}"/> instance.</param>
    /// <returns>The <see cref="MultiTenantBuilder{TTenantInfo}"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithPerTenantAuthenticationConventions<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder)
        where TTenantInfo : ITenantInfo
    {
        // Set events to set and validate tenant for each cookie based authentication principal.
        builder.Services.ConfigureAll<CookieAuthenticationOptions>(options =>
        {
            // Validate that claimed tenant matches current tenant.
            var origOnValidatePrincipal = options.Events.OnValidatePrincipal;
            options.Events.OnValidatePrincipal = async context =>
            {
                // Skip if bypass set (e.g. ClaimsStrategy in effect)
                if (context.HttpContext.Items.ContainsKey($"{Constants.TenantToken}__bypass_validate_principal__"))
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

                await origOnValidatePrincipal(context).ConfigureAwait(false);
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
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    /// <param name="builder"><see cref="MultiTenantBuilder{TTenantInfo}"/> instance.</param>
    /// <param name="config">Authentication options config.</param>
    /// <returns>The <see cref="MultiTenantBuilder{TTenantInfo}"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithPerTenantAuthenticationCore<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder, Action<MultiTenantAuthenticationOptions>? config =
            null)
        where TTenantInfo : ITenantInfo
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
    /// Adds and configures a <see cref="SessionStrategy"/> to the application.
    /// </summary>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    /// <param name="builder"><see cref="MultiTenantBuilder{TTenantInfo}"/> instance.</param>
    /// <returns>The <see cref="MultiTenantBuilder{TTenantInfo}"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithSessionStrategy<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder)
        where TTenantInfo : ITenantInfo
        => builder.WithStrategy<SessionStrategy>(ServiceLifetime.Singleton, Constants.TenantToken);

    /// <summary>
    /// Adds and configures a <see cref="SessionStrategy"/> to the application.
    /// </summary>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    /// <param name="builder"><see cref="MultiTenantBuilder{TTenantInfo}"/> instance.</param>
    /// <param name="tenantKey">The session key to use.</param>
    /// <returns>The <see cref="MultiTenantBuilder{TTenantInfo}"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithSessionStrategy<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder, string tenantKey)
        where TTenantInfo : ITenantInfo
        => builder.WithStrategy<SessionStrategy>(ServiceLifetime.Singleton, tenantKey);

    /// <summary>
    /// Adds and configures a <see cref="RemoteAuthenticationCallbackStrategy"/> to the application.
    /// </summary>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    /// <param name="builder">The <see cref="MultiTenantBuilder{TTenantInfo}"/> instance.</param>
    /// <returns>The <see cref="MultiTenantBuilder{TTenantInfo}"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithRemoteAuthenticationCallbackStrategy<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder)
        where TTenantInfo : ITenantInfo
    {
        return builder.WithStrategy<RemoteAuthenticationCallbackStrategy>(ServiceLifetime.Singleton);
    }

    /// <summary>
    /// Adds and configures a <see cref="BasePathStrategy"/> to the application.
    /// </summary>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    /// <param name="builder">The <see cref="MultiTenantBuilder{TTenantInfo}"/> instance.</param>
    /// <returns>The <see cref="MultiTenantBuilder{TTenantInfo}"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithBasePathStrategy<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder)
        where TTenantInfo : ITenantInfo => WithBasePathStrategy(builder,
        configureOptions => { configureOptions.RebaseAspNetCorePathBase = true; });

    /// <summary>
    /// Adds and configures a <see cref="BasePathStrategy"/> to the application.
    /// </summary>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    /// <param name="builder">The <see cref="MultiTenantBuilder{TTenantInfo}"/> instance.</param>
    /// <param name="configureOptions">Action to configure the <see cref="BasePathStrategyOptions"/>.</param>
    /// <returns>The <see cref="MultiTenantBuilder{TTenantInfo}"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithBasePathStrategy<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder, Action<BasePathStrategyOptions> configureOptions)
        where TTenantInfo : ITenantInfo
    {
        builder.Services.Configure(configureOptions);
        builder.Services.Configure<MultiTenantOptions<TTenantInfo>>(options =>
        {
            var origOnTenantResolved = options.Events.OnTenantResolveCompleted;
            options.Events.OnTenantResolveCompleted = resolutionCompletedContext =>
            {
                if (resolutionCompletedContext.MultiTenantContext.StrategyInfo?.StrategyType ==
                    typeof(BasePathStrategy) &&
                    resolutionCompletedContext.Context is HttpContext httpContext &&
                    httpContext.RequestServices.GetRequiredService<IOptions<BasePathStrategyOptions>>().Value
                        .RebaseAspNetCorePathBase)
                {
                    httpContext.Request.Path.StartsWithSegments(
                        $"/{resolutionCompletedContext.MultiTenantContext.TenantInfo?.Identifier}",
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
    /// Adds and configures a <see cref="RouteStrategy"/> with a route parameter <see cref="Abstractions.Constants.TenantToken"/> to the application.
    /// </summary>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    /// <param name="builder">The <see cref="MultiTenantBuilder{TTenantInfo}"/> instance.</param>
    /// <returns>The <see cref="MultiTenantBuilder{TTenantInfo}"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithRouteStrategy<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder)
        where TTenantInfo : ITenantInfo
        => builder.WithRouteStrategy(Constants.TenantToken, true);

    /// <summary>
    /// Adds and configures a <see cref="RouteStrategy"/> to the application.
    /// </summary>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    /// <param name="builder"><see cref="MultiTenantBuilder{TTenantInfo}"/> instance.</param>
    /// <param name="tenantParam">The name of the route parameter used to determine the tenant identifier.</param>
    /// <param name="useTenantAmbientRouteValue">If true, promotes the tenant route value from ambient to explicit values when generating links.</param>
    /// <returns>The <see cref="MultiTenantBuilder{TTenantInfo}"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithRouteStrategy<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder, string tenantParam, bool useTenantAmbientRouteValue)
        where TTenantInfo : ITenantInfo
    {
        if (string.IsNullOrWhiteSpace(tenantParam))
        {
            throw new ArgumentException("Invalid value for \"tenantParam\"", nameof(tenantParam));
        }
        
        if(useTenantAmbientRouteValue)
        {
            // Decorate LinkGenerator to promote tenant route value from ambient to explicit values.
            builder.Services.DecorateService<LinkGenerator, MultiTenantAmbientValueLinkGenerator>(new List<string>{tenantParam});
        }

        return builder.WithStrategy<RouteStrategy>(ServiceLifetime.Singleton, tenantParam);
    }

    /// <summary>
    /// Adds and configures a <see cref="HostStrategy"/> with template "__tenant__.*" to the application.
    /// </summary>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    /// <param name="builder">The <see cref="MultiTenantBuilder{TTenantInfo}"/> instance.</param>
    /// <returns>The <see cref="MultiTenantBuilder{TTenantInfo}"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithHostStrategy<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder)
        where TTenantInfo : ITenantInfo
        => builder.WithHostStrategy($"{Constants.TenantToken}.*");

    /// <summary>
    /// Adds and configures a <see cref="HostStrategy"/> to the application.
    /// </summary>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    /// <param name="builder"><see cref="MultiTenantBuilder{TTenantInfo}"/> instance.</param>
    /// <param name="template">The template for determining the tenant identifier in the host.</param>
    /// <returns>The <see cref="MultiTenantBuilder{TTenantInfo}"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithHostStrategy<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder, string template)
        where TTenantInfo : ITenantInfo
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            throw new ArgumentException("Invalid value for \"template\"", nameof(template));
        }

        return builder.WithStrategy<HostStrategy>(ServiceLifetime.Singleton, template);
    }

    /// <summary>
    /// Adds and configures a <see cref="ClaimStrategy"/> for claim name "__tenant__" to the application. Uses the default authentication handler scheme.
    /// </summary>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    /// <param name="builder">The <see cref="MultiTenantBuilder{TTenantInfo}"/> instance.</param>
    /// <returns>The <see cref="MultiTenantBuilder{TTenantInfo}"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithClaimStrategy<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder) where TTenantInfo : ITenantInfo
    {
        return builder.WithClaimStrategy(Constants.TenantToken);
    }

    /// <summary>
    /// Adds and configures a <see cref="ClaimStrategy"/> to the application. Uses the default authentication handler scheme.
    /// </summary>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    /// <param name="builder"><see cref="MultiTenantBuilder{TTenantInfo}"/> instance.</param>
    /// <param name="tenantKey">Claim name for determining the tenant identifier.</param>
    /// <returns>The <see cref="MultiTenantBuilder{TTenantInfo}"/> so that additional calls can be chained.</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public static MultiTenantBuilder<TTenantInfo> WithClaimStrategy<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder, string tenantKey)
        where TTenantInfo : ITenantInfo
    {
        BypassSessionPrincipalValidation(builder);
        return builder.WithStrategy<ClaimStrategy>(ServiceLifetime.Singleton, tenantKey);
    }

    /// <summary>
    /// Adds and configures a <see cref="ClaimStrategy"/> to the application.
    /// </summary>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    /// <param name="builder"><see cref="MultiTenantBuilder{TTenantInfo}"/> instance.</param>
    /// <param name="tenantKey">Claim name for determining the tenant identifier.</param>
    /// <param name="authenticationScheme">The authentication scheme to check for claims.</param>
    /// <returns>The <see cref="MultiTenantBuilder{TTenantInfo}"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithClaimStrategy<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder, string tenantKey, string authenticationScheme)
        where TTenantInfo : ITenantInfo
    {
        BypassSessionPrincipalValidation(builder);
        return builder.WithStrategy<ClaimStrategy>(ServiceLifetime.Singleton, tenantKey, authenticationScheme);
    }

    /// <summary>
    /// Adds and configures an <see cref="HttpContext"/> delegate strategy to the application.
    /// </summary>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    /// <param name="builder"><see cref="MultiTenantBuilder{TTenantInfo}"/> instance.</param>
    /// <param name="doStrategy">The delegate to execute to determine the tenant identifier.</param>
    /// <returns>The <see cref="MultiTenantBuilder{TTenantInfo}"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithHttpContextStrategy<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder, Func<HttpContext, Task<string?>> doStrategy)
        where TTenantInfo : ITenantInfo
    {
        return builder.WithDelegateStrategy(doStrategy);
    }

    private static void BypassSessionPrincipalValidation<TTenantInfo>(
        MultiTenantBuilder<TTenantInfo> builder)
        where TTenantInfo : ITenantInfo
    {
        builder.Services.ConfigureAll<CookieAuthenticationOptions>(options =>
        {
            var origOnValidatePrincipal = options.Events.OnValidatePrincipal;
            options.Events.OnValidatePrincipal = async context =>
            {
                // Skip if bypass set (e.g. ClaimStrategy in effect)
                if (context.HttpContext.Items.ContainsKey($"{Constants.TenantToken}__bypass_validate_principal__"))
                    return;

                await origOnValidatePrincipal(context).ConfigureAwait(false);
            };
        });
    }

    /// <summary>
    /// Adds and configures a <see cref="HeaderStrategy"/> using HTTP header key "__tenant__" to the application.
    /// </summary>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    /// <param name="builder">The <see cref="MultiTenantBuilder{TTenantInfo}"/> instance.</param>
    /// <returns>The <see cref="MultiTenantBuilder{TTenantInfo}"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithHeaderStrategy<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder) where TTenantInfo : ITenantInfo
    {
        return builder.WithStrategy<HeaderStrategy>(ServiceLifetime.Singleton, Constants.TenantToken);
    }

    /// <summary>
    /// Adds and configures a <see cref="HeaderStrategy"/> to the application with a custom HTTP header key.
    /// </summary>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    /// <param name="builder"><see cref="MultiTenantBuilder{TTenantInfo}"/> instance.</param>
    /// <param name="tenantKey">The HTTP header key for determining the tenant identifier in the request.</param>
    /// <returns>The <see cref="MultiTenantBuilder{TTenantInfo}"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithHeaderStrategy<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder, string tenantKey)
        where TTenantInfo : ITenantInfo
    {
        return builder.WithStrategy<HeaderStrategy>(ServiceLifetime.Singleton, tenantKey);
    }
}