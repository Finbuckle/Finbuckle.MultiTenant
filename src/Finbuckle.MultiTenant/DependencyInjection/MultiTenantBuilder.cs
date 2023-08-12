// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Options;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Builder class for Finbuckle.MultiTenant configuration.
/// </summary>
/// <typeparam name="T">A type implementing ITenantInfo.</typeparam>
public class FinbuckleMultiTenantBuilder<T> where T : class, ITenantInfo, new()
{
    /// <summary>
    /// Gets or sets the IServiceCollection instance used by the builder.
    /// </summary>
    public IServiceCollection Services { get; set; }

    /// <summary>
    /// Construction a new instance of FinbuckleMultiTenantBuilder.
    /// </summary>
    /// <param name="services">An IServiceCollection instance to be used by the builder.</param>
    public FinbuckleMultiTenantBuilder(IServiceCollection services)
    {
        Services = services;
    }

    /// <summary>
    /// Adds per-tenant configuration for an options class.
    /// </summary>
    /// <param name="tenantConfigureOptions">The configuration action to be run for each tenant.</param>
    /// <returns>The same MultiTenantBuilder passed into the method.</returns>
    /// <remarks>This is similar to `ConfigureAll` in that it applies to all named and unnamed options of the type.</remarks>
    public FinbuckleMultiTenantBuilder<T> WithPerTenantOptions<TOptions>(
        Action<TOptions, T> tenantConfigureOptions) where TOptions : class, new()
    {
        // TODO maybe change this to string empty so null an be used for all options, note remarks.
        return WithPerTenantNamedOptions<TOptions>(null, tenantConfigureOptions);
    }

    /// <summary>
    /// Adds per-tenant configuration for an named options class.
    /// </summary>
    /// <param name="name">The option name.</param>
    /// <param name="tenantConfigureNamedOptions">The configuration action to be run for each tenant.</param>
    /// <returns>The same MultiTenantBuilder passed into the method.</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public FinbuckleMultiTenantBuilder<T> WithPerTenantNamedOptions<TOptions>(string? name,
        Action<TOptions, T> tenantConfigureNamedOptions) where TOptions : class, new()
    {
        if (tenantConfigureNamedOptions == null)
        {
            throw new ArgumentNullException(nameof(tenantConfigureNamedOptions));
        }

        Services.AddPerTenantOptionsCore<TOptions>();
        Services.TryAddEnumerable(ServiceDescriptor.Scoped<IConfigureOptions<TOptions>, TenantConfigureNamedOptionsWrapper<TOptions, T>>());
        Services.AddScoped<ITenantConfigureNamedOptions<TOptions, T>>(sp => new TenantConfigureNamedOptions<TOptions, T>(name, tenantConfigureNamedOptions));

        return this;
    }

    // TODO consider per tenant AllOptions variation
    // TODO consider per-tenant post options
    

    /// <summary>
    /// Adds and configures an IMultiTenantStore to the application using default dependency injection.
    /// </summary>>
    /// <param name="lifetime">The service lifetime.</param>
    /// <param name="parameters">a parameter list for any constructor parameters not covered by dependency injection.</param>
    /// <returns>The same MultiTenantBuilder passed into the method.</returns>
    public FinbuckleMultiTenantBuilder<T> WithStore<TStore>(ServiceLifetime lifetime,
        params object[] parameters)
        where TStore : IMultiTenantStore<T>
        => WithStore<TStore>(lifetime, sp => ActivatorUtilities.CreateInstance<TStore>(sp, parameters));

    /// <summary>
    /// Adds and configures an IMultiTenantStore to the application using a factory method.
    /// </summary>
    /// <param name="lifetime">The service lifetime.</param>
    /// <param name="factory">A delegate that will create and configure the store.</param>
    /// <returns>The same MultiTenantBuilder passed into the method.</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public FinbuckleMultiTenantBuilder<T> WithStore<TStore>(ServiceLifetime lifetime,
        Func<IServiceProvider, TStore> factory)
        where TStore : IMultiTenantStore<T>
    {
        if (factory == null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        // Note: can't use TryAddEnumerable here because ServiceDescriptor.Describe with a factory can't set implementation type.
        Services.Add(
            ServiceDescriptor.Describe(typeof(IMultiTenantStore<T>), sp => factory(sp), lifetime));

        return this;
    }

    /// <summary>
    /// Adds and configures an IMultiTenantStrategy to the application using default dependency injection.
    /// </summary>
    /// <param name="lifetime">The service lifetime.</param>
    /// <param name="parameters">a parameter list for any constructor parameters not covered by dependency injection.</param>
    /// <returns>The same MultiTenantBuilder passed into the method.</returns>
    public FinbuckleMultiTenantBuilder<T> WithStrategy<TStrategy>(ServiceLifetime lifetime,
        params object[] parameters) where TStrategy : IMultiTenantStrategy
        => WithStrategy(lifetime, sp => ActivatorUtilities.CreateInstance<TStrategy>(sp, parameters));

    /// <summary>
    /// Adds and configures an IMultiTenantStrategy to the application using a factory method.
    /// </summary>
    /// <param name="lifetime">The service lifetime.</param>
    /// <param name="factory">A delegate that will create and configure the strategy.</param>
    /// <returns>The same MultiTenantBuilder passed into the method.</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public FinbuckleMultiTenantBuilder<T> WithStrategy<TStrategy>(ServiceLifetime lifetime,
        Func<IServiceProvider, TStrategy> factory)
        where TStrategy : IMultiTenantStrategy
    {
        if (factory == null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        // Potential for multiple entries per service is intended.
        Services.Add(ServiceDescriptor.Describe(typeof(IMultiTenantStrategy), sp => factory(sp), lifetime));

        return this;
    }
}