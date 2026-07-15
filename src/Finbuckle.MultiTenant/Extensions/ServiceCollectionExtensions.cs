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

        services.AddSingleton<AmbientTenantContext<TTenantInfo>>(new AmbientTenantContext<TTenantInfo>());
        services.AddSingleton<ITenantScopeProvider>(sp => sp.GetRequiredService<AmbientTenantContext<TTenantInfo>>());
        services.AddSingleton<ITenantContext<TTenantInfo>>(sp => sp.GetRequiredService<AmbientTenantContext<TTenantInfo>>());
        services.AddSingleton<ITenantContext>(sp => sp.GetRequiredService<AmbientTenantContext<TTenantInfo>>());

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
    /// Decorates existing unkeyed service registrations with a new implementation that wraps the original.
    /// </summary>
    /// <typeparam name="TService">The service type to decorate.</typeparam>
    /// <typeparam name="TImpl">The decorator implementation type.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="parameters">Additional parameters to pass to the decorator constructor after the inner service.</param>
    /// <returns><c>true</c> if the service was successfully decorated.</returns>
    /// <exception cref="ArgumentException">Thrown when no service of type <typeparamref name="TService"/> is found.</exception>
    /// <remarks>
    /// Keyed registrations are not changed. Calling this method repeatedly stacks decorators around the existing
    /// registration. A decorator that wraps a disposable inner service should forward disposal as appropriate.
    /// </remarks>
    public static bool DecorateService<TService, TImpl>(this IServiceCollection services, params object[] parameters)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(parameters);

        if (!typeof(TService).IsAssignableFrom(typeof(TImpl)))
            throw new ArgumentException(
                $"Decorator type {typeof(TImpl).Name} must implement or inherit from {typeof(TService).Name}.",
                nameof(TImpl));

        var parametersCopy = parameters.ToArray();
        var existingServices = services.Select((descriptor, index) => (descriptor, index))
            .Where(item => item.descriptor.ServiceType == typeof(TService) && !item.descriptor.IsKeyedService)
            .ToList();
        if (existingServices.Count == 0)
            throw new ArgumentException($"No service of type {typeof(TService).Name} found.");

        var replacements = existingServices.Select(item =>
            (item.index, descriptor: CreateDecoratedDescriptor<TService, TImpl>(item.descriptor, parametersCopy)))
            .ToList();

        foreach (var replacement in replacements)
            services[replacement.index] = replacement.descriptor;

        return true;
    }

    private static ServiceDescriptor CreateDecoratedDescriptor<TService, TImpl>(
        ServiceDescriptor existingService, object[] parameters)
    {
        Func<IServiceProvider, object?> createInner;

        if (existingService.ImplementationType is not null)
        {
            var implementationType = existingService.ImplementationType;
            createInner = sp => ActivatorUtilities.CreateInstance(sp, implementationType);
        }
        else if (existingService.ImplementationInstance is not null)
        {
            var implementationInstance = existingService.ImplementationInstance;
            createInner = _ => implementationInstance;
        }
        else if (existingService.ImplementationFactory is not null)
        {
            createInner = existingService.ImplementationFactory;
        }
        else
        {
            throw new ArgumentException("Unable to instantiate decorated type.");
        }

        return new ServiceDescriptor(existingService.ServiceType,
            sp =>
            {
                var innerObject = createInner(sp);
                if (innerObject is not TService inner)
                    throw new ArgumentException(
                        $"Unable to instantiate decorated service of type {typeof(TService).Name}.");

                var constructorParameters = new object[parameters.Length + 1];
                constructorParameters[0] = inner;
                Array.Copy(parameters, 0, constructorParameters, 1, parameters.Length);

                return ActivatorUtilities.CreateInstance<TImpl>(sp, constructorParameters)!;
            },
            existingService.Lifetime);
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
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

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
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

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

        // Per-tenant options require these closed registrations. Leave the standard open generic
        // registrations in place for all other options types.
        services.RemoveAll<IOptionsMonitorCache<TOptions>>();
        services.RemoveAll<IOptionsSnapshot<TOptions>>();
        services.RemoveAll<IOptions<TOptions>>();
        services.AddSingleton<IOptionsMonitorCache<TOptions>, MultiTenantOptionsCache<TOptions>>();
        services.AddScoped<IOptionsSnapshot<TOptions>>(BuildOptionsManager);
        services.AddSingleton<IOptions<TOptions>>(BuildOptionsManager);
        return;

        MultiTenantOptionsManager<TOptions> BuildOptionsManager(IServiceProvider sp)
        {
            IOptionsMonitorCache<TOptions> cache =
                ActivatorUtilities.CreateInstance<MultiTenantOptionsCache<TOptions>>(sp);
            return ActivatorUtilities.CreateInstance<MultiTenantOptionsManager<TOptions>>(sp, cache);
        }
    }
}
