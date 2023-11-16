// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

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

        // TODO this might require instance
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

    // TODO adjust summary
    /// <summary>
    /// Registers an action used to configure a particular type of options.
    /// Note: These are run before all <seealso cref="PostConfigure{TOptions}(IServiceCollection, Action{TOptions})"/>.
    /// </summary>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="name">The name of the options instance.</param>
    /// <param name="configureOptions">The action used to configure the options.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection ConfigurePerTenant<TOptions, TTenantInfo>(this IServiceCollection services,
        string? name, Action<TOptions, TTenantInfo> action) where TOptions : class
        where TTenantInfo : class, ITenantInfo, new()
    {
        // Required infrastructure.
        services.AddOptions();
        services.TryAddSingleton<IOptionsMonitorCache<TOptions>, MultiTenantOptionsCache<TOptions>>();
        services.TryAddScoped<IOptionsSnapshot<TOptions>>(BuildOptionsManager<TOptions>);
        services.TryAddSingleton<IOptions<TOptions>>(BuildOptionsManager<TOptions>);
        
        services.AddSingleton<IConfigureOptions<TOptions>>(sp =>
        {
            var multiTenantContextAccessor = sp.GetRequiredService<IMultiTenantContextAccessor<TTenantInfo>>();

            void ConfigureAction(TOptions options)
            {
                var multiTenantContext = multiTenantContextAccessor.MultiTenantContext;
                if (multiTenantContext.HasResolvedTenant)
                    action(options, multiTenantContext.TenantInfo);
            }

            return new ConfigureNamedOptions<TOptions>(name, ConfigureAction);
        });

        return services;
    }
    
    // TODO adjust summary
    /// <summary>
    /// Registers an action used to configure a particular type of options.
    /// Note: These are run before all <seealso cref="PostConfigure{TOptions}(IServiceCollection, Action{TOptions})"/>.
    /// </summary>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="name">The name of the options instance.</param>
    /// <param name="configureOptions">The action used to configure the options.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection ConfigurePerTenant<TOptions, TTenantInfo>(this IServiceCollection services,
        Action<TOptions, TTenantInfo> action) where TOptions : class
        where TTenantInfo : class, ITenantInfo, new()
    {
        return services.ConfigurePerTenant(Options.Options.DefaultName, action);
    }

    private static void AddOptionsPerTenantCore<TOptions>(this IServiceCollection services) where TOptions : class
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        
    }

    private static MultiTenantOptionsManager<TOptions> BuildOptionsManager<TOptions>(IServiceProvider sp)
        where TOptions : class
    {
        var cache = (IOptionsMonitorCache<TOptions>)ActivatorUtilities.CreateInstance(sp,
            typeof(MultiTenantOptionsCache<TOptions>));
        return (MultiTenantOptionsManager<TOptions>)
            ActivatorUtilities.CreateInstance(sp, typeof(MultiTenantOptionsManager<TOptions>), cache);
    }
}