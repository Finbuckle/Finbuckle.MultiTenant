// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.Stores;
using Finbuckle.MultiTenant.Strategies;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Finbuckle.MultiTenant;

public class TenantResolver<T> : ITenantResolver<T>, ITenantResolver
    where T : class, ITenantInfo, new()
{
    private readonly IOptionsMonitor<MultiTenantOptions> options;
    private readonly ILoggerFactory? loggerFactory;

    public TenantResolver(IEnumerable<IMultiTenantStrategy> strategies, IEnumerable<IMultiTenantStore<T>> stores, IOptionsMonitor<MultiTenantOptions> options) :
        this(strategies, stores, options, null)
    {
    }

    public TenantResolver(IEnumerable<IMultiTenantStrategy> strategies, IEnumerable<IMultiTenantStore<T>> stores, IOptionsMonitor<MultiTenantOptions> options, ILoggerFactory? loggerFactory)
    {
        Stores = stores;
        this.options = options;
        this.loggerFactory = loggerFactory;

        Strategies = strategies.OrderByDescending(s => s.Priority);
    }

    public IEnumerable<IMultiTenantStrategy> Strategies { get; set; }
    public IEnumerable<IMultiTenantStore<T>> Stores { get; set; }

    public async Task<IMultiTenantContext<T>?> ResolveAsync(object context)
    {
        IMultiTenantContext<T>? result = null;

        foreach (var strategy in Strategies)
        {
            var _strategy = new MultiTenantStrategyWrapper(strategy, loggerFactory?.CreateLogger(strategy.GetType()) ?? NullLogger.Instance);
            var identifier = await _strategy.GetIdentifierAsync(context);

            if (options.CurrentValue.IgnoredIdentifiers.Contains(identifier, StringComparer.OrdinalIgnoreCase))
            {
                (loggerFactory?.CreateLogger(GetType()) ?? NullLogger.Instance).LogInformation("Ignored identifier: {Identifier}", identifier);
                identifier = null;
            }

            if (identifier != null)
            {
                foreach (var store in Stores)
                {
                    var _store = new MultiTenantStoreWrapper<T>(store, loggerFactory?.CreateLogger(store.GetType()) ?? NullLogger.Instance);
                    var tenantInfo = await _store.TryGetByIdentifierAsync(identifier);
                    if (tenantInfo != null)
                    {
                        result = new MultiTenantContext<T>();
                        result.StoreInfo = new StoreInfo<T> { Store = store, StoreType = store.GetType() };
                        result.StrategyInfo = new StrategyInfo { Strategy = strategy, StrategyType = strategy.GetType() };
                        result.TenantInfo = tenantInfo;

                        await options.CurrentValue.Events.OnTenantResolved(new TenantResolvedContext { Context = 
                            context, TenantInfo = tenantInfo, StrategyType = strategy.GetType(), StoreType = store.GetType()});

                        break;
                    }
                }

                if (result != null)
                    break;

                await options.CurrentValue.Events.OnTenantNotResolved(new TenantNotResolvedContext { Context = context, Identifier = identifier });
            }
        }

        return result;
    }

    // TODO move this to the base interface?
    async Task<IMultiTenantContext?> ITenantResolver.ResolveAsync(object context)
    {
        return (await ResolveAsync(context)) as IMultiTenantContext;
    }
}