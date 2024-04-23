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
/// This static class provides extension methods for the IServiceCollection interface.
/// These methods are used to configure Finbuckle.MultiTenant services for the application.
/// </summary>
[SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
public static class FinbuckleServiceCollectionExtensions
{
    /// <summary>
    /// Configures Finbuckle.MultiTenant services for the application.
    /// </summary>
    /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
    /// <param name="services">The <c>IServiceCollection</c> instance the extension method applies to.</param>
    /// <param name="config">An action to configure the MultiTenantOptions instance.</param>
    /// <returns>A new instance of MultiTenantBuilder.</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public static MultiTenantBuilder<TTenantInfo> AddMultiTenant<TTenantInfo>(this IServiceCollection services,
        Action<MultiTenantOptions> config)
        where TTenantInfo : class, ITenantInfo, new()
    {
        services.AddScoped<ITenantResolver<TTenantInfo>, TenantResolver<TTenantInfo>>();
        services.AddScoped<ITenantResolver>(
            sp => sp.GetRequiredService<ITenantResolver<TTenantInfo>>());

        services.AddSingleton<IMultiTenantContextAccessor<TTenantInfo>,
            AsyncLocalMultiTenantContextAccessor<TTenantInfo>>();
        services.AddSingleton<IMultiTenantContextAccessor>(sp =>
            sp.GetRequiredService<IMultiTenantContextAccessor<TTenantInfo>>());
        
        services.AddSingleton<IMultiTenantContextSetter>(sp =>
            (IMultiTenantContextSetter)sp.GetRequiredService<IMultiTenantContextAccessor>());

        services.Configure<MultiTenantOptions>(options => options.TenantInfoType = typeof(TTenantInfo));
        services.Configure(config);

        return new MultiTenantBuilder<TTenantInfo>(services);
    }

    /// <summary>
    /// Configures Finbuckle.MultiTenant services for the application.
    /// </summary>
    /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
    /// <param name="services">The IServiceCollection instance the extension method applies to.</param>
    /// <returns>A new instance of MultiTenantBuilder.</returns>
    public static MultiTenantBuilder<TTenantInfo> AddMultiTenant<TTenantInfo>(this IServiceCollection services)
        where TTenantInfo : class, ITenantInfo, new()
    {
        return services.AddMultiTenant<TTenantInfo>(_ => { });
    }

    /// <summary>
    /// Decorates an existing service with a new implementation.
    /// </summary>
    /// <typeparam name="TService">The type of the service to be decorated.</typeparam>
    /// <typeparam name="TImpl">The type of the new implementation.</typeparam>
    /// <param name="services">The IServiceCollection instance the extension method applies to.</param>
    /// <param name="parameters">Additional parameters for the new implementation.</param>
    /// <returns>True if the decoration was successful, false otherwise.</returns>
    public static bool DecorateService<TService, TImpl>(this IServiceCollection services, params object[] parameters)
    {
        var existingService = services.SingleOrDefault(s => s.ServiceType == typeof(TService));
        if (existingService is null)
            throw new ArgumentException($"No service of type {typeof(TService).Name} found.");

        ServiceDescriptor? newService;

        if (existingService.ImplementationType is not null)
        {
            newService = new ServiceDescriptor(existingService.ServiceType,
                sp =>
                {
                    TService inner =
                        (TService)ActivatorUtilities.CreateInstance(sp, existingService.ImplementationType);

                    if (inner is null)
                        throw new Exception(
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
                    return ActivatorUtilities.CreateInstance<TImpl>(sp, inner, parameters)!;
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
                        throw new Exception(
                            "Unable to instantiate decorated type via implementation factory.");

                    return ActivatorUtilities.CreateInstance<TImpl>(sp, inner, parameters)!;
                },
                existingService.Lifetime);
        }
        else
        {
            throw new Exception(
                "Unable to instantiate decorated type.");
        }

        services.Remove(existingService);
        services.Add(newService);

        return true;
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

    internal static void ConfigurePerTenantReqs<TOptions>(IServiceCollection services)
        where TOptions : class
    {
        if (services == null) throw new ArgumentNullException(nameof(services));

        // Required infrastructure.
        services.AddOptions();

        // TODO: Add check for success
        services.TryAddSingleton<IOptionsMonitorCache<TOptions>, MultiTenantOptionsCache<TOptions>>();
        services.TryAddScoped<IOptionsSnapshot<TOptions>>(BuildOptionsManager);
        services.TryAddSingleton<IOptions<TOptions>>(BuildOptionsManager);
        return;

        MultiTenantOptionsManager<TOptions> BuildOptionsManager(IServiceProvider sp)
        {
            var cache = (IOptionsMonitorCache<TOptions>)ActivatorUtilities.CreateInstance(sp,
                typeof(MultiTenantOptionsCache<TOptions>));
            return (MultiTenantOptionsManager<TOptions>)
                ActivatorUtilities.CreateInstance(sp, typeof(MultiTenantOptionsManager<TOptions>), cache);
        }
    }
}