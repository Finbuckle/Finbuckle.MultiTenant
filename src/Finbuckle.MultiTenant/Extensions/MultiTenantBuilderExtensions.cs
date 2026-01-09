// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Stores;
using Finbuckle.MultiTenant.Strategies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Finbuckle.MultiTenant.Extensions;

/// <summary>
/// Provides builder methods for Finbuckle.MultiTenant services and configuration.
/// </summary>
public static class MultiTenantBuilderExtensions
{
    /// <summary>
    /// Adds a <see cref="DistributedCacheStore{TTenantInfo}"/> to the application with maximum sliding expiration.
    /// </summary>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    /// <param name="builder">The <see cref="MultiTenantBuilder{TTenantInfo}"/> instance.</param>
    /// <returns>The <see cref="MultiTenantBuilder{TTenantInfo}"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithDistributedCacheStore<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder)
        where TTenantInfo : ITenantInfo
        => builder.WithDistributedCacheStore(TimeSpan.MaxValue);

    /// <summary>
    /// Adds a <see cref="DistributedCacheStore{TTenantInfo}"/> to the application.
    /// </summary>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    /// <param name="builder">The <see cref="MultiTenantBuilder{TTenantInfo}"/> instance.</param>
    /// <param name="slidingExpiration">The timespan for a cache entry's sliding expiration.</param>
    /// <returns>The <see cref="MultiTenantBuilder{TTenantInfo}"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithDistributedCacheStore<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder, TimeSpan? slidingExpiration)
        where TTenantInfo : ITenantInfo
    {
        var storeParams = slidingExpiration is null
            ? new object[] { Constants.TenantToken }
            : new object[] { Constants.TenantToken, slidingExpiration };

        return builder.WithStore<DistributedCacheStore<TTenantInfo>>(ServiceLifetime.Transient, storeParams);
    }

    /// <summary>
    /// Adds a <see cref="HttpRemoteStore{TTenantInfo}"/> to the application.
    /// </summary>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    /// <param name="builder">The <see cref="MultiTenantBuilder{TTenantInfo}"/> instance.</param>
    /// <param name="endpointTemplate">The endpoint URI template.</param>
    /// <returns>The <see cref="MultiTenantBuilder{TTenantInfo}"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithHttpRemoteStore<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder, string endpointTemplate)
        where TTenantInfo : ITenantInfo
        => builder.WithHttpRemoteStore(endpointTemplate, null);

    /// <summary>
    /// Adds a <see cref="HttpRemoteStore{TTenantInfo}"/> to the application.
    /// </summary>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    /// <param name="builder">The <see cref="MultiTenantBuilder{TTenantInfo}"/> instance.</param>
    /// <param name="endpointTemplate">The endpoint URI template.</param>
    /// <param name="clientConfig">An action to configure the underlying <see cref="HttpClient"/>.</param>
    /// <returns>The <see cref="MultiTenantBuilder{TTenantInfo}"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithHttpRemoteStore<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder,
        string endpointTemplate,
        Action<IHttpClientBuilder>? clientConfig) where TTenantInfo : ITenantInfo
    {
        var httpClientBuilder = builder.Services.AddHttpClient(typeof(HttpRemoteStoreClient<TTenantInfo>).FullName!);
        clientConfig?.Invoke(httpClientBuilder);

        builder.Services.TryAddSingleton<HttpRemoteStoreClient<TTenantInfo>>();

        return builder.WithStore<HttpRemoteStore<TTenantInfo>>(ServiceLifetime.Singleton, endpointTemplate);
    }

    /// <summary>
    /// Adds a <see cref="ConfigurationStore{TTenantInfo}"/> to the application. Uses the default <see cref="IConfiguration"/> and section "Finbuckle:MultiTenant:Stores:ConfigurationStore".
    /// </summary>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    /// <param name="builder">The <see cref="MultiTenantBuilder{TTenantInfo}"/> instance.</param>
    /// <returns>The <see cref="MultiTenantBuilder{TTenantInfo}"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithConfigurationStore<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder)
        where TTenantInfo : ITenantInfo
        => builder.WithStore<ConfigurationStore<TTenantInfo>>(ServiceLifetime.Singleton);

    /// <summary>
    /// Adds a <see cref="ConfigurationStore{TTenantInfo}"/> to the application.
    /// </summary>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    /// <param name="builder">The <see cref="MultiTenantBuilder{TTenantInfo}"/> instance.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> to load the section from.</param>
    /// <param name="sectionName">The configuration section to load.</param>
    /// <returns>The <see cref="MultiTenantBuilder{TTenantInfo}"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithConfigurationStore<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder,
        IConfiguration configuration,
        string sectionName)
        where TTenantInfo : ITenantInfo
        => builder.WithStore<ConfigurationStore<TTenantInfo>>(ServiceLifetime.Singleton, configuration, sectionName);

    /// <summary>
    /// Adds an empty <see cref="InMemoryStore{TTenantInfo}"/> to the application.
    /// </summary>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    /// <param name="builder">The <see cref="MultiTenantBuilder{TTenantInfo}"/> instance.</param>
    /// <returns>The <see cref="MultiTenantBuilder{TTenantInfo}"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithInMemoryStore<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder)
        where TTenantInfo : ITenantInfo
        => builder.WithInMemoryStore(_ => { });

    /// <summary>
    /// Adds and configures <see cref="InMemoryStore{TTenantInfo}"/> to the application using the provided action.
    /// </summary>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    /// <param name="builder">The <see cref="MultiTenantBuilder{TTenantInfo}"/> instance.</param>
    /// <param name="config">An action for configuring the store.</param>
    /// <returns>The <see cref="MultiTenantBuilder{TTenantInfo}"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithInMemoryStore<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder,
        Action<InMemoryStoreOptions<TTenantInfo>> config)
        where TTenantInfo : ITenantInfo
    {
        ArgumentNullException.ThrowIfNull(config);

        builder.Services.Configure(config);

        return builder.WithStore<InMemoryStore<TTenantInfo>>(ServiceLifetime.Singleton);
    }

    /// <summary>
    /// Adds an <see cref="EchoStore{TTenantInfo}"/> to the application.
    /// </summary>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    /// <param name="builder">The <see cref="MultiTenantBuilder{TTenantInfo}"/> instance.</param>
    /// <returns>The <see cref="MultiTenantBuilder{TTenantInfo}"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithEchoStore<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder)
        where TTenantInfo : ITenantInfo
        => builder.WithStore<EchoStore<TTenantInfo>>(ServiceLifetime.Singleton);

    /// <summary>
    /// Adds and configures a <see cref="StaticStrategy"/> to the application.
    /// </summary>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    /// <param name="builder">The <see cref="MultiTenantBuilder{TTenantInfo}"/> instance.</param>
    /// <param name="identifier">The tenant identifier to use for all tenant resolution.</param>
    /// <returns>The <see cref="MultiTenantBuilder{TTenantInfo}"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithStaticStrategy<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder,
        string identifier)
        where TTenantInfo : ITenantInfo
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            throw new ArgumentNullException(nameof(identifier), "Invalid value for \"identifier\"");
        }

        return builder.WithStrategy<StaticStrategy>(ServiceLifetime.Singleton, identifier);
    }

    /// <summary>
    /// Adds and configures a <see cref="DelegateStrategy"/> to the application.
    /// </summary>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    /// <param name="builder">The <see cref="MultiTenantBuilder{TTenantInfo}"/> instance.</param>
    /// <param name="doStrategy">The delegate implementing the strategy.</param>
    /// <returns>The <see cref="MultiTenantBuilder{TTenantInfo}"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithDelegateStrategy<TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder,
        Func<object, Task<string?>> doStrategy)
        where TTenantInfo : ITenantInfo
    {
        ArgumentNullException.ThrowIfNull(doStrategy);

        return builder.WithStrategy<DelegateStrategy>(ServiceLifetime.Singleton, doStrategy);
    }

    /// <summary>
    /// Adds and configures a typed <see cref="DelegateStrategy"/> to the application.
    /// </summary>
    /// <typeparam name="TContext">The strategy context type.</typeparam>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    /// <param name="builder">The <see cref="MultiTenantBuilder{TTenantInfo}"/> instance.</param>
    /// <param name="doStrategy">The delegate implementing the strategy.</param>
    /// <remarks>
    /// The delegate will be invoked when the runtime <c>context</c> instance is assignable to <typeparamref name="TContext"/>,
    /// i.e., when it is of type <typeparamref name="TContext"/> or a derived type. If the runtime type is not assignable to
    /// <typeparamref name="TContext"/>, this strategy returns <c>null</c> and resolution falls through to the next strategy.
    /// </remarks>
    /// <returns>The <see cref="MultiTenantBuilder{TTenantInfo}"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithDelegateStrategy<TContext, TTenantInfo>(
        this MultiTenantBuilder<TTenantInfo> builder,
        Func<TContext, Task<string?>> doStrategy)
        where TTenantInfo : ITenantInfo
    {
        ArgumentNullException.ThrowIfNull(doStrategy);

        Task<string?> WrapStrategy(object context)
        {
            if (context is TContext typed)
            {
                return doStrategy(typed);
            }

            return Task.FromResult<string?>(null);
        }

        return builder.WithDelegateStrategy(WrapStrategy);
    }
}