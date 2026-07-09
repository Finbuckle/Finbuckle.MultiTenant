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
/// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
public class TenantResolver<TTenantInfo> : ITenantResolver<TTenantInfo>
    where TTenantInfo : ITenantInfo
{
    private readonly MultiTenantOptions<TTenantInfo> _options;
    private readonly ILoggerFactory? _loggerFactory;
    private readonly TenantManager<TTenantInfo> _tenantManager;

    /// <summary>
    /// Initializes a new instance of TenantResolver.
    /// </summary>
    /// <param name="strategies">The collection of strategies to use for tenant resolution.</param>
    /// <param name="tenantManager">The tenant manager.</param>
    /// <param name="options">The multi-tenant options.</param>
    public TenantResolver(IEnumerable<IMultiTenantStrategy> strategies,
        TenantManager<TTenantInfo> tenantManager, IOptions<MultiTenantOptions<TTenantInfo>> options) :
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
        TenantManager<TTenantInfo> tenantManager, IOptions<MultiTenantOptions<TTenantInfo>> options,
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
    public IMultiTenantStore<TTenantInfo> Store => _tenantManager.Store;

    /// <inheritdoc />
    public IEnumerable<IMultiTenantStoreCache<TTenantInfo>> StoreCaches => _tenantManager.Caches;

    /// <inheritdoc />
    public async Task<ITenantContext<TTenantInfo>> ResolveAsync(object context)
    {
        var tenantResolverLogger = _loggerFactory?.CreateLogger(GetType()) ?? NullLogger.Instance;
        IMultiTenantStrategy finalStrategy = null!;
        IMultiTenantStore<TTenantInfo>? finalStore = null;
        IMultiTenantStoreCache<TTenantInfo>? finalCache = null;
        ITenantContext<TTenantInfo> tc = new TenantContext<TTenantInfo>();

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
                    var cacheResolveCompletedContext = new StoreCacheResolveCompletedContext<TTenantInfo>
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

                var storeResolveCompletedContext = new StoreResolveCompletedContext<TTenantInfo>
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
                tc = new TenantContext<TTenantInfo> { TenantInfo = tenantInfo };

            // no longer check strategies if tenant is resolved
            if (tc.IsResolved)
                break;
        }

        var resolutionCompletedContext = new TenantResolveCompletedContext<TTenantInfo>
        {
            TenantContext = tc,
            Context = context,
            Store = finalStore,
            Cache = finalCache,
            Strategy = finalStrategy
        };
        await _options.Events.OnTenantResolveCompleted(resolutionCompletedContext).ConfigureAwait(false);
        return resolutionCompletedContext.TenantContext;
    }

    /// <inheritdoc />
    async Task<ITenantContext> ITenantResolver.ResolveAsync(object context)
    {
        return await ResolveAsync(context).ConfigureAwait(false);
    }
}
