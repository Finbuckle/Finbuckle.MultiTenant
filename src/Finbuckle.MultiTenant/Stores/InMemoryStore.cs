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

    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly InMemoryStoreOptions<TTenantInfo> _options;

    /// <summary>
    /// Constructor for InMemoryStore.
    /// </summary>
    /// <param name="options"><see cref="InMemoryStoreOptions{TTenantInfo}"/> instance for desired behavior.</param>
    /// <exception cref="MultiTenantException">Thrown when tenant configuration is invalid.</exception>
    public InMemoryStore(IOptions<InMemoryStoreOptions<TTenantInfo>> options)
    {
        _options = options.Value;

        var stringComparer = StringComparer.OrdinalIgnoreCase;
        if (_options.IsCaseSensitive)
            stringComparer = StringComparer.Ordinal;

        _tenantMap = new ConcurrentDictionary<string, TTenantInfo>(stringComparer);
        foreach (var tenant in _options.Tenants)
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
    public async Task<TTenantInfo?> GetAsync(string id)
    {
        var result = _tenantMap.Values.SingleOrDefault(ti => ti.Id == id);
        return await Task.FromResult(result).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<TTenantInfo?> GetByIdentifierAsync(string identifier)
    {
        _tenantMap.TryGetValue(identifier, out var result);

        return await Task.FromResult(result).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TTenantInfo>> GetAllAsync()
    {
        return await Task.FromResult(_tenantMap.Select(x => x.Value).ToList()).ConfigureAwait(false);
    }

    /// <summary>
    /// Not implemented in this implementation.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public Task<IEnumerable<TTenantInfo>> GetAllAsync(int take, int skip)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async Task<bool> AddAsync(TTenantInfo tenantInfo)
    {
        var result = _tenantMap.TryAdd(tenantInfo.Identifier, tenantInfo);

        return await Task.FromResult(result).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<bool> RemoveAsync(string identifier)
    {
        var result = _tenantMap.TryRemove(identifier, out var _);

        return await Task.FromResult(result).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<bool> UpdateAsync(TTenantInfo tenantInfo)
    {
        var existingTenantInfo = await GetAsync(tenantInfo.Id).ConfigureAwait(false);

        if (existingTenantInfo?.Identifier != null)
        {
            var result = _tenantMap.TryUpdate(existingTenantInfo.Identifier, tenantInfo, existingTenantInfo);
            return await Task.FromResult(result).ConfigureAwait(false);
        }

        return await Task.FromResult(false).ConfigureAwait(false);
    }
}