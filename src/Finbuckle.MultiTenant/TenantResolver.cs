// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Events;
using Finbuckle.MultiTenant.Stores;
using Finbuckle.MultiTenant.Strategies;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Finbuckle.MultiTenant;

/// <summary>
/// Resolves the current tenant.
/// </summary>
/// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>X
public class TenantResolver<TTenantInfo> : ITenantResolver<TTenantInfo>
    where TTenantInfo : class, ITenantInfo, new()
{
    private readonly IOptionsMonitor<MultiTenantOptions<TTenantInfo>> options;
    private readonly ILoggerFactory? loggerFactory;

    public TenantResolver(IEnumerable<IMultiTenantStrategy> strategies,
        IEnumerable<IMultiTenantStore<TTenantInfo>> stores, IOptionsMonitor<MultiTenantOptions<TTenantInfo>> options) :
        this(strategies, stores, options, null)
    {
    }

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
        var mtc = new MultiTenantContext<TTenantInfo>();
        var tenantResoloverLogger = loggerFactory?.CreateLogger(this.GetType()) ?? NullLogger.Instance;

        string? identifier = null;
        foreach (var strategy in Strategies)
        {
            var strategyLogger = loggerFactory?.CreateLogger(strategy.GetType()) ?? NullLogger.Instance;

            var wrappedStrategy = new MultiTenantStrategyWrapper(strategy, strategyLogger);
            identifier = await wrappedStrategy.GetIdentifierAsync(context).ConfigureAwait(false);

            var strategyResolveCompletedContext = new StrategyResolveCompletedContext
                { Context = context, Strategy = strategy, Identifier = identifier };
            await options.CurrentValue.Events.OnStrategyResolveCompleted(strategyResolveCompletedContext).ConfigureAwait(false);
            if (identifier is not null && strategyResolveCompletedContext.Identifier is null)
                tenantResoloverLogger.LogDebug("OnStrategyResolveCompleted set non-null Identifier to null");
            identifier = strategyResolveCompletedContext.Identifier;
            
            if (options.CurrentValue.IgnoredIdentifiers.Contains(identifier, StringComparer.OrdinalIgnoreCase))
            {
                tenantResoloverLogger.LogDebug("Ignored identifier: {Identifier}", identifier);               
                identifier = null;
            }
            
            if (identifier == null)
                continue;

            foreach (var store in Stores)
            {
                var storeLogger = loggerFactory?.CreateLogger(store.GetType()) ?? NullLogger.Instance;

                var wrappedStore = new MultiTenantStoreWrapper<TTenantInfo>(store, storeLogger);
                var tenantInfo = await wrappedStore.TryGetByIdentifierAsync(identifier).ConfigureAwait(false);

                var storeResolveCompletedContext = new StoreResolveCompletedContext<TTenantInfo>
                    { Context = context, Store = store, Strategy = strategy, Identifier = identifier, TenantInfo = tenantInfo };
                await options.CurrentValue.Events.OnStoreResolveCompleted(storeResolveCompletedContext).ConfigureAwait(false);
                if (tenantInfo is not null && storeResolveCompletedContext.TenantInfo is null)
                    tenantResoloverLogger.LogDebug("OnStoreResolveCompleted set non-null TenantInfo to null");
                tenantInfo = storeResolveCompletedContext.TenantInfo;

                if (tenantInfo != null)
                {
                    mtc.StoreInfo = new StoreInfo<TTenantInfo> { Store = store, StoreType = store.GetType() };
                    mtc.StrategyInfo = new StrategyInfo { Strategy = strategy, StrategyType = strategy.GetType() };
                    mtc.TenantInfo = tenantInfo;
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