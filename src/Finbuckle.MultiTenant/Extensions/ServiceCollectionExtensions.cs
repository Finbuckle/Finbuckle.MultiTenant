// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System;
using System.Linq;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Internal;
using Finbuckle.MultiTenant.Options;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// IServiceCollection extension methods for Finbuckle.MultiTenant.
/// </summary>
public static class FinbuckleServiceCollectionExtensions
{
    /// <summary>
    /// Configure Finbuckle.MultiTenant services for the application.
    /// </summary>
    /// <param name="services">The IServiceCollection<c/> instance the extension method applies to.</param>
    /// <param name="config">An action to configure the MultiTenantOptions instance.</param>
    /// <returns>A new instance of MultiTenantBuilder.</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public static FinbuckleMultiTenantBuilder<T> AddMultiTenant<T>(this IServiceCollection services,
        Action<MultiTenantOptions> config)
        where T : class, ITenantInfo, new()
    {
        services.AddScoped<ITenantResolver<T>, TenantResolver<T>>();
        services.AddScoped<ITenantResolver>(sp => (ITenantResolver)sp.GetRequiredService<ITenantResolver<T>>());

        services.AddScoped<IMultiTenantContext<T>>(sp =>
            sp.GetRequiredService<IMultiTenantContextAccessor<T>>().MultiTenantContext!);

        services.AddScoped<T>(sp =>
            sp.GetRequiredService<IMultiTenantContextAccessor<T>>().MultiTenantContext?.TenantInfo!);
        services.AddScoped<ITenantInfo>(sp => sp.GetService<T>()!);

        services.AddSingleton<IMultiTenantContextAccessor<T>, AsyncLocalMultiTenantContextAccessor<T>>();
        services.AddSingleton<IMultiTenantContextAccessor>(sp =>
            (IMultiTenantContextAccessor)sp.GetRequiredService<IMultiTenantContextAccessor<T>>());

        services.Configure(config);

        return new FinbuckleMultiTenantBuilder<T>(services);
    }

    /// <summary>
    /// Configure Finbuckle.MultiTenant services for the application.
    /// </summary>
    /// <param name="services">The IServiceCollection<c/> instance the extension method applies to.</param>
    /// <returns>An new instance of MultiTenantBuilder.</returns>
    public static FinbuckleMultiTenantBuilder<T> AddMultiTenant<T>(this IServiceCollection services)
        where T : class, ITenantInfo, new()
    {
        return services.AddMultiTenant<T>(_ => { });
    }

    /// <summary>
    /// Gets an options builder that forwards Configure calls for the same named per-tenant <typeparamref name="TOptions"/> to the underlying service collection.
    /// </summary>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="name">The name of the options instance.</param>
    /// <returns>The <see cref="OptionsBuilder{TOptions}"/> so that configure calls can be chained in it.</returns>
    public static OptionsBuilder<TOptions> AddPerTenantOptions<TOptions>(this IServiceCollection services, string? name) where TOptions : class, new()
    {

        services.AddPerTenantOptionsCore<TOptions>();
        return new OptionsBuilder<TOptions>(services, name);
    }

    /// <summary>
    /// Gets an options builder that forwards Configure calls for the same per-tenant <typeparamref name="TOptions"/> to the underlying service collection.
    /// </summary>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <returns>The <see cref="OptionsBuilder{TOptions}"/> so that configure calls can be chained in it.</returns>
    public static OptionsBuilder<TOptions> AddPerTenantOptions<TOptions>(this IServiceCollection services) where TOptions : class, new() =>
        services.AddPerTenantOptions<TOptions>(Options.Options.DefaultName);

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
                            $"Unable to instantiate decorated type via implementation factory.");

                    return ActivatorUtilities.CreateInstance<TImpl>(sp, inner, parameters)!;
                },
                existingService.Lifetime);
        }
        else
        {
            throw new Exception(
                $"Unable to instantiate decorated type.");
        }

        services.Remove(existingService);
        services.Add(newService);

        return true;
    }

    internal static void AddPerTenantOptionsCore<TOptions>(this IServiceCollection services) where TOptions : class, new()
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        // Handles multiplexing cached options.
        services.TryAddSingleton<IOptionsMonitorCache<TOptions>, MultiTenantOptionsCache<TOptions>>();
        services.TryAddTransient<IOptionsFactory<TOptions>, MultiTenantOptionsFactory<TOptions>>();
        services.TryAddScoped<IOptionsSnapshot<TOptions>>(BuildOptionsManager<TOptions>);
        services.TryAddSingleton<IOptions<TOptions>>(BuildOptionsManager<TOptions>);
    }

    private static MultiTenantOptionsManager<TOptions> BuildOptionsManager<TOptions>(IServiceProvider sp)
        where TOptions : class, new()
    {
        var cache = (IOptionsMonitorCache<TOptions>)ActivatorUtilities.CreateInstance(sp,
            typeof(MultiTenantOptionsCache<TOptions>));
        return (MultiTenantOptionsManager<TOptions>)
            ActivatorUtilities.CreateInstance(sp, typeof(MultiTenantOptionsManager<TOptions>), cache);
    }
}