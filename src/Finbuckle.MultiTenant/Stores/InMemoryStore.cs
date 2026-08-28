// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant.Stores;

/// <summary>
/// Basic store that keeps tenants in memory.
/// </summary>
/// <typeparam name="TTenantInfo">The <see cref="ITenantInfo{TId}"/> implementation type.</typeparam>
/// <typeparam name="TId">The ID implementation type.</typeparam>
/// <remarks>
/// Lookups are keyed by the <c>Id</c> and <c>Identifier</c> values captured when a tenant is added or updated, not by
/// live reads of the stored instance, so later mutation of an instance handed out by the store cannot corrupt the
/// store's indexes.
/// </remarks>
public class InMemoryStore<TTenantInfo, TId> : IMultiTenantStore<TTenantInfo, TId>
    where TTenantInfo : ITenantInfo<TId> where TId : IEquatable<TId>
{
    // Identifier (case-insensitive) -> tenant. The key is a snapshot taken on add/update.
    private readonly Dictionary<string, TTenantInfo> _tenantMap = new(StringComparer.OrdinalIgnoreCase);

    // Id -> identifier snapshot, used for id based lookups without reading the stored instance.
    private readonly Dictionary<TId, string> _identifierById = new();
    private readonly Lock _tenantMapLock = new();

    /// <summary>
    /// Constructor for InMemoryStore.
    /// </summary>
    public InMemoryStore()
    {
    }

    /// <inheritdoc />
    public Task<TTenantInfo?> GetAsync(TId id, CancellationToken cancellationToken = default)
    {
        if (EqualityComparer<TId>.Default.Equals(id, default!))
            throw new ArgumentException("Tenant id cannot be the default value.", nameof(id));

        lock (_tenantMapLock)
        {
            TTenantInfo? result = default;
            if (_identifierById.TryGetValue(id, out var identifier))
                _tenantMap.TryGetValue(identifier, out result);

            return Task.FromResult(result);
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
    public Task<IEnumerable<TTenantInfo>> GetAllAsync(int take, int skip,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<bool> AddAsync(TTenantInfo tenantInfo, CancellationToken cancellationToken = default)
    {
        tenantInfo.EnsureValid();

        // snapshot the keys once so the stored instance is never read again
        var id = tenantInfo.Id;
        var identifier = tenantInfo.Identifier;

        lock (_tenantMapLock)
        {
            if (_identifierById.ContainsKey(id) || _tenantMap.ContainsKey(identifier))
                return Task.FromResult(false);

            _tenantMap.Add(identifier, tenantInfo);
            _identifierById.Add(id, identifier);
            return Task.FromResult(true);
        }
    }

    /// <inheritdoc />
    public Task<bool> RemoveAsync(TId id, CancellationToken cancellationToken = default)
    {
        if (EqualityComparer<TId>.Default.Equals(id, default!))
            throw new ArgumentException("Tenant id cannot be the default value.", nameof(id));

        lock (_tenantMapLock)
        {
            if (!_identifierById.Remove(id, out var identifier))
                return Task.FromResult(false);

            _tenantMap.Remove(identifier);
            return Task.FromResult(true);
        }
    }

    /// <inheritdoc />
    public Task<bool> RemoveByIdentifierAsync(string identifier, CancellationToken cancellationToken = default)
    {
        lock (_tenantMapLock)
        {
            if (!_tenantMap.Remove(identifier))
                return Task.FromResult(false);

            var id = _identifierById.Single(kv => _tenantMap.Comparer.Equals(kv.Value, identifier)).Key;
            _identifierById.Remove(id);
            return Task.FromResult(true);
        }
    }

    /// <inheritdoc />
    public Task<bool> UpdateAsync(TTenantInfo tenantInfo, CancellationToken cancellationToken = default)
    {
        tenantInfo.EnsureValid();

        var id = tenantInfo.Id;
        var newIdentifier = tenantInfo.Identifier;

        lock (_tenantMapLock)
        {
            if (!_identifierById.TryGetValue(id, out var existingIdentifier))
                return Task.FromResult(false);

            // a different tenant already owns the new identifier
            if (!_tenantMap.Comparer.Equals(existingIdentifier, newIdentifier) && _tenantMap.ContainsKey(newIdentifier))
                return Task.FromResult(false);

            // re-add so a case-only identifier change is reflected in the key
            _tenantMap.Remove(existingIdentifier);
            _tenantMap.Add(newIdentifier, tenantInfo);
            _identifierById[id] = newIdentifier;
            return Task.FromResult(true);
        }
    }
}
