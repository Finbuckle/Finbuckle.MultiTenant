// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Finbuckle.MultiTenant;

/// <summary>
/// Provides the main API for tenant store interaction.
/// </summary>
/// <typeparam name="TTenantInfo">The <see cref="ITenantInfo{TId}"/> implementation type.</typeparam>
/// <typeparam name="TId">The ID implementation type.</typeparam>
public class TenantManager<TTenantInfo, TId>
    where TTenantInfo : ITenantInfo<TId> where TId : IEquatable<TId>
{
    private readonly IMultiTenantStore<TTenantInfo, TId> _store;
    private readonly IReadOnlyList<IMultiTenantStoreCache<TTenantInfo, TId>> _caches;
    private readonly ILoggerFactory? _loggerFactory;

    /// <summary>
    /// Initializes a new instance of TenantManager.
    /// </summary>
    /// <param name="store">The primary tenant store.</param>
    /// <param name="caches">The configured tenant store caches.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    public TenantManager(IMultiTenantStore<TTenantInfo, TId> store,
        IEnumerable<IMultiTenantStoreCache<TTenantInfo, TId>> caches,
        ILoggerFactory? loggerFactory = null)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _caches = caches.ToArray() ?? throw new ArgumentNullException(nameof(caches));
        _loggerFactory = loggerFactory;
    }

    /// <summary>
    /// Gets the configured primary tenant store.
    /// </summary>
    public IMultiTenantStore<TTenantInfo, TId> Store => _store;

    /// <summary>
    /// Gets the configured tenant store caches.
    /// </summary>
    public IEnumerable<IMultiTenantStoreCache<TTenantInfo, TId>> Caches => _caches;

    /// <summary>
    /// Try to add the TTenantInfo to the primary store.
    /// </summary>
    /// <param name="tenantInfo">New TTenantInfo instance to add.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>True if successfully added.</returns>
    public async Task<bool> AddAsync(TTenantInfo tenantInfo, CancellationToken cancellationToken = default)
    {
        var result = await AddToStoreAsync(tenantInfo, cancellationToken).ConfigureAwait(false);
        if (result)
            await InvalidateAsync(tenantInfo, cancellationToken).ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Try to update the TTenantInfo in the primary store.
    /// </summary>
    /// <param name="tenantInfo">Existing TTenantInfo instance to update.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>True if successfully updated.</returns>
    public async Task<bool> UpdateAsync(TTenantInfo tenantInfo, CancellationToken cancellationToken = default)
    {
        tenantInfo.EnsureValid();

        var existing = await GetFromStoreAsync(tenantInfo.Id, cancellationToken).ConfigureAwait(false);
        var result = await UpdateStoreAsync(tenantInfo, existing, cancellationToken).ConfigureAwait(false);
        if (result)
        {
            if (existing is not null)
                await InvalidateAsync(existing, cancellationToken).ConfigureAwait(false);

            await InvalidateAsync(tenantInfo, cancellationToken).ConfigureAwait(false);
        }

        return result;
    }

    /// <summary>
    /// Try to remove the TTenantInfo from the primary store by tenant Id.
    /// </summary>
    /// <param name="id">TenantId for the tenant to remove.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>True if successfully removed.</returns>
    public async Task<bool> RemoveAsync(TId id, CancellationToken cancellationToken = default)
    {
        if (EqualityComparer<TId>.Default.Equals(id, default!))
            throw new ArgumentException("Tenant id cannot be the default value.", nameof(id));

        var existing = await GetFromStoreAsync(id, cancellationToken).ConfigureAwait(false);
        var result = await RemoveFromStoreAsync(id, cancellationToken).ConfigureAwait(false);
        if (result)
        {
            if (existing is not null)
                await InvalidateAsync(existing, cancellationToken).ConfigureAwait(false);
            else
                await InvalidateByIdAsync(id, cancellationToken).ConfigureAwait(false);
        }

        return result;
    }

    /// <summary>
    /// Try to remove the TTenantInfo from the primary store by identifier.
    /// </summary>
    /// <param name="identifier">Identifier for the tenant to remove.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>True if successfully removed.</returns>
    public async Task<bool> RemoveByIdentifierAsync(string identifier, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(identifier);

        var existing = await GetByIdentifierFromStoreAsync(identifier, cancellationToken).ConfigureAwait(false);
        var result = await RemoveByIdentifierFromStoreAsync(identifier, cancellationToken).ConfigureAwait(false);
        if (result)
        {
            if (existing is not null)
                await InvalidateAsync(existing, cancellationToken).ConfigureAwait(false);
            else
                await InvalidateByIdentifierAsync(identifier, cancellationToken).ConfigureAwait(false);
        }

        return result;
    }

    /// <summary>
    /// Retrieve the TTenantInfo for a given identifier.
    /// </summary>
    /// <param name="identifier">Identifier for the tenant to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The found TTenantInfo instance or null if none found.</returns>
    public Task<TTenantInfo?> GetByIdentifierAsync(string identifier, CancellationToken cancellationToken = default)
    {
        return GetByIdentifierAsync(identifier, null, cancellationToken);
    }

    /// <summary>
    /// Retrieve the TTenantInfo for a given tenant Id.
    /// </summary>
    /// <param name="id">TenantId for the tenant to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The found TTenantInfo instance or null if none found.</returns>
    public async Task<TTenantInfo?> GetAsync(TId id, CancellationToken cancellationToken = default)
    {
        if (EqualityComparer<TId>.Default.Equals(id, default!))
            throw new ArgumentException("Tenant id cannot be the default value.", nameof(id));

        var missedCaches = new List<IMultiTenantStoreCache<TTenantInfo, TId>>();
        foreach (var cache in _caches)
        {
            var result = await GetFromCacheAsync(cache, id, cancellationToken).ConfigureAwait(false);
            if (result is not null)
            {
                await FillCachesAsync(missedCaches, result, cancellationToken).ConfigureAwait(false);
                return result;
            }

            missedCaches.Add(cache);
        }

        var tenantInfo = await GetFromStoreAsync(id, cancellationToken).ConfigureAwait(false);
        if (tenantInfo is not null)
            await FillCachesAsync(missedCaches, tenantInfo, cancellationToken).ConfigureAwait(false);

        return tenantInfo;
    }

    /// <summary>
    /// Retrieve all the TTenantInfo's from the primary store.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An IEnumerable of all tenants in the store.</returns>
    public Task<IEnumerable<TTenantInfo>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return GetAllFromStoreAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieve a subset of the TTenantInfo's from the primary store.
    /// </summary>
    /// <param name="take">Number of elements to take from the list.</param>
    /// <param name="skip">Number of elements to skip from the list.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An IEnumerable of applicable tenants in the store.</returns>
    public Task<IEnumerable<TTenantInfo>> GetAllAsync(int take, int skip,
        CancellationToken cancellationToken = default)
    {
        return GetAllFromStoreAsync(take, skip, cancellationToken);
    }

    internal async Task<TTenantInfo?> GetByIdentifierAsync(string identifier,
        Func<TenantStoreLookupInfo<TTenantInfo, TId>, Task<TTenantInfo?>>? lookupCompleted,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(identifier);

        var missedCaches = new List<IMultiTenantStoreCache<TTenantInfo, TId>>();
        foreach (var cache in _caches)
        {
            var tenantInfo = await GetByIdentifierFromCacheAsync(cache, identifier, cancellationToken)
                .ConfigureAwait(false);
            tenantInfo = await CompleteCacheLookupAsync(cache, identifier, tenantInfo, lookupCompleted)
                .ConfigureAwait(false);

            if (tenantInfo is not null)
            {
                await FillCachesAsync(missedCaches, tenantInfo, cancellationToken).ConfigureAwait(false);
                return tenantInfo;
            }

            missedCaches.Add(cache);
        }

        var storeResult = await GetByIdentifierFromStoreAsync(identifier, cancellationToken).ConfigureAwait(false);
        storeResult = await CompleteStoreLookupAsync(identifier, storeResult, lookupCompleted).ConfigureAwait(false);

        if (storeResult is not null)
            await FillCachesAsync(missedCaches, storeResult, cancellationToken).ConfigureAwait(false);

        return storeResult;
    }

    private static async Task<TTenantInfo?> CompleteCacheLookupAsync(IMultiTenantStoreCache<TTenantInfo, TId> cache,
        string identifier, TTenantInfo? tenantInfo,
        Func<TenantStoreLookupInfo<TTenantInfo, TId>, Task<TTenantInfo?>>? lookupCompleted)
    {
        if (lookupCompleted is null)
            return tenantInfo;

        return await lookupCompleted(new TenantStoreLookupInfo<TTenantInfo, TId>
        {
            Cache = cache,
            Identifier = identifier,
            TenantInfo = tenantInfo
        }).ConfigureAwait(false);
    }

    private async Task<TTenantInfo?> CompleteStoreLookupAsync(string identifier, TTenantInfo? tenantInfo,
        Func<TenantStoreLookupInfo<TTenantInfo, TId>, Task<TTenantInfo?>>? lookupCompleted)
    {
        if (lookupCompleted is null)
            return tenantInfo;

        return await lookupCompleted(new TenantStoreLookupInfo<TTenantInfo, TId>
        {
            Store = _store,
            Identifier = identifier,
            TenantInfo = tenantInfo
        }).ConfigureAwait(false);
    }

    private async Task FillCachesAsync(IEnumerable<IMultiTenantStoreCache<TTenantInfo, TId>> cachesToFill,
        TTenantInfo tenantInfo, CancellationToken cancellationToken)
    {
        foreach (var cache in cachesToFill)
            await SetCacheAsync(cache, tenantInfo, cancellationToken).ConfigureAwait(false);
    }

    private async Task InvalidateAsync(TTenantInfo tenantInfo, CancellationToken cancellationToken)
    {
        await InvalidateByIdAsync(tenantInfo.Id, cancellationToken).ConfigureAwait(false);
        await InvalidateByIdentifierAsync(tenantInfo.Identifier, cancellationToken).ConfigureAwait(false);
    }

    private async Task InvalidateByIdAsync(TId id, CancellationToken cancellationToken)
    {
        foreach (var cache in _caches)
            await RemoveFromCacheAsync(cache, id, cancellationToken).ConfigureAwait(false);
    }

    private async Task InvalidateByIdentifierAsync(string identifier, CancellationToken cancellationToken)
    {
        foreach (var cache in _caches)
            await RemoveByIdentifierFromCacheAsync(cache, identifier, cancellationToken).ConfigureAwait(false);
    }

    private async Task<TTenantInfo?> GetFromStoreAsync(TId id, CancellationToken cancellationToken)
    {
        if (EqualityComparer<TId>.Default.Equals(id, default!))
            throw new ArgumentException("Tenant id cannot be the default value.", nameof(id));

        var logger = GetLogger(_store);
        TTenantInfo? result = default;

        try
        {
            result = await _store.GetAsync(id, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Exception in {nameof(IMultiTenantStore<TTenantInfo, TId>.GetAsync)}");
        }

        if (logger.IsEnabled(LogLevel.Debug))
        {
            if (result is not null)
                logger.LogDebug($"{nameof(IMultiTenantStore<TTenantInfo, TId>.GetAsync)}: Tenant Id \"{{TenantId}}\" found.", id);
            else
                logger.LogDebug($"{nameof(IMultiTenantStore<TTenantInfo, TId>.GetAsync)}: Unable to find Tenant Id \"{{TenantId}}\".", id);
        }

        return result;
    }

    private async Task<TTenantInfo?> GetByIdentifierFromStoreAsync(string identifier, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(identifier);

        var logger = GetLogger(_store);
        TTenantInfo? result = default;

        try
        {
            result = await _store.GetByIdentifierAsync(identifier, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Exception in {nameof(IMultiTenantStore<TTenantInfo, TId>.GetByIdentifierAsync)}");
        }

        if (logger.IsEnabled(LogLevel.Debug))
        {
            if (result is not null)
                logger.LogDebug(
                    $"{nameof(IMultiTenantStore<TTenantInfo, TId>.GetByIdentifierAsync)}: Tenant found with identifier \"{{TenantIdentifier}}\"",
                    identifier);
            else
                logger.LogDebug(
                    $"{nameof(IMultiTenantStore<TTenantInfo, TId>.GetByIdentifierAsync)}: Unable to find Tenant with identifier \"{{TenantIdentifier}}\"",
                    identifier);
        }

        return result;
    }

    private async Task<IEnumerable<TTenantInfo>> GetAllFromStoreAsync(CancellationToken cancellationToken)
    {
        var logger = GetLogger(_store);

        try
        {
            return await _store.GetAllAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Exception in {nameof(IMultiTenantStore<TTenantInfo, TId>.GetAllAsync)}");
            return [];
        }
    }

    private async Task<IEnumerable<TTenantInfo>> GetAllFromStoreAsync(int take, int skip,
        CancellationToken cancellationToken)
    {
        var logger = GetLogger(_store);

        try
        {
            return await _store.GetAllAsync(take, skip, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Exception in {nameof(IMultiTenantStore<TTenantInfo, TId>.GetAllAsync)}");
            return [];
        }
    }

    private async Task<bool> AddToStoreAsync(TTenantInfo tenantInfo, CancellationToken cancellationToken)
    {
        tenantInfo.EnsureValid();

        var logger = GetLogger(_store);
        var result = false;

        try
        {
            var existing = await GetFromStoreAsync(tenantInfo.Id, cancellationToken).ConfigureAwait(false);
            if (existing is not null)
            {
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug(
                        $"{nameof(IMultiTenantStore<TTenantInfo, TId>.AddAsync)}: Tenant already exists. Id: \"{{TenantId}}\", Identifier: \"{{TenantIdentifier}}\"",
                        tenantInfo.Id, tenantInfo.Identifier);
                }
            }
            else
            {
                existing = await GetByIdentifierFromStoreAsync(tenantInfo.Identifier, cancellationToken)
                    .ConfigureAwait(false);
                if (existing is not null)
                {
                    if (logger.IsEnabled(LogLevel.Debug))
                    {
                        logger.LogDebug(
                            $"{nameof(IMultiTenantStore<TTenantInfo, TId>.AddAsync)}: Tenant already exists. Id: \"{{TenantId}}\", Identifier: \"{{TenantIdentifier}}\"",
                            tenantInfo.Id, tenantInfo.Identifier);
                    }
                }
                else
                    result = await _store.AddAsync(tenantInfo, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Exception in {nameof(IMultiTenantStore<TTenantInfo, TId>.AddAsync)}");
        }

        if (logger.IsEnabled(LogLevel.Debug))
        {
            if (result)
                logger.LogDebug(
                    $"{nameof(IMultiTenantStore<TTenantInfo, TId>.AddAsync)}: Tenant added. Id: \"{{TenantId}}\", Identifier: \"{{TenantIdentifier}}\"",
                    tenantInfo.Id, tenantInfo.Identifier);
            else
                logger.LogDebug(
                    $"{nameof(IMultiTenantStore<TTenantInfo, TId>.AddAsync)}: Unable to add Tenant. Id: \"{{TenantId}}\", Identifier: \"{{TenantIdentifier}}\"",
                    tenantInfo.Id, tenantInfo.Identifier);
        }

        return result;
    }

    private async Task<bool> UpdateStoreAsync(TTenantInfo tenantInfo, TTenantInfo? existing,
        CancellationToken cancellationToken)
    {
        tenantInfo.EnsureValid();

        var logger = GetLogger(_store);
        var result = false;

        try
        {
            if (existing is null)
            {
                if (logger.IsEnabled(LogLevel.Debug))
                    logger.LogDebug($"{nameof(IMultiTenantStore<TTenantInfo, TId>.UpdateAsync)}: Tenant Id: \"{{TenantId}}\" not found", tenantInfo.Id);
            }
            else
                result = await _store.UpdateAsync(tenantInfo, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Exception in {nameof(IMultiTenantStore<TTenantInfo, TId>.UpdateAsync)}");
        }

        if (logger.IsEnabled(LogLevel.Debug))
        {
            if (result)
                logger.LogDebug($"{nameof(IMultiTenantStore<TTenantInfo, TId>.UpdateAsync)}: Tenant Id: \"{{TenantId}}\" updated",
                    tenantInfo.Id);
            else
                logger.LogDebug(
                    $"{nameof(IMultiTenantStore<TTenantInfo, TId>.UpdateAsync)}: Unable to update Tenant Id: \"{{TenantId}}\"",
                    tenantInfo.Id);
        }

        return result;
    }

    private async Task<bool> RemoveFromStoreAsync(TId id, CancellationToken cancellationToken)
    {
        if (EqualityComparer<TId>.Default.Equals(id, default!))
            throw new ArgumentException("Tenant id cannot be the default value.", nameof(id));

        var logger = GetLogger(_store);
        var result = false;

        try
        {
            result = await _store.RemoveAsync(id, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Exception in {nameof(IMultiTenantStore<TTenantInfo, TId>.RemoveAsync)}");
        }

        if (logger.IsEnabled(LogLevel.Debug))
        {
            if (result)
                logger.LogDebug(
                    $"{nameof(IMultiTenantStore<TTenantInfo, TId>.RemoveAsync)}: Tenant Id: \"{{TenantId}}\" removed",
                    id);
            else
                logger.LogDebug(
                    $"{nameof(IMultiTenantStore<TTenantInfo, TId>.RemoveAsync)}: Unable to remove Tenant Id: \"{{TenantId}}\"",
                    id);
        }

        return result;
    }

    private async Task<bool> RemoveByIdentifierFromStoreAsync(string identifier, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(identifier);

        var logger = GetLogger(_store);
        var result = false;

        try
        {
            result = await _store.RemoveByIdentifierAsync(identifier, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Exception in {nameof(IMultiTenantStore<TTenantInfo, TId>.RemoveByIdentifierAsync)}");
        }

        if (logger.IsEnabled(LogLevel.Debug))
        {
            if (result)
                logger.LogDebug(
                    $"{nameof(IMultiTenantStore<TTenantInfo, TId>.RemoveByIdentifierAsync)}: Tenant Identifier: \"{{TenantIdentifier}}\" removed",
                    identifier);
            else
                logger.LogDebug(
                    $"{nameof(IMultiTenantStore<TTenantInfo, TId>.RemoveByIdentifierAsync)}: Unable to remove Tenant Identifier: \"{{TenantIdentifier}}\"",
                    identifier);
        }

        return result;
    }

    private async Task<TTenantInfo?> GetFromCacheAsync(IMultiTenantStoreCache<TTenantInfo, TId> cache, TId id,
        CancellationToken cancellationToken)
    {
        if (EqualityComparer<TId>.Default.Equals(id, default!))
            throw new ArgumentException("Tenant id cannot be the default value.", nameof(id));

        try
        {
            return await cache.GetAsync(id, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            GetLogger(cache).LogError(e, $"Exception in {nameof(IMultiTenantStoreCache<TTenantInfo, TId>.GetAsync)}");
            return default;
        }
    }

    private async Task<TTenantInfo?> GetByIdentifierFromCacheAsync(IMultiTenantStoreCache<TTenantInfo, TId> cache,
        string identifier, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(identifier);

        try
        {
            return await cache.GetByIdentifierAsync(identifier, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            GetLogger(cache).LogError(e,
                $"Exception in {nameof(IMultiTenantStoreCache<TTenantInfo, TId>.GetByIdentifierAsync)}");
            return default;
        }
    }

    private async Task SetCacheAsync(IMultiTenantStoreCache<TTenantInfo, TId> cache, TTenantInfo tenantInfo,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tenantInfo);

        try
        {
            await cache.SetAsync(tenantInfo, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            GetLogger(cache).LogError(e, $"Exception in {nameof(IMultiTenantStoreCache<TTenantInfo, TId>.SetAsync)}");
        }
    }

    private async Task RemoveFromCacheAsync(IMultiTenantStoreCache<TTenantInfo, TId> cache, TId id,
        CancellationToken cancellationToken)
    {
        if (EqualityComparer<TId>.Default.Equals(id, default!))
            throw new ArgumentException("Tenant id cannot be the default value.", nameof(id));

        try
        {
            await cache.RemoveAsync(id, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            GetLogger(cache).LogError(e, $"Exception in {nameof(IMultiTenantStoreCache<TTenantInfo, TId>.RemoveAsync)}");
        }
    }

    private async Task RemoveByIdentifierFromCacheAsync(IMultiTenantStoreCache<TTenantInfo, TId> cache, string identifier,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(identifier);

        try
        {
            await cache.RemoveByIdentifierAsync(identifier, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            GetLogger(cache).LogError(e,
                $"Exception in {nameof(IMultiTenantStoreCache<TTenantInfo, TId>.RemoveByIdentifierAsync)}");
        }
    }

    private ILogger GetLogger(object source)
    {
        return _loggerFactory?.CreateLogger(source.GetType()) ?? NullLogger.Instance;
    }
}
