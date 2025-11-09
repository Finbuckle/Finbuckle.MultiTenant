// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Diagnostics.CodeAnalysis;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Finbuckle.MultiTenant.Extensions;

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
        where TTenantInfo : TenantInfo
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
        where TTenantInfo : TenantInfo
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
                throw new  ArgumentException(
                    "Unable to instantiate decorated type.");
            }

            services.Remove(existingService);
            services.Add(newService);
        }

        return true;
    }
}