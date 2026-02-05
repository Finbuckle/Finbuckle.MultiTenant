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
    /// <param name="multiTenantContextAccessor">The multi-tenant context accessor.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="multiTenantContextAccessor"/> is null.</exception>
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
        var cache = map.GetOrAdd(tenantId, new OptionsCache<TOptions>());

        cache.Clear();
    }

    /// <summary>
    /// Clears all cached options for the given tenant.
    /// </summary>
    /// <param name="tenantId">The Id of the tenant which will have its options cleared.</param>
    public void Clear(string tenantId)
    {
        var cache = map.GetOrAdd(tenantId, new OptionsCache<TOptions>());

        cache.Clear();
    }

    /// <summary>
    /// Clears all cached options for all tenants and no tenant.
    /// </summary>
    public void ClearAll()
    {
        foreach (var cache in map.Values)
            cache.Clear();
    }

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
        name = name ?? Microsoft.Extensions.Options.Options.DefaultName;
        var tenantId = multiTenantContextAccessor.MultiTenantContext?.TenantInfo?.Id ?? "";
        var cache = map.GetOrAdd(tenantId, new OptionsCache<TOptions>());

        return cache.TryAdd(name, options);
    }

    /// <summary>
    /// Try to remove an options instance for the current tenant.
    /// </summary>
    /// <param name="name">The options name.</param>
    /// <returns>True if the options was removed from the cache for the current tenant.</returns>
    public bool TryRemove(string? name)
    {
        name = name ?? Microsoft.Extensions.Options.Options.DefaultName;
        var tenantId = multiTenantContextAccessor.MultiTenantContext?.TenantInfo?.Id ?? "";
        var cache = map.GetOrAdd(tenantId, new OptionsCache<TOptions>());

        return cache.TryRemove(name);
    }
}