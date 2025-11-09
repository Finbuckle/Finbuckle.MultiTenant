// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Finbuckle.MultiTenant;

// TODO: factor TTenantInfo into WithStore only

/// <summary>
/// Builder class for Finbuckle.MultiTenant configuration.
/// </summary>
/// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
public class MultiTenantBuilder<TTenantInfo> where TTenantInfo : TenantInfo
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
        ArgumentNullException.ThrowIfNull(factory);

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
    public MultiTenantBuilder<TTenantInfo> WithStrategy<TStrategy>(ServiceLifetime lifetime,
        Func<IServiceProvider, TStrategy> factory)
        where TStrategy : IMultiTenantStrategy
    {
        ArgumentNullException.ThrowIfNull(factory);

        // Potential for multiple entries per service is intended.
        Services.Add(ServiceDescriptor.Describe(typeof(IMultiTenantStrategy), sp => factory(sp), lifetime));

        return this;
    }
}