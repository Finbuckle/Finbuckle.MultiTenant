// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Collections.Concurrent;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.Options;

namespace Finbuckle.MultiTenant.Stores.InMemoryStore;

/// <summary>
/// Basic store that keeps tenants in memory.
/// </summary>
/// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
public class InMemoryStore<TTenantInfo> : IMultiTenantStore<TTenantInfo>
    where TTenantInfo : class, ITenantInfo, new()
{
    private readonly ConcurrentDictionary<string, TTenantInfo> _tenantMap;
    private readonly object _tenantMapLock = new();
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly InMemoryStoreOptions<TTenantInfo> _options;

    /// <summary>
    /// Constructor for InMemoryStore.
    /// </summary>
    /// <param name="options">InMemoryStoreOptions instance for desired behavior.</param>
    /// <exception cref="MultiTenantException"></exception>
    public InMemoryStore(IOptions<InMemoryStoreOptions<TTenantInfo>> options)
    {
        _options = options.Value;

        var stringComparer = StringComparer.OrdinalIgnoreCase;
        if(_options.IsCaseSensitive)
            stringComparer = StringComparer.Ordinal;

        _tenantMap = new ConcurrentDictionary<string, TTenantInfo>(stringComparer);
        foreach(var tenant in _options.Tenants)
        {
            if(String.IsNullOrWhiteSpace(tenant.Id))
                throw new MultiTenantException("Missing tenant id in options.");
            if(String.IsNullOrWhiteSpace(tenant.Identifier))
                throw new MultiTenantException("Missing tenant identifier in options.");
            if(_tenantMap.ContainsKey(tenant.Identifier))
                throw new MultiTenantException("Duplicate tenant identifier in options.");

            _tenantMap.TryAdd(tenant.Identifier, tenant);
        }
    }

    /// <inheritdoc />
    public Task<TTenantInfo?> TryGetAsync(string id)
    {
        lock (_tenantMapLock)
        {
            return Task.FromResult(_tenantMap.Values.SingleOrDefault(ti => ti.Id == id));
        }
    }

    /// <inheritdoc />
    public Task<TTenantInfo?> TryGetByIdentifierAsync(string identifier)
    {
        lock (_tenantMapLock)
        {
            _tenantMap.TryGetValue(identifier, out var result);
            return Task.FromResult(result);
        }
    }

    /// <inheritdoc />
    public Task<IEnumerable<TTenantInfo>> GetAllAsync()
    {
        lock (_tenantMapLock)
        {
            return Task.FromResult<IEnumerable<TTenantInfo>>(_tenantMap.Select(x => x.Value).ToList());
        }
    }

    /// <inheritdoc />
    public Task<bool> TryAddAsync(TTenantInfo tenantInfo)
    {
        lock (_tenantMapLock)
        {
            return Task.FromResult(tenantInfo.Identifier != null &&
                                   _tenantMap.TryAdd(tenantInfo.Identifier, tenantInfo));
        }
    }

    /// <inheritdoc />
    public Task<bool> TryRemoveAsync(string identifier)
    {
        lock (_tenantMapLock)
        {
            return Task.FromResult(_tenantMap.TryRemove(identifier, out _));
        }
    }

    /// <inheritdoc />
    public Task<bool> TryUpdateAsync(TTenantInfo tenantInfo)
    {
        lock (_tenantMapLock)
        {
            if (tenantInfo.Id is null || tenantInfo.Identifier is null)
                return Task.FromResult(false);

            var existingTenantInfo = _tenantMap.Values.SingleOrDefault(ti => ti.Id == tenantInfo.Id);
            if (existingTenantInfo?.Identifier is null)
                return Task.FromResult(false);

            if (_tenantMap.Comparer.Equals(existingTenantInfo.Identifier, tenantInfo.Identifier))
                return Task.FromResult(
                    _tenantMap.TryUpdate(existingTenantInfo.Identifier, tenantInfo, existingTenantInfo));

            if (_tenantMap.ContainsKey(tenantInfo.Identifier))
                return Task.FromResult(false);

            if (!_tenantMap.TryRemove(existingTenantInfo.Identifier, out var removedTenantInfo))
                return Task.FromResult(false);

            if (_tenantMap.TryAdd(tenantInfo.Identifier, tenantInfo))
                return Task.FromResult(true);

            _tenantMap.TryAdd(existingTenantInfo.Identifier, removedTenantInfo);
            return Task.FromResult(false);
        }
    }
}
