// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Stores;
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
    private readonly IOptionsMonitor<MultiTenantOptions<TTenantInfo>> options;
    private readonly ILoggerFactory? loggerFactory;

    /// <summary>
    /// Initializes a new instance of TenantResolver.
    /// </summary>
    /// <param name="strategies">The collection of strategies to use for tenant resolution.</param>
    /// <param name="stores">The collection of stores to use for tenant resolution.</param>
    /// <param name="options">The multi-tenant options.</param>
    public TenantResolver(IEnumerable<IMultiTenantStrategy> strategies,
        IEnumerable<IMultiTenantStore<TTenantInfo>> stores, IOptionsMonitor<MultiTenantOptions<TTenantInfo>> options) :
        this(strategies, stores, options, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of TenantResolver.
    /// </summary>
    /// <param name="strategies">The collection of strategies to use for tenant resolution.</param>
    /// <param name="stores">The collection of stores to use for tenant resolution.</param>
    /// <param name="options">The multi-tenant options.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    public TenantResolver(IEnumerable<IMultiTenantStrategy> strategies,
        IEnumerable<IMultiTenantStore<TTenantInfo>> stores, IOptionsMonitor<MultiTenantOptions<TTenantInfo>> options,
        ILoggerFactory? loggerFactory)
    {
        Stores = stores;
        this.options = options;
        this.loggerFactory = loggerFactory;

        Strategies = strategies.OrderByDescending(s => s.Priority);
    }

    /// <inheritdoc />
    public IEnumerable<IMultiTenantStrategy> Strategies { get; set; }

    /// <inheritdoc />
    public IEnumerable<IMultiTenantStore<TTenantInfo>> Stores { get; set; }

    /// <inheritdoc />
    public async Task<IMultiTenantContext<TTenantInfo>> ResolveAsync(object context)
    {
        var mtc = new MultiTenantContext<TTenantInfo>(default);
        var tenantResolverLogger = loggerFactory?.CreateLogger(GetType()) ?? NullLogger.Instance;

        foreach (var strategy in Strategies)
        {
            var strategyLogger = loggerFactory?.CreateLogger(strategy.GetType()) ?? NullLogger.Instance;

            var wrappedStrategy = new MultiTenantStrategyWrapper(strategy, strategyLogger);
            var identifier = await wrappedStrategy.GetIdentifierAsync(context).ConfigureAwait(false);

            var strategyResolveCompletedContext = new StrategyResolveCompletedContext
                { Context = context, Strategy = strategy, Identifier = identifier };
            await options.CurrentValue.Events.OnStrategyResolveCompleted(strategyResolveCompletedContext)
                .ConfigureAwait(false);
            if (identifier is not null && strategyResolveCompletedContext.Identifier is null)
                tenantResolverLogger.LogDebug("OnStrategyResolveCompleted set non-null Identifier to null");
            identifier = strategyResolveCompletedContext.Identifier;

            if (options.CurrentValue.IgnoredIdentifiers.Contains(identifier, StringComparer.OrdinalIgnoreCase))
            {
                tenantResolverLogger.LogDebug("Ignored identifier: {Identifier}", identifier);
                identifier = null;
            }

            if (identifier == null)
                continue;

            foreach (var store in Stores)
            {
                var storeLogger = loggerFactory?.CreateLogger(store.GetType()) ?? NullLogger.Instance;

                var wrappedStore = new MultiTenantStoreWrapper<TTenantInfo>(store, storeLogger);
                var tenantInfo = await wrappedStore.GetByIdentifierAsync(identifier).ConfigureAwait(false);

                var storeResolveCompletedContext = new StoreResolveCompletedContext<TTenantInfo>
                {
                    Context = context, Store = store, Strategy = strategy, Identifier = identifier,
                    TenantInfo = tenantInfo
                };
                await options.CurrentValue.Events.OnStoreResolveCompleted(storeResolveCompletedContext)
                    .ConfigureAwait(false);
                if (tenantInfo is not null && storeResolveCompletedContext.TenantInfo is null)
                    tenantResolverLogger.LogDebug("OnStoreResolveCompleted set non-null TenantInfo to null");
                tenantInfo = storeResolveCompletedContext.TenantInfo;

                if (tenantInfo != null)
                {
                    var storeInfo = new StoreInfo<TTenantInfo> { Store = store };
                    var strategyInfo = new StrategyInfo { Strategy = strategy };
                    mtc = new MultiTenantContext<TTenantInfo>(tenantInfo, strategyInfo, storeInfo);
                }

                // no longer check stores if tenant is resolved
                if (mtc.IsResolved)
                    break;
            }

            // no longer check strategies if tenant is resolved
            if (mtc.IsResolved)
                break;
        }

        var resolutionCompletedContext = new TenantResolveCompletedContext<TTenantInfo>
            { MultiTenantContext = mtc, Context = context };
        await options.CurrentValue.Events.OnTenantResolveCompleted(resolutionCompletedContext).ConfigureAwait(false);
        return resolutionCompletedContext.MultiTenantContext;
    }

    /// <inheritdoc />
    async Task<IMultiTenantContext> ITenantResolver.ResolveAsync(object context)
    {
        return await ResolveAsync(context).ConfigureAwait(false);
    }
}