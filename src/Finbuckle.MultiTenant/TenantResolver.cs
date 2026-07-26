// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Strategies;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Finbuckle.MultiTenant;

/// <summary>
/// Resolves the current tenant.
/// </summary>
/// <typeparam name="TTenantInfo">The <see cref="ITenantInfo{TId}"/> implementation type.</typeparam>
/// <typeparam name="TId">The ID implementation type.</typeparam>
public class TenantResolver<TTenantInfo, TId> : ITenantResolver<TTenantInfo,TId>
    where TTenantInfo : ITenantInfo<TId> where TId : IEquatable<TId>
{
    private readonly MultiTenantOptions<TTenantInfo, TId> _options;
    private readonly ILoggerFactory? _loggerFactory;
    private readonly TenantManager<TTenantInfo, TId> _tenantManager;

    /// <summary>
    /// Initializes a new instance of TenantResolver.
    /// </summary>
    /// <param name="strategies">The collection of strategies to use for tenant resolution.</param>
    /// <param name="tenantManager">The tenant manager.</param>
    /// <param name="options">The multi-tenant options.</param>
    public TenantResolver(IEnumerable<IMultiTenantStrategy> strategies,
        TenantManager<TTenantInfo, TId> tenantManager, IOptions<MultiTenantOptions<TTenantInfo, TId>> options) :
        this(strategies, tenantManager, options, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of TenantResolver.
    /// </summary>
    /// <param name="strategies">The collection of strategies to use for tenant resolution.</param>
    /// <param name="tenantManager">The tenant manager.</param>
    /// <param name="options">The multi-tenant options.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    public TenantResolver(IEnumerable<IMultiTenantStrategy> strategies,
        TenantManager<TTenantInfo, TId> tenantManager, IOptions<MultiTenantOptions<TTenantInfo, TId>> options,
         ILoggerFactory? loggerFactory)
    {
        _tenantManager = tenantManager ?? throw new ArgumentNullException(nameof(tenantManager));
        _options = options.Value;
        _loggerFactory = loggerFactory;

        Strategies = strategies.OrderByDescending(s => s.Priority);
    }

    /// <inheritdoc />
    public IEnumerable<IMultiTenantStrategy> Strategies { get; set; }

    /// <inheritdoc />
    public IMultiTenantStore<TTenantInfo, TId> Store => _tenantManager.Store;

    /// <inheritdoc />
    public IEnumerable<IMultiTenantStoreCache<TTenantInfo, TId>> StoreCaches => _tenantManager.Caches;

    /// <inheritdoc />
    public async Task<TTenantInfo?> ResolveAsync(object context)
    {
        var tenantResolverLogger = _loggerFactory?.CreateLogger(GetType()) ?? NullLogger.Instance;
        IMultiTenantStrategy? finalStrategy = null;
        IMultiTenantStore<TTenantInfo, TId>? finalStore = null;
        IMultiTenantStoreCache<TTenantInfo, TId>? finalCache = null;
        TTenantInfo? resolvedTenantInfo = default;

        foreach (var strategy in Strategies)
        {
            var strategyLogger = _loggerFactory?.CreateLogger(strategy.GetType()) ?? NullLogger.Instance;

            var wrappedStrategy = new MultiTenantStrategyWrapper(strategy, strategyLogger);
            var identifier = await wrappedStrategy.GetIdentifierAsync(context).ConfigureAwait(false);

            var strategyResolveCompletedContext = new StrategyResolveCompletedContext
                { Context = context, Strategy = strategy, Identifier = identifier };
            await _options.Events.OnStrategyResolveCompleted(strategyResolveCompletedContext)
                .ConfigureAwait(false);
            if (identifier is not null && strategyResolveCompletedContext.Identifier is null &&
                tenantResolverLogger.IsEnabled(LogLevel.Debug))
                tenantResolverLogger.LogDebug("OnStrategyResolveCompleted set non-null Identifier to null");
            identifier = strategyResolveCompletedContext.Identifier;

            if (_options.IgnoredIdentifiers.Contains(identifier, StringComparer.OrdinalIgnoreCase))
            {
                if (tenantResolverLogger.IsEnabled(LogLevel.Debug))
                {
                    tenantResolverLogger.LogDebug("Ignored identifier: {Identifier}", identifier);
                }

                identifier = null;
            }

            if (identifier == null)
                continue;

            var tenantInfo = await _tenantManager.GetByIdentifierAsync(identifier, async lookupInfo =>
            {
                if (lookupInfo.Cache is not null)
                {
                    var cacheResolveCompletedContext = new StoreCacheResolveCompletedContext<TTenantInfo, TId>
                    {
                        Context = context,
                        Cache = lookupInfo.Cache,
                        Strategy = strategy,
                        Identifier = identifier,
                        TenantInfo = lookupInfo.TenantInfo
                    };
                    await _options.Events.OnStoreCacheResolveCompleted(cacheResolveCompletedContext)
                        .ConfigureAwait(false);
                    if (lookupInfo.TenantInfo is not null && cacheResolveCompletedContext.TenantInfo is null)
                        tenantResolverLogger.LogDebug("OnStoreCacheResolveCompleted set non-null TenantInfo to null");

                    if (cacheResolveCompletedContext.TenantInfo is not null)
                    {
                        finalCache = lookupInfo.Cache;
                        finalStore = null;
                        finalStrategy = strategy;
                    }

                    return cacheResolveCompletedContext.TenantInfo;
                }

                var storeResolveCompletedContext = new StoreResolveCompletedContext<TTenantInfo, TId>
                {
                    Context = context,
                    Store = lookupInfo.Store!,
                    Strategy = strategy,
                    Identifier = identifier,
                    TenantInfo = lookupInfo.TenantInfo
                };
                await _options.Events.OnStoreResolveCompleted(storeResolveCompletedContext).ConfigureAwait(false);
                if (lookupInfo.TenantInfo is not null && storeResolveCompletedContext.TenantInfo is null)
                    tenantResolverLogger.LogDebug("OnStoreResolveCompleted set non-null TenantInfo to null");

                if (storeResolveCompletedContext.TenantInfo is not null)
                {
                    finalStore = lookupInfo.Store;
                    finalCache = null;
                    finalStrategy = strategy;
                }

                return storeResolveCompletedContext.TenantInfo;
            }).ConfigureAwait(false);

            if (tenantInfo is not null)
                resolvedTenantInfo = tenantInfo;

            // no longer check strategies if tenant is resolved
            if (resolvedTenantInfo is not null)
                break;
        }

        var resolutionCompletedContext = new TenantResolveCompletedContext<TTenantInfo, TId>
        {
            TenantInfo = resolvedTenantInfo,
            Context = context,
            Store = finalStore,
            Cache = finalCache,
            Strategy = finalStrategy
        };
        await _options.Events.OnTenantResolveCompleted(resolutionCompletedContext).ConfigureAwait(false);
        return resolutionCompletedContext.TenantInfo;
    }

    /// <inheritdoc />
    async Task<ITenantInfo<TId>?> ITenantResolver<TId>.ResolveAsync(object context)
    {
        return await ResolveAsync(context).ConfigureAwait(false);
    }
}
