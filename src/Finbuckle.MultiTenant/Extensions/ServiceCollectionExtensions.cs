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

        services.AddSingleton<IMultiTenantContextAccessor<TTenantInfo>,
            AsyncLocalMultiTenantContextAccessor<TTenantInfo>>();
        services.AddSingleton<IMultiTenantContextAccessor>(sp =>
            sp.GetRequiredService<IMultiTenantContextAccessor<TTenantInfo>>());

        services.AddSingleton<IMultiTenantContextSetter>(sp =>
            (IMultiTenantContextSetter)sp.GetRequiredService<IMultiTenantContextAccessor>());

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
    /// </summary>
    /// <typeparam name="TService">The service type to decorate.</typeparam>
    /// <typeparam name="TImpl">The decorator implementation type.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="parameters">Additional parameters to pass to the decorator constructor after the inner service.</param>
    /// <returns><c>true</c> if the service was successfully decorated.</returns>
    /// <exception cref="ArgumentException">Thrown when no service of type <typeparamref name="TService"/> is found.</exception>
    /// <exception cref="Exception">Thrown when the service cannot be instantiated.</exception>
    public static bool DecorateService<TService, TImpl>(this IServiceCollection services, params object[] parameters)
    {
        var existingServices = services.Where(s => s.ServiceType == typeof(TService)).ToList();
        if (existingServices.Count == 0)
            throw new ArgumentException($"No service of type {typeof(TService).Name} found.");

        foreach (var existingService in existingServices)
        {
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
                    existingService.Lifetime);
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
                    existingService.Lifetime);
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
                    existingService.Lifetime);
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
    /// Registers an action used to configure a particular type of options per tenant.
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
            new ConfigureNamedOptions<TOptions, IMultiTenantContextAccessor<TTenantInfo>>(
                name,
                sp.GetRequiredService<IMultiTenantContextAccessor<TTenantInfo>>(),
                (options, mtcAccessor) =>
                {
                    var tenantInfo = mtcAccessor.MultiTenantContext.TenantInfo;
                    if (tenantInfo is not null)
                        configureOptions(options, tenantInfo);
                }));

        return services;
    }

    /// <summary>
    /// Registers an action used to configure a particular type of options.
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
    /// Registers an action used to configure all instances of a particular type of options per tenant.
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
    /// Registers a post configure action used to configure a particular type of options per tenant.
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
            new PostConfigureOptions<TOptions, IMultiTenantContextAccessor<TTenantInfo>>(
                name,
                sp.GetRequiredService<IMultiTenantContextAccessor<TTenantInfo>>(),
                (options, mtcAccessor) =>
                {
                    var tenantInfo = mtcAccessor.MultiTenantContext.TenantInfo;
                    if (tenantInfo is not null)
                        configureOptions(options, tenantInfo);
                }));

        return services;
    }

    /// <summary>
    /// Registers a post configure action used to configure a particular type of options per tenant.
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
    /// Registers a post configure action used to configure all instances of a particular type of options per tenant.
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

        // Required infrastructure.
        services.AddOptions();

        // TODO: Add check for success
        services.TryAddSingleton<IOptionsMonitorCache<TOptions>, MultiTenantOptionsCache<TOptions>>();
        services.TryAddScoped<IOptionsSnapshot<TOptions>>(BuildOptionsManager);
        services.TryAddSingleton<IOptions<TOptions>>(BuildOptionsManager);
        return;

        MultiTenantOptionsManager<TOptions> BuildOptionsManager(IServiceProvider sp)
        {
            IOptionsMonitorCache<TOptions> cache =
                ActivatorUtilities.CreateInstance<MultiTenantOptionsCache<TOptions>>(sp);
            return ActivatorUtilities.CreateInstance<MultiTenantOptionsManager<TOptions>>(sp, cache);
        }
    }
}