// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Stores.ConfigurationStore;
using Finbuckle.MultiTenant.Stores.DistributedCacheStore;
using Finbuckle.MultiTenant.Stores.EchoStore;
using Finbuckle.MultiTenant.Stores.HttpRemoteStore;
using Finbuckle.MultiTenant.Stores.InMemoryStore;
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
    /// Adds a DistributedCacheStore to the application with maximum sliding expiration.
    /// </summary>
    /// <typeparam name="TTenantInfo">The TenantInfo derived type.</typeparam>
    /// <returns>The <see cref="MultiTenantBuilder&lt;TTenantInfo&gt;"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithDistributedCacheStore<TTenantInfo>(this MultiTenantBuilder<TTenantInfo> builder)
        where TTenantInfo : TenantInfo
        => builder.WithDistributedCacheStore(TimeSpan.MaxValue);


    /// <summary>
    /// Adds a DistributedCacheStore to the application.
    /// </summary>
    /// <typeparam name="TTenantInfo">The TenantInfo derived type.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="slidingExpiration">The timespan for a cache entry's sliding expiration.</param>
    /// <returns>The <see cref="MultiTenantBuilder&lt;TTenantInfo&gt;"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithDistributedCacheStore<TTenantInfo>(this MultiTenantBuilder<TTenantInfo> builder, TimeSpan? slidingExpiration)
        where TTenantInfo : TenantInfo
    {
        var storeParams = slidingExpiration is null ? new object[] { Constants.TenantToken } : new object[] { Constants.TenantToken, slidingExpiration };

        return builder.WithStore<DistributedCacheStore<TTenantInfo>>(ServiceLifetime.Transient, storeParams);
    }

    /// <summary>
    /// Adds a HttpRemoteStore to the application.
    /// </summary>
    /// <typeparam name="TTenantInfo">The TenantInfo derived type.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="endpointTemplate">The endpoint URI template.</param>
    /// <returns>The <see cref="MultiTenantBuilder&lt;TTenantInfo&gt;"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithHttpRemoteStore<TTenantInfo>(this MultiTenantBuilder<TTenantInfo> builder, string endpointTemplate)
        where TTenantInfo : TenantInfo
        => builder.WithHttpRemoteStore(endpointTemplate, null);

    /// <summary>
    /// Adds a HttpRemoteStore to the application.
    /// </summary>
    /// <typeparam name="TTenantInfo">The TenantInfo derived type.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="endpointTemplate">The endpoint URI template.</param>
    /// <param name="clientConfig">An action to configure the underlying HttpClient.</param>
    /// <returns>The <see cref="MultiTenantBuilder&lt;TTenantInfo&gt;"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithHttpRemoteStore<TTenantInfo>(this MultiTenantBuilder<TTenantInfo> builder,
        string endpointTemplate,
        Action<IHttpClientBuilder>? clientConfig) where TTenantInfo : TenantInfo
    {
        var httpClientBuilder = builder.Services.AddHttpClient(typeof(HttpRemoteStoreClient<TTenantInfo>).FullName!);
        clientConfig?.Invoke(httpClientBuilder);

        builder.Services.TryAddSingleton<HttpRemoteStoreClient<TTenantInfo>>();

        return builder.WithStore<HttpRemoteStore<TTenantInfo>>(ServiceLifetime.Singleton, endpointTemplate);
    }

    /// <summary>
    /// Adds a ConfigurationStore to the application. Uses the default IConfiguration and section "Finbuckle:MultiTenant:Stores:ConfigurationStore".
    /// </summary>
    /// <typeparam name="TTenantInfo">The TenantInfo derived type.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <returns>The <see cref="MultiTenantBuilder&lt;TTenantInfo&gt;"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithConfigurationStore<TTenantInfo>(this MultiTenantBuilder<TTenantInfo> builder)
        where TTenantInfo : TenantInfo
        => builder.WithStore<ConfigurationStore<TTenantInfo>>(ServiceLifetime.Singleton);

    /// <summary>
    /// Adds a ConfigurationStore to the application.
    /// </summary>
    /// <typeparam name="TTenantInfo">The TenantInfo derived type.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="configuration">The IConfiguration to load the section from.</param>
    /// <param name="sectionName">The configuration section to load.</param>
    /// <returns>The <see cref="MultiTenantBuilder&lt;TTenantInfo&gt;"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithConfigurationStore<TTenantInfo>(this MultiTenantBuilder<TTenantInfo> builder,
        IConfiguration configuration,
        string sectionName)
        where TTenantInfo : TenantInfo
        => builder.WithStore<ConfigurationStore<TTenantInfo>>(ServiceLifetime.Singleton, configuration, sectionName);

    /// <summary>
    /// Adds an empty InMemoryStore to the application.
    /// </summary>
    /// <typeparam name="TTenantInfo">The TenantInfo derived type.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <returns>The <see cref="MultiTenantBuilder&lt;TTenantInfo&gt;"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithInMemoryStore<TTenantInfo>(this MultiTenantBuilder<TTenantInfo> builder)
        where TTenantInfo : TenantInfo
        => builder.WithInMemoryStore(_ => {});

    /// <summary>
    /// Adds and configures InMemoryStore to the application using the provided action.
    /// </summary>
    /// <typeparam name="TTenantInfo">The TenantInfo derived type.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="config">An action for configuring the store.</param>
    /// <returns>The <see cref="MultiTenantBuilder&lt;TTenantInfo&gt;"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithInMemoryStore<TTenantInfo>(this MultiTenantBuilder<TTenantInfo> builder,
        Action<InMemoryStoreOptions<TTenantInfo>> config)
        where TTenantInfo : TenantInfo
    {
        ArgumentNullException.ThrowIfNull(config);

        // ReSharper disable once RedundantTypeArgumentsOfMethod
        builder.Services.Configure<InMemoryStoreOptions<TTenantInfo>>(config);

        return builder.WithStore<InMemoryStore<TTenantInfo>>(ServiceLifetime.Singleton);
    }
    
    /// <summary>
    /// Adds an EchoStore to the application.
    /// </summary>
    /// <typeparam name="TTenantInfo">The TenantInfo derived type.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <returns>The <see cref="MultiTenantBuilder&lt;TTenantInfo&gt;"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithEchoStore<TTenantInfo>(this MultiTenantBuilder<TTenantInfo> builder)
        where TTenantInfo : TenantInfo
        => builder.WithStore<EchoStore<TTenantInfo>>(ServiceLifetime.Singleton);

    /// <summary>
    /// Adds and configures a StaticStrategy to the application.
    /// </summary>
    /// <typeparam name="TTenantInfo">The TenantInfo derived type.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="identifier">The tenant identifier to use for all tenant resolution.</param>
    /// <returns>The <see cref="MultiTenantBuilder&lt;TTenantInfo&gt;"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithStaticStrategy<TTenantInfo>(this MultiTenantBuilder<TTenantInfo> builder,
        string identifier)
        where TTenantInfo : TenantInfo
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            throw new ArgumentNullException(nameof(identifier), "Invalid value for \"identifier\"");
        }

        return builder.WithStrategy<StaticStrategy>(ServiceLifetime.Singleton, new object[] { identifier });
    }

    /// <summary>
    /// Adds and configures a DelegateStrategy to the application.
    /// </summary>
    /// <typeparam name="TTenantInfo">The TenantInfo derived type.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="doStrategy">The delegate implementing the strategy.</param>
    /// <returns>The <see cref="MultiTenantBuilder&lt;TTenantInfo&gt;"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithDelegateStrategy<TTenantInfo>(this MultiTenantBuilder<TTenantInfo> builder,
        Func<object, Task<string?>> doStrategy)
        where TTenantInfo : TenantInfo
    {
        ArgumentNullException.ThrowIfNull(doStrategy);

        return builder.WithStrategy<DelegateStrategy>(ServiceLifetime.Singleton, new object[] { doStrategy });
    }
    
    /// <summary>
    /// Adds and configures a typed DelegateStrategy&lt;TContext&gt; to the application.
    /// </summary>
    /// <typeparam name="TContext">The strategy context type.</typeparam>
    /// <typeparam name="TTenantInfo">The TenantInfo derived type.</typeparam>
    /// <param name="builder"></param>
    /// <param name="doStrategy"></param>
    /// <returns>The <see cref="MultiTenantBuilder&lt;TTenantInfo&gt;"/> so that additional calls can be chained.</returns>
    public static MultiTenantBuilder<TTenantInfo> WithDelegateStrategy<TContext, TTenantInfo>(this MultiTenantBuilder<TTenantInfo> builder,
        Func<TContext, Task<string?>> doStrategy)
        where TTenantInfo : TenantInfo
    {
        ArgumentNullException.ThrowIfNull(doStrategy, nameof(doStrategy));

        Func<object, Task<string?>> wrapStrategy = context =>
        {
            if (context.GetType() == typeof(TContext))
            {
                return doStrategy((TContext)context);
            }

            return Task.FromResult<string?>(null);
        };

        return builder.WithDelegateStrategy(wrapStrategy);
    }
}