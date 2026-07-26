// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Finbuckle.MultiTenant;

/// <summary>
/// Builder class for Finbuckle.MultiTenant configuration.
/// </summary>
/// <typeparam name="TTenantInfo">The <see cref="ITenantInfo{TId}"/> implementation type.</typeparam>
/// <typeparam name="TId"></typeparam>
public class MultiTenantBuilder<TTenantInfo,TId> where TTenantInfo : ITenantInfo<TId> where TId : IEquatable<TId>
{
    /// <summary>
    /// Gets or sets the <see cref="IServiceCollection"/> instance used by the builder.
    /// </summary>
    public IServiceCollection Services { get; set; }

    /// <summary>
    /// Constructs a new instance of MultiTenantBuilder.
    /// </summary>
    /// <param name="services">An <see cref="IServiceCollection"/> instance to be used by the builder.</param>
    public MultiTenantBuilder(IServiceCollection services)
    {
        Services = services;
    }

    /// <summary>
    /// Adds and configures an <see cref="IMultiTenantStore{TTenantInfo, TId}"/> to the application using default dependency injection.
    /// </summary>
    /// <param name="lifetime">The service lifetime.</param>
    /// <param name="parameters">a parameter list for any constructor parameters not covered by dependency injection.</param>
    /// <returns>The same <see cref="MultiTenantBuilder{TTenantInfo, TId}"/> passed into the method.</returns>
    public MultiTenantBuilder<TTenantInfo, TId> WithStore<TStore>(ServiceLifetime lifetime,
        params object[] parameters)
        where TStore : IMultiTenantStore<TTenantInfo, TId>
        => WithStore<TStore>(lifetime, sp => ActivatorUtilities.CreateInstance<TStore>(sp, parameters));

    /// <summary>
    /// Adds and configures an <see cref="IMultiTenantStore{TTenantInfo, TId}"/> to the application using a factory method.
    /// </summary>
    /// <param name="lifetime">The service lifetime.</param>
    /// <param name="factory">A delegate that will create and configure the store.</param>
    /// <returns>The same <see cref="MultiTenantBuilder{TTenantInfo, TId}"/> passed into the method.</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public MultiTenantBuilder<TTenantInfo, TId> WithStore<TStore>(ServiceLifetime lifetime,
        Func<IServiceProvider, TStore> factory)
        where TStore : IMultiTenantStore<TTenantInfo, TId>
    {
        ArgumentNullException.ThrowIfNull(factory);

        if (Services.Any(sd => sd.ServiceType == typeof(IMultiTenantStore<TTenantInfo, TId>)))
            throw new InvalidOperationException(
                $"Only one primary {nameof(IMultiTenantStore<TTenantInfo, TId>)} can be registered.");

        Services.Add(
            ServiceDescriptor.Describe(typeof(IMultiTenantStore<TTenantInfo, TId>), sp => factory(sp), lifetime));

        return this;
    }

    /// <summary>
    /// Adds and configures an <see cref="IMultiTenantStoreCache{TTenantInfo, TId}"/> to the application using default dependency injection.
    /// </summary>
    /// <param name="lifetime">The service lifetime.</param>
    /// <param name="parameters">a parameter list for any constructor parameters not covered by dependency injection.</param>
    /// <returns>The same <see cref="MultiTenantBuilder{TTenantInfo, TId}"/> passed into the method.</returns>
    public MultiTenantBuilder<TTenantInfo, TId> WithStoreCache<TStoreCache>(ServiceLifetime lifetime,
        params object[] parameters)
        where TStoreCache : IMultiTenantStoreCache<TTenantInfo, TId>
        => WithStoreCache<TStoreCache>(lifetime,
            sp => ActivatorUtilities.CreateInstance<TStoreCache>(sp, parameters));

    /// <summary>
    /// Adds and configures an <see cref="IMultiTenantStoreCache{TTenantInfo, TId}"/> to the application using a factory method.
    /// </summary>
    /// <param name="lifetime">The service lifetime.</param>
    /// <param name="factory">A delegate that will create and configure the store cache.</param>
    /// <returns>The same <see cref="MultiTenantBuilder{TTenantInfo, TId}"/> passed into the method.</returns>
    public MultiTenantBuilder<TTenantInfo, TId> WithStoreCache<TStoreCache>(ServiceLifetime lifetime,
        Func<IServiceProvider, TStoreCache> factory)
        where TStoreCache : IMultiTenantStoreCache<TTenantInfo, TId>
    {
        ArgumentNullException.ThrowIfNull(factory);

        Services.Add(
            ServiceDescriptor.Describe(typeof(IMultiTenantStoreCache<TTenantInfo, TId>), sp => factory(sp), lifetime));

        return this;
    }

    /// <summary>
    /// Adds and configures an <see cref="IMultiTenantStrategy"/> to the application using default dependency injection.
    /// </summary>
    /// <param name="lifetime">The service lifetime.</param>
    /// <param name="parameters">a parameter list for any constructor parameters not covered by dependency injection.</param>
    /// <returns>The same <see cref="MultiTenantBuilder{TTenantInfo, TId}"/> passed into the method.</returns>
    public MultiTenantBuilder<TTenantInfo, TId> WithStrategy<TStrategy>(ServiceLifetime lifetime,
        params object[] parameters) where TStrategy : IMultiTenantStrategy
        => WithStrategy(lifetime, sp => ActivatorUtilities.CreateInstance<TStrategy>(sp, parameters));

    /// <summary>
    /// Adds and configures an <see cref="IMultiTenantStrategy"/> to the application using a factory method.
    /// </summary>
    /// <param name="lifetime">The service lifetime.</param>
    /// <param name="factory">A delegate that will create and configure the strategy.</param>
    /// <returns>The same <see cref="MultiTenantBuilder{TTenantInfo, TId}"/> passed into the method.</returns>
    public MultiTenantBuilder<TTenantInfo, TId> WithStrategy<TStrategy>(ServiceLifetime lifetime,
        Func<IServiceProvider, TStrategy> factory)
        where TStrategy : IMultiTenantStrategy
    {
        ArgumentNullException.ThrowIfNull(factory);

        // Potential for multiple entries per service is intended.
        Services.Add(ServiceDescriptor.Describe(typeof(IMultiTenantStrategy), sp => factory(sp), lifetime));

        return this;
    }
}
