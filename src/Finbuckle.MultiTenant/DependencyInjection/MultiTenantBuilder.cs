// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Builder class for Finbuckle.MultiTenant configuration.
/// </summary>
/// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
public class MultiTenantBuilder<TTenantInfo> where TTenantInfo : class, ITenantInfo, new()
{
    /// <summary>
    /// Gets or sets the IServiceCollection instance used by the builder.
    /// </summary>
    public IServiceCollection Services { get; set; }

    /// <summary>
    /// Construction a new instance of FinbuckleMultiTenantBuilder.
    /// </summary>
    /// <param name="services">An IServiceCollection instance to be used by the builder.</param>
    public MultiTenantBuilder(IServiceCollection services)
    {
        Services = services;
    }

    /// <summary>
    /// Adds per-tenant configuration for an options class.
    /// </summary>
    /// <param name="tenantConfigureOptions">The configuration action to be run for each tenant.</param>
    /// <returns>The same MultiTenantBuilder passed into the method.</returns>
    /// <remarks>This is similar to `ConfigureAll` in that it applies to all named and unnamed options of the type.</remarks>
    [Obsolete]
    public MultiTenantBuilder<TTenantInfo> WithPerTenantOptions<TOptions>(
        Action<TOptions, TTenantInfo> tenantConfigureOptions) where TOptions : class, new()
    {
        // TODO remove this method
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
    [Obsolete]
    public MultiTenantBuilder<TTenantInfo> WithPerTenantNamedOptions<TOptions>(string? name,
        Action<TOptions, TTenantInfo> tenantConfigureNamedOptions) where TOptions : class, new()
    {
        // TODO remove this method
        // if (tenantConfigureNamedOptions == null)
        // {
        //     throw new ArgumentNullException(nameof(tenantConfigureNamedOptions));
        // }
        //
        // // Services.AddOptionsCore<TOptions>();
        // Services.TryAddEnumerable(ServiceDescriptor
        //     .Scoped<IConfigureOptions<TOptions>, TenantConfigureNamedOptionsWrapper<TOptions, T>>());
        // Services.AddScoped<ITenantConfigureNamedOptionsOld<TOptions, T>>(sp =>
        //     new MultiTenantConfigureNamedOptions<TOptions, T>(name, tenantConfigureNamedOptions));

        return this;
    }

    /// <summary>
    /// Adds and configures an IMultiTenantStore to the application using default dependency injection.
    /// </summary>>
    /// <param name="lifetime">The service lifetime.</param>
    /// <param name="parameters">a parameter list for any constructor parameters not covered by dependency injection.</param>
    /// <returns>The same MultiTenantBuilder passed into the method.</returns>
    public MultiTenantBuilder<TTenantInfo> WithStore<TStore>(ServiceLifetime lifetime,
        params object[] parameters)
        where TStore : IMultiTenantStore<TTenantInfo>
        => WithStore<TStore>(lifetime, sp => ActivatorUtilities.CreateInstance<TStore>(sp, parameters));

    /// <summary>
    /// Adds and configures an IMultiTenantStore to the application using a factory method.
    /// </summary>
    /// <param name="lifetime">The service lifetime.</param>
    /// <param name="factory">A delegate that will create and configure the store.</param>
    /// <returns>The same MultiTenantBuilder passed into the method.</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public MultiTenantBuilder<TTenantInfo> WithStore<TStore>(ServiceLifetime lifetime,
        Func<IServiceProvider, TStore> factory)
        where TStore : IMultiTenantStore<TTenantInfo>
    {
        if (factory == null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        // Note: can't use TryAddEnumerable here because ServiceDescriptor.Describe with a factory can't set implementation type.
        Services.Add(
            ServiceDescriptor.Describe(typeof(IMultiTenantStore<TTenantInfo>), sp => factory(sp), lifetime));

        return this;
    }

    /// <summary>
    /// Adds and configures an IMultiTenantStrategy to the application using default dependency injection.
    /// </summary>
    /// <param name="lifetime">The service lifetime.</param>
    /// <param name="parameters">a parameter list for any constructor parameters not covered by dependency injection.</param>
    /// <returns>The same MultiTenantBuilder passed into the method.</returns>
    public MultiTenantBuilder<TTenantInfo> WithStrategy<TStrategy>(ServiceLifetime lifetime,
        params object[] parameters) where TStrategy : IMultiTenantStrategy
        => WithStrategy(lifetime, sp => ActivatorUtilities.CreateInstance<TStrategy>(sp, parameters));

    /// <summary>
    /// Adds and configures an IMultiTenantStrategy to the application using a factory method.
    /// </summary>
    /// <param name="lifetime">The service lifetime.</param>
    /// <param name="factory">A delegate that will create and configure the strategy.</param>
    /// <returns>The same MultiTenantBuilder passed into the method.</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public MultiTenantBuilder<TTenantInfo> WithStrategy<TStrategy>(ServiceLifetime lifetime,
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