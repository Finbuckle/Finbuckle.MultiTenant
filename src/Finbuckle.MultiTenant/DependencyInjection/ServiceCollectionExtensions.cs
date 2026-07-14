// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Diagnostics.CodeAnalysis;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Internal;
using Finbuckle.MultiTenant.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

// ReSharper disable once CheckNamespace
namespace Finbuckle.MultiTenant;

/// <summary>
/// IServiceCollection extension methods for Finbuckle.MultiTenant.
/// </summary>
[SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
public static class FinbuckleServiceCollectionExtensions
{
    /// <summary>
    /// Configure Finbuckle.MultiTenant services for the application.
    /// </summary>
    /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
    /// <param name="services">The <c>IServiceCollection</c> instance the extension method applies to.</param>
    /// <param name="config">An action to configure the MultiTenantOptions instance.</param>
    /// <returns>A new instance of MultiTenantBuilder.</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public static MultiTenantBuilder<TTenantInfo> AddMultiTenant<TTenantInfo>(this IServiceCollection services,
        Action<MultiTenantOptions<TTenantInfo>> config)
        where TTenantInfo : class, ITenantInfo, new()
    {
        services.AddScoped<ITenantResolver<TTenantInfo>, TenantResolver<TTenantInfo>>();
        services.AddScoped<ITenantResolver>(
            sp => (ITenantResolver)sp.GetRequiredService<ITenantResolver<TTenantInfo>>());

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
    /// <param name="services">The IServiceCollection instance the extension method applies to.</param>
    /// <returns>An new instance of MultiTenantBuilder.</returns>
    public static MultiTenantBuilder<TTenantInfo> AddMultiTenant<TTenantInfo>(this IServiceCollection services)
        where TTenantInfo : class, ITenantInfo, new()
    {
        return services.AddMultiTenant<TTenantInfo>(_ => { });
    }

    /// <summary>
    /// Decorates existing unkeyed service registrations with a new implementation that wraps the original.
    /// </summary>
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
            .Where(item => item.descriptor.ServiceType == typeof(TService)
#if NET8_0_OR_GREATER
                           && !item.descriptor.IsKeyedService
#endif
            )
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
    /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="name">The name of the options instance.</param>
    /// <param name="configureOptions">The action used to configure the options.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection ConfigurePerTenant<TOptions, TTenantInfo>(
        this IServiceCollection services,
        string? name, Action<TOptions, TTenantInfo> configureOptions)
        where TOptions : class
        where TTenantInfo : class, ITenantInfo, new()
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        ConfigurePerTenantReqs<TOptions>(services);

        services.AddTransient<IConfigureOptions<TOptions>>(sp =>
            new ConfigureNamedOptions<TOptions, IMultiTenantContextAccessor<TTenantInfo>>(
                name,
                sp.GetRequiredService<IMultiTenantContextAccessor<TTenantInfo>>(),
                (options, mtcAccessor) =>
                {
                    var tenantInfo = mtcAccessor.MultiTenantContext?.TenantInfo;
                    if (tenantInfo is not null)
                        configureOptions(options, tenantInfo);
                }));

        return services;
    }

    /// <summary>
    /// Registers an action used to configure a particular type of options.
    /// </summary>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configureOptions">The action used to configure the options.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection ConfigurePerTenant<TOptions, TTenantInfo>(
        this IServiceCollection services,
        Action<TOptions, TTenantInfo> configureOptions)
        where TOptions : class
        where TTenantInfo : class, ITenantInfo, new()
    {
        return services.ConfigurePerTenant(Microsoft.Extensions.Options.Options.DefaultName, configureOptions);
    }

    /// <summary>
    /// Registers an action used to configure all instances of a particular type of options per tenant.
    /// </summary>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configureOptions">The action used to configure the options.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection ConfigureAllPerTenant<TOptions, TTenantInfo>(
        this IServiceCollection services,
        Action<TOptions, TTenantInfo> configureOptions)
        where TOptions : class
        where TTenantInfo : class, ITenantInfo, new()
    {
        return services.ConfigurePerTenant(null, configureOptions);
    }

    /// <summary>
    /// Registers a post configure action used to configure a particular type of options per tenant.
    /// </summary>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="name">The name of the options instance.</param>
    /// <param name="configureOptions">The action used to configure the options.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection PostConfigurePerTenant<TOptions, TTenantInfo>(
        this IServiceCollection services,
        string? name, Action<TOptions, TTenantInfo> configureOptions)
        where TOptions : class
        where TTenantInfo : class, ITenantInfo, new()
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        ConfigurePerTenantReqs<TOptions>(services);

        services.AddTransient<IPostConfigureOptions<TOptions>>(sp =>
            new PostConfigureOptions<TOptions, IMultiTenantContextAccessor<TTenantInfo>>(
                name,
                sp.GetRequiredService<IMultiTenantContextAccessor<TTenantInfo>>(),
                (options, mtcAccessor) =>
                {
                    var tenantInfo = mtcAccessor.MultiTenantContext?.TenantInfo;
                    if (tenantInfo is not null)
                        configureOptions(options, tenantInfo);
                }));

        return services;
    }

    /// <summary>
    /// Registers a post configure action used to configure a particular type of options per tenant.
    /// </summary>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configureOptions">The action used to configure the options.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection PostConfigurePerTenant<TOptions, TTenantInfo>(
        this IServiceCollection services,
        Action<TOptions, TTenantInfo> configureOptions)
        where TOptions : class
        where TTenantInfo : class, ITenantInfo, new()
    {
        return services.PostConfigurePerTenant(Microsoft.Extensions.Options.Options.DefaultName, configureOptions);
    }

    /// <summary>
    /// Registers a post configure action used to configure all instances of a particular type of options per tenant.
    /// </summary>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configureOptions">The action used to configure the options.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection PostConfigureAllPerTenant<TOptions, TTenantInfo>(
        this IServiceCollection services,
        Action<TOptions, TTenantInfo> configureOptions)
        where TOptions : class
        where TTenantInfo : class, ITenantInfo, new()
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

        services.RemoveAll<IOptionsMonitorCache<TOptions>>();
        services.RemoveAll<IOptionsSnapshot<TOptions>>();
        services.RemoveAll<IOptions<TOptions>>();
        services.AddSingleton<IOptionsMonitorCache<TOptions>, MultiTenantOptionsCache<TOptions>>();
        services.AddScoped<IOptionsSnapshot<TOptions>>(BuildOptionsManager);
        services.AddSingleton<IOptions<TOptions>>(BuildOptionsManager);
        return;

        MultiTenantOptionsManager<TOptions> BuildOptionsManager(IServiceProvider sp)
        {
            IOptionsMonitorCache<TOptions> cache = ActivatorUtilities.CreateInstance<MultiTenantOptionsCache<TOptions>>(sp);
            return ActivatorUtilities.CreateInstance<MultiTenantOptionsManager<TOptions>>(sp, cache);
        }
    }
}
