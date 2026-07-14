// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Collections.Concurrent;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.Options;

namespace Finbuckle.MultiTenant.Options;

/// <summary>
/// Adds, retrieves, and removes instances of TOptions after adjusting them for the current TenantContext.
/// </summary>
public class MultiTenantOptionsCache<TOptions> : IOptionsMonitorCache<TOptions>
    where TOptions : class
{
    private readonly IMultiTenantContextAccessor multiTenantContextAccessor;
    
    private readonly ConcurrentDictionary<string, IOptionsMonitorCache<TOptions>> map = new();

    /// <summary>
    /// Constructs a new instance of MultiTenantOptionsCache.
    /// </summary>
    /// <param name="multiTenantContextAccessor"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public MultiTenantOptionsCache(IMultiTenantContextAccessor multiTenantContextAccessor)
    {
        this.multiTenantContextAccessor = multiTenantContextAccessor ??
                                          throw new ArgumentNullException(nameof(multiTenantContextAccessor));
    }

    /// <summary>
    /// Clears all cached options for the current tenant.
    /// </summary>
    public void Clear()
    {
        var tenantId = multiTenantContextAccessor.MultiTenantContext?.TenantInfo?.Id ?? "";
        map.TryRemove(tenantId, out _);
    }

    /// <summary>
    /// Clears all cached options for the given tenant.
    /// </summary>
    /// <param name="tenantId">The Id of the tenant which will have its options cleared.</param>
    public void Clear(string tenantId)
    {
        map.TryRemove(tenantId, out _);
    }

    /// <summary>
    /// Clears all cached options for all tenants and no tenant.
    /// </summary>
    public void ClearAll() => map.Clear();

    /// <summary>
    /// Gets a named options instance for the current tenant, or adds a new instance created with createOptions.
    /// </summary>
    /// <param name="name">The options name.</param>
    /// <param name="createOptions">The factory function for creating the options instance.</param>
    /// <returns>The existing or new options instance.</returns>
    public TOptions GetOrAdd(string? name, Func<TOptions> createOptions)
    {
        ArgumentNullException.ThrowIfNull(createOptions);

        name ??= Microsoft.Extensions.Options.Options.DefaultName;
        var tenantId = multiTenantContextAccessor.MultiTenantContext?.TenantInfo?.Id ?? "";
        var cache = map.GetOrAdd(tenantId, new OptionsCache<TOptions>());

        return cache.GetOrAdd(name, createOptions);
    }

    /// <summary>
    /// Tries to adds a new option to the cache for the current tenant.
    /// </summary>
    /// <param name="name">The options name.</param>
    /// <param name="options">The options instance.</param>
    /// <returns>True if the options was added to the cache for the current tenant.</returns>
    public bool TryAdd(string? name, TOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        name ??= Microsoft.Extensions.Options.Options.DefaultName;
        var tenantId = multiTenantContextAccessor.MultiTenantContext?.TenantInfo?.Id ?? "";
        var cache = map.GetOrAdd(tenantId, new OptionsCache<TOptions>());

        return cache.TryAdd(name, options);
    }

    /// <summary>
    /// Tries to remove a named options instance for all tenants and no tenant.
    /// </summary>
    /// <param name="name">The options name.</param>
    /// <returns>True if the options was removed from at least one tenant cache.</returns>
    public bool TryRemove(string? name)
    {
        name ??= Microsoft.Extensions.Options.Options.DefaultName;
        var removed = false;

        foreach (var cache in map.Values)
            removed |= cache.TryRemove(name);

        return removed;
    }
}
