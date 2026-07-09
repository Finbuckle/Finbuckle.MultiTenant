// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Finbuckle.MultiTenant.Extensions;

/// <summary>
/// <see cref="IServiceCollection"/> extension methods for Finbuckle.MultiTenant.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configure Finbuckle.MultiTenant services for the application.
    /// </summary>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> instance the extension method applies to.</param>
    /// <param name="config">An action to configure the <see cref="MultiTenantOptions{TTenantInfo}"/> instance.</param>
    /// <returns>A new instance of <see cref="MultiTenantBuilder{TTenantInfo}"/>.</returns>
    public static MultiTenantBuilder<TTenantInfo> AddMultiTenant<TTenantInfo>(this IServiceCollection services,
        Action<MultiTenantOptions<TTenantInfo>> config)
        where TTenantInfo : ITenantInfo
    {
        services.AddScoped<ITenantResolver<TTenantInfo>, TenantResolver<TTenantInfo>>();
        services.AddScoped<ITenantResolver>(sp => sp.GetRequiredService<ITenantResolver<TTenantInfo>>());
        services.AddScoped<TenantManager<TTenantInfo>>();
        
        services.AddScoped<ITenantContext<TTenantInfo>>(_ => new TenantContext<TTenantInfo>());
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<ITenantContext<TTenantInfo>>());

        services.Configure<MultiTenantOptions<TTenantInfo>>(options => options.TenantInfoType = typeof(TTenantInfo));
        services.Configure(config);

        return new MultiTenantBuilder<TTenantInfo>(services);
    }

    /// <summary>
    /// Configure Finbuckle.MultiTenant services for the application.
    /// </summary>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> instance the extension method applies to.</param>
    /// <returns>A new instance of <see cref="MultiTenantBuilder{TTenantInfo}"/>.</returns>
    public static MultiTenantBuilder<TTenantInfo> AddMultiTenant<TTenantInfo>(this IServiceCollection services)
        where TTenantInfo : ITenantInfo
    {
        return services.AddMultiTenant<TTenantInfo>(_ => { });
    }


    /// <summary>
    /// Decorates an existing service registration with a new implementation that wraps the original.
    /// The decorator is registered with the same lifetime as the existing registration.
    /// </summary>
    /// <typeparam name="TService">The service type to decorate.</typeparam>
    /// <typeparam name="TImpl">The decorator implementation type.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="parameters">Additional parameters to pass to the decorator constructor after the inner service.</param>
    /// <returns><c>true</c> if the service was successfully decorated.</returns>
    /// <exception cref="ArgumentException">Thrown when no service of type <typeparamref name="TService"/> is found.</exception>
    /// <exception cref="Exception">Thrown when the service cannot be instantiated.</exception>
    public static bool DecorateService<TService, TImpl>(this IServiceCollection services, params object[] parameters) =>
        services.DecorateService<TService, TImpl>(null, parameters);

    /// <summary>
    /// Decorates an existing service registration with a new implementation that wraps the original,
    /// registering the decorator with the specified <paramref name="lifetime"/> instead of the existing
    /// registration's lifetime. This is safe as long as <paramref name="lifetime"/> is the same as or
    /// shorter-lived than the lifetime(s) of the existing registration(s) being decorated (e.g. decorating
    /// a Singleton with a Scoped decorator is safe; the reverse is not).
    /// </summary>
    /// <typeparam name="TService">The service type to decorate.</typeparam>
    /// <typeparam name="TImpl">The decorator implementation type.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="lifetime">The <see cref="ServiceLifetime"/> to register the decorator with.</param>
    /// <param name="parameters">Additional parameters to pass to the decorator constructor after the inner service.</param>
    /// <returns><c>true</c> if the service was successfully decorated.</returns>
    /// <exception cref="ArgumentException">Thrown when no service of type <typeparamref name="TService"/> is found.</exception>
    /// <exception cref="Exception">Thrown when the service cannot be instantiated.</exception>
    public static bool DecorateService<TService, TImpl>(this IServiceCollection services, ServiceLifetime lifetime,
        params object[] parameters) =>
        services.DecorateService<TService, TImpl>((ServiceLifetime?)lifetime, parameters);

    private static bool DecorateService<TService, TImpl>(this IServiceCollection services, ServiceLifetime? lifetimeOverride,
        object[] parameters)
    {
        var existingServices = services.Where(s => s.ServiceType == typeof(TService)).ToList();
        if (existingServices.Count == 0)
            throw new ArgumentException($"No service of type {typeof(TService).Name} found.");

        foreach (var existingService in existingServices)
        {
            var lifetime = lifetimeOverride ?? existingService.Lifetime;
            ServiceDescriptor? newService;
            if (existingService.ImplementationType is not null)
            {
                newService = new ServiceDescriptor(existingService.ServiceType,
                    sp =>
                    {
                        TService inner =
                            (TService)ActivatorUtilities.CreateInstance(sp, existingService.ImplementationType);

                        if (inner is null)
                            throw new ArgumentException(
                                $"Unable to instantiate decorated type via implementation type {existingService.ImplementationType.Name}.");

                        var parameters2 = new object[parameters.Length + 1];
                        Array.Copy(parameters, 0, parameters2, 1, parameters.Length);
                        parameters2[0] = inner;

                        return ActivatorUtilities.CreateInstance<TImpl>(sp, parameters2)!;
                    },
                    lifetime);
            }
            else if (existingService.ImplementationInstance is not null)
            {
                newService = new ServiceDescriptor(existingService.ServiceType,
                    sp =>
                    {
                        TService inner = (TService)existingService.ImplementationInstance;
                        if (inner is null)
                            throw new ArgumentException(
                                $"Unable to instantiate decorated type via implementation instance of type {existingService.ImplementationInstance.GetType().Name}.");

                        var parameters2 = new object[parameters.Length + 1];
                        Array.Copy(parameters, 0, parameters2, 1, parameters.Length);
                        parameters2[0] = inner;

                        return ActivatorUtilities.CreateInstance<TImpl>(sp, parameters2)!;
                    },
                    lifetime);
            }
            else if (existingService.ImplementationFactory is not null)
            {
                newService = new ServiceDescriptor(existingService.ServiceType,
                    sp =>
                    {
                        TService inner = (TService)existingService.ImplementationFactory(sp);
                        if (inner is null)
                            throw new ArgumentException(
                                $"Unable to instantiate decorated type via implementation factory for type {existingService.ServiceType}.");

                        var parameters2 = new object[parameters.Length + 1];
                        Array.Copy(parameters, 0, parameters2, 1, parameters.Length);
                        parameters2[0] = inner;

                        return ActivatorUtilities.CreateInstance<TImpl>(sp, parameters2)!;
                    },
                    lifetime);
            }
            else
            {
                throw new ArgumentException(
                    "Unable to instantiate decorated type.");
            }

            services.Remove(existingService);
            services.Add(newService);
        }

        return true;
    }

    /// <summary>
    /// Registers an action used to configure an options type per tenant.
    /// </summary>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    /// <typeparam name="TTenantInfo">The TTenantInfo derived type.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="name">The name of the options instance.</param>
    /// <param name="configureOptions">The action used to configure the options.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection ConfigurePerTenant<TOptions, TTenantInfo>(
        this IServiceCollection services,
        string? name, Action<TOptions, TTenantInfo> configureOptions)
        where TOptions : class
        where TTenantInfo : ITenantInfo
    {
        ConfigurePerTenantReqs<TOptions>(services);

        services.AddTransient<IConfigureOptions<TOptions>>(sp =>
            new ConfigureNamedOptions<TOptions, ITenantContext<TTenantInfo>>(
                name,
                sp.GetRequiredService<ITenantContext<TTenantInfo>>(),
                (options, tenantContext) =>
                {
                    var tenantInfo = tenantContext.TenantInfo;
                    if (tenantInfo is not null)
                        configureOptions(options, tenantInfo);
                }));

        return services;
    }

    /// <summary>
    /// Registers an action used to configure an options type per tenant.
    /// </summary>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    /// <typeparam name="TTenantInfo">The TTenantInfo derived type.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configureOptions">The action used to configure the options.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection ConfigurePerTenant<TOptions, TTenantInfo>(
        this IServiceCollection services,
        Action<TOptions, TTenantInfo> configureOptions)
        where TOptions : class
        where TTenantInfo : ITenantInfo
    {
        return services.ConfigurePerTenant(Microsoft.Extensions.Options.Options.DefaultName, configureOptions);
    }

    /// <summary>
    /// Registers an action used to configure all instances of an options type per tenant.
    /// </summary>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    /// <typeparam name="TTenantInfo">The TTenantInfo derived type.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configureOptions">The action used to configure the options.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection ConfigureAllPerTenant<TOptions, TTenantInfo>(
        this IServiceCollection services,
        Action<TOptions, TTenantInfo> configureOptions)
        where TOptions : class
        where TTenantInfo : ITenantInfo
    {
        return services.ConfigurePerTenant(null, configureOptions);
    }

    /// <summary>
    /// Registers an action used to post-configure an options type per tenant.
    /// </summary>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    /// <typeparam name="TTenantInfo">The TTenantInfo derived type.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="name">The name of the options instance.</param>
    /// <param name="configureOptions">The action used to configure the options.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection PostConfigurePerTenant<TOptions, TTenantInfo>(
        this IServiceCollection services,
        string? name, Action<TOptions, TTenantInfo> configureOptions)
        where TOptions : class
        where TTenantInfo : ITenantInfo
    {
        ConfigurePerTenantReqs<TOptions>(services);

        services.AddTransient<IPostConfigureOptions<TOptions>>(sp =>
            new PostConfigureOptions<TOptions, ITenantContext<TTenantInfo>>(
                name,
                sp.GetRequiredService<ITenantContext<TTenantInfo>>(),
                (options, tenantContext) =>
                {
                    var tenantInfo = tenantContext.TenantInfo;
                    if (tenantInfo is not null)
                        configureOptions(options, tenantInfo);
                }));

        return services;
    }

    /// <summary>
    /// Registers an action used to post-configure an options type per tenant.
    /// </summary>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    /// <typeparam name="TTenantInfo">The TTenantInfo derived type.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configureOptions">The action used to configure the options.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection PostConfigurePerTenant<TOptions, TTenantInfo>(
        this IServiceCollection services,
        Action<TOptions, TTenantInfo> configureOptions)
        where TOptions : class
        where TTenantInfo : ITenantInfo
    {
        return services.PostConfigurePerTenant(Microsoft.Extensions.Options.Options.DefaultName, configureOptions);
    }

    /// <summary>
    /// Registers an action used to post-configure all instances of an options type per tenant.
    /// </summary>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    /// <typeparam name="TTenantInfo">The TTenantInfo derived type.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configureOptions">The action used to configure the options.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection PostConfigureAllPerTenant<TOptions, TTenantInfo>(
        this IServiceCollection services,
        Action<TOptions, TTenantInfo> configureOptions)
        where TOptions : class
        where TTenantInfo : ITenantInfo
    {
        return services.PostConfigurePerTenant(null, configureOptions);
    }

    /// <summary>
    /// Configures the required services for per-tenant options support.
    /// </summary>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    internal static void ConfigurePerTenantReqs<TOptions>(IServiceCollection services)
        where TOptions : class
    {
        ArgumentNullException.ThrowIfNull(services);
        const string optionsManagerCacheKey = "finbuckle.options.manager.cache";

        // Required infrastructure.
        services.AddOptions();

        // Infrastructure for IOptionsMonitor<TOptions>
        services.TryAddSingleton<MultiTenantOptionsCache<TOptions>>();
        services.TryAddSingleton<MultiTenantOptionsChangeTokenHub<TOptions>>();
        services.TryAddScoped<IOptionsMonitor<TOptions>, MultiTenantOptionsMonitor<TOptions>>();
        
        // Infrastructure for IOptionsSnapshot<TOptions>
        services.TryAddScoped<IOptionsSnapshot<TOptions>>(BuildOptionsSnapshotManagerWithPrivateCache);
        
        // Infrastructure for IOptions<TOptions>
        services.TryAddKeyedSingleton<MultiTenantOptionsCache<TOptions>>(optionsManagerCacheKey);
        services.TryAddScoped<IOptions<TOptions>>(BuildOptionsManagerWithSingletonCache);
        
        return;

        MultiTenantOptionsManager<TOptions> BuildOptionsSnapshotManagerWithPrivateCache(IServiceProvider sp)
        {
            return ActivatorUtilities.CreateInstance<MultiTenantOptionsManager<TOptions>>(sp,
                new MultiTenantOptionsCache<TOptions>());
        }

        MultiTenantOptionsManager<TOptions> BuildOptionsManagerWithSingletonCache(IServiceProvider sp)
        {
            return ActivatorUtilities.CreateInstance<MultiTenantOptionsManager<TOptions>>(sp,
                sp.GetRequiredKeyedService<MultiTenantOptionsCache<TOptions>>(optionsManagerCacheKey));
        }
    }
}
