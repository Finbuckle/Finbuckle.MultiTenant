// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Collections.Concurrent;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.Options;

namespace Finbuckle.MultiTenant.Stores;

/// <summary>
/// Basic store that keeps tenants in memory.
/// </summary>
/// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
public class InMemoryStore<TTenantInfo> : IMultiTenantStore<TTenantInfo>
    where TTenantInfo : ITenantInfo
{
    private readonly ConcurrentDictionary<string, TTenantInfo> _tenantMap;
    private readonly Lock _tenantMapLock = new();

    /// <summary>
    /// Constructor for InMemoryStore.
    /// </summary>
    /// <param name="options"><see cref="InMemoryStoreOptions{TTenantInfo}"/> instance for desired behavior.</param>
    /// <exception cref="MultiTenantException">Thrown when tenant configuration is invalid.</exception>
    public InMemoryStore(IOptions<InMemoryStoreOptions<TTenantInfo>> options)
    {
        var options1 = options.Value;

        var stringComparer = StringComparer.OrdinalIgnoreCase;
        if (options1.IsCaseSensitive)
            stringComparer = StringComparer.Ordinal;

        _tenantMap = new ConcurrentDictionary<string, TTenantInfo>(stringComparer);
        foreach (var tenant in options1.Tenants)
        {
            if (String.IsNullOrWhiteSpace(tenant.Id))
                throw new MultiTenantException("Missing tenant id in options.");
            if (String.IsNullOrWhiteSpace(tenant.Identifier))
                throw new MultiTenantException("Missing tenant identifier in options.");
            if (_tenantMap.ContainsKey(tenant.Identifier))
                throw new MultiTenantException("Duplicate tenant identifier in options.");

            _tenantMap.TryAdd(tenant.Identifier, tenant);
        }
    }

    /// <inheritdoc />
    public Task<TTenantInfo?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        lock (_tenantMapLock)
        {
            return Task.FromResult(_tenantMap.Values.SingleOrDefault(ti => ti.Id == id));
        }
    }

    /// <inheritdoc />
    public Task<TTenantInfo?> GetByIdentifierAsync(string identifier, CancellationToken cancellationToken = default)
    {
        lock (_tenantMapLock)
        {
            _tenantMap.TryGetValue(identifier, out var result);
            return Task.FromResult(result);
        }
    }

    /// <inheritdoc />
    public Task<IEnumerable<TTenantInfo>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        lock (_tenantMapLock)
        {
            return Task.FromResult<IEnumerable<TTenantInfo>>(_tenantMap.Select(x => x.Value).ToList());
        }
    }

    /// <summary>
    /// Not implemented in this implementation.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public Task<IEnumerable<TTenantInfo>> GetAllAsync(int take, int skip, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<bool> AddAsync(TTenantInfo tenantInfo, CancellationToken cancellationToken = default)
    {
        lock (_tenantMapLock)
        {
            return Task.FromResult(_tenantMap.TryAdd(tenantInfo.Identifier, tenantInfo));
        }
    }

    /// <inheritdoc />
    public Task<bool> RemoveAsync(string id, CancellationToken cancellationToken = default)
    {
        lock (_tenantMapLock)
        {
            var existingTenantInfo = _tenantMap.Values.SingleOrDefault(ti => ti.Id == id);
            return Task.FromResult(existingTenantInfo?.Identifier is not null &&
                                   _tenantMap.TryRemove(existingTenantInfo.Identifier, out _));
        }
    }

    /// <inheritdoc />
    public Task<bool> RemoveByIdentifierAsync(string identifier, CancellationToken cancellationToken = default)
    {
        lock (_tenantMapLock)
        {
            return Task.FromResult(_tenantMap.TryRemove(identifier, out _));
        }
    }

    /// <inheritdoc />
    public Task<bool> UpdateAsync(TTenantInfo tenantInfo, CancellationToken cancellationToken = default)
    {
        lock (_tenantMapLock)
        {
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
