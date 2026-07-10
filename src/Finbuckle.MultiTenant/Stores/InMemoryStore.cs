// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant.Stores;

/// <summary>
/// Basic store that keeps tenants in memory.
/// </summary>
/// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
public class InMemoryStore<TTenantInfo> : IMultiTenantStore<TTenantInfo>
    where TTenantInfo : ITenantInfo
{
    private readonly Dictionary<string, TTenantInfo> _tenantMap = new(StringComparer.OrdinalIgnoreCase);
    private readonly Lock _tenantMapLock = new();

    /// <summary>
    /// Constructor for InMemoryStore.
    /// </summary>
    public InMemoryStore()
    {
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
        ValidateTenantInfo(tenantInfo);

        lock (_tenantMapLock)
        {
            if (_tenantMap.Values.Any(existing => existing.Id == tenantInfo.Id))
                return Task.FromResult(false);

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
                                   _tenantMap.Remove(existingTenantInfo.Identifier));
        }
    }

    /// <inheritdoc />
    public Task<bool> RemoveByIdentifierAsync(string identifier, CancellationToken cancellationToken = default)
    {
        lock (_tenantMapLock)
        {
            return Task.FromResult(_tenantMap.Remove(identifier));
        }
    }

    /// <inheritdoc />
    public Task<bool> UpdateAsync(TTenantInfo tenantInfo, CancellationToken cancellationToken = default)
    {
        ValidateTenantInfo(tenantInfo);

        lock (_tenantMapLock)
        {
            var existingTenantInfo = _tenantMap.Values.SingleOrDefault(ti => ti.Id == tenantInfo.Id);
            if (existingTenantInfo?.Identifier is null)
                return Task.FromResult(false);

            if (_tenantMap.Comparer.Equals(existingTenantInfo.Identifier, tenantInfo.Identifier))
            {
                _tenantMap[existingTenantInfo.Identifier] = tenantInfo;
                return Task.FromResult(true);
            }

            if (_tenantMap.ContainsKey(tenantInfo.Identifier))
                return Task.FromResult(false);

            _tenantMap.Remove(existingTenantInfo.Identifier);
            _tenantMap.Add(tenantInfo.Identifier, tenantInfo);
            return Task.FromResult(true);
        }
    }

    private static void ValidateTenantInfo(TTenantInfo tenantInfo)
    {
        ArgumentNullException.ThrowIfNull(tenantInfo);

        if (string.IsNullOrWhiteSpace(tenantInfo.Id))
            throw new MultiTenantException("Missing tenant id.");
        if (string.IsNullOrWhiteSpace(tenantInfo.Identifier))
            throw new MultiTenantException("Missing tenant identifier.");
    }
}
