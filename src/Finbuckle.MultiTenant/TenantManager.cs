// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Finbuckle.MultiTenant;

/// <summary>
/// Provides the main API for tenant store interaction.
/// </summary>
/// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
public class TenantManager<TTenantInfo>
    where TTenantInfo : ITenantInfo
{
    private readonly IMultiTenantStore<TTenantInfo> _store;
    private readonly IReadOnlyList<IMultiTenantStoreCache<TTenantInfo>> _caches;
    private readonly ILoggerFactory? _loggerFactory;

    /// <summary>
    /// Initializes a new instance of TenantManager.
    /// </summary>
    /// <param name="store">The primary tenant store.</param>
    /// <param name="caches">The configured tenant store caches.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    public TenantManager(IMultiTenantStore<TTenantInfo> store,
        IEnumerable<IMultiTenantStoreCache<TTenantInfo>> caches,
        ILoggerFactory? loggerFactory = null)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _caches = caches?.ToArray() ?? throw new ArgumentNullException(nameof(caches));
        _loggerFactory = loggerFactory;
    }

    /// <summary>
    /// Gets the configured primary tenant store.
    /// </summary>
    public IMultiTenantStore<TTenantInfo> Store => _store;

    /// <summary>
    /// Gets the configured tenant store caches.
    /// </summary>
    public IEnumerable<IMultiTenantStoreCache<TTenantInfo>> Caches => _caches;

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
        ArgumentNullException.ThrowIfNull(tenantInfo);
        ArgumentNullException.ThrowIfNull(tenantInfo.Id);

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
    public async Task<bool> RemoveAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);

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
    public async Task<TTenantInfo?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        var missedCaches = new List<IMultiTenantStoreCache<TTenantInfo>>();
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
        Func<TenantStoreLookupInfo<TTenantInfo>, Task<TTenantInfo?>>? lookupCompleted,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(identifier);

        var missedCaches = new List<IMultiTenantStoreCache<TTenantInfo>>();
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

    private static async Task<TTenantInfo?> CompleteCacheLookupAsync(IMultiTenantStoreCache<TTenantInfo> cache,
        string identifier, TTenantInfo? tenantInfo,
        Func<TenantStoreLookupInfo<TTenantInfo>, Task<TTenantInfo?>>? lookupCompleted)
    {
        if (lookupCompleted is null)
            return tenantInfo;

        return await lookupCompleted(new TenantStoreLookupInfo<TTenantInfo>
        {
            Cache = cache,
            Identifier = identifier,
            TenantInfo = tenantInfo
        }).ConfigureAwait(false);
    }

    private async Task<TTenantInfo?> CompleteStoreLookupAsync(string identifier, TTenantInfo? tenantInfo,
        Func<TenantStoreLookupInfo<TTenantInfo>, Task<TTenantInfo?>>? lookupCompleted)
    {
        if (lookupCompleted is null)
            return tenantInfo;

        return await lookupCompleted(new TenantStoreLookupInfo<TTenantInfo>
        {
            Store = _store,
            Identifier = identifier,
            TenantInfo = tenantInfo
        }).ConfigureAwait(false);
    }

    private async Task FillCachesAsync(IEnumerable<IMultiTenantStoreCache<TTenantInfo>> cachesToFill,
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

    private async Task InvalidateByIdAsync(string id, CancellationToken cancellationToken)
    {
        foreach (var cache in _caches)
            await RemoveFromCacheAsync(cache, id, cancellationToken).ConfigureAwait(false);
    }

    private async Task InvalidateByIdentifierAsync(string identifier, CancellationToken cancellationToken)
    {
        foreach (var cache in _caches)
            await RemoveByIdentifierFromCacheAsync(cache, identifier, cancellationToken).ConfigureAwait(false);
    }

    private async Task<TTenantInfo?> GetFromStoreAsync(string id, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(id);

        var logger = GetLogger(_store);
        TTenantInfo? result = default;

        try
        {
            result = await _store.GetAsync(id, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Exception in {nameof(IMultiTenantStore<TTenantInfo>.GetAsync)}");
        }

        if (logger.IsEnabled(LogLevel.Debug))
        {
            if (result is not null)
                logger.LogDebug($"{nameof(IMultiTenantStore<TTenantInfo>.GetAsync)}: Tenant Id \"{{TenantId}}\" found.", id);
            else
                logger.LogDebug($"{nameof(IMultiTenantStore<TTenantInfo>.GetAsync)}: Unable to find Tenant Id \"{{TenantId}}\".", id);
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
            logger.LogError(e, $"Exception in {nameof(IMultiTenantStore<TTenantInfo>.GetByIdentifierAsync)}");
        }

        if (logger.IsEnabled(LogLevel.Debug))
        {
            if (result is not null)
                logger.LogDebug(
                    $"{nameof(IMultiTenantStore<TTenantInfo>.GetByIdentifierAsync)}: Tenant found with identifier \"{{TenantIdentifier}}\"",
                    identifier);
            else
                logger.LogDebug(
                    $"{nameof(IMultiTenantStore<TTenantInfo>.GetByIdentifierAsync)}: Unable to find Tenant with identifier \"{{TenantIdentifier}}\"",
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
            logger.LogError(e, $"Exception in {nameof(IMultiTenantStore<TTenantInfo>.GetAllAsync)}");
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
            logger.LogError(e, $"Exception in {nameof(IMultiTenantStore<TTenantInfo>.GetAllAsync)}");
            return [];
        }
    }

    private async Task<bool> AddToStoreAsync(TTenantInfo tenantInfo, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tenantInfo);
        ArgumentNullException.ThrowIfNull(tenantInfo.Id);
        ArgumentNullException.ThrowIfNull(tenantInfo.Identifier);

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
                        $"{nameof(IMultiTenantStore<TTenantInfo>.AddAsync)}: Tenant already exists. Id: \"{{TenantId}}\", Identifier: \"{{TenantIdentifier}}\"",
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
                            $"{nameof(IMultiTenantStore<TTenantInfo>.AddAsync)}: Tenant already exists. Id: \"{{TenantId}}\", Identifier: \"{{TenantIdentifier}}\"",
                            tenantInfo.Id, tenantInfo.Identifier);
                    }
                }
                else
                    result = await _store.AddAsync(tenantInfo, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Exception in {nameof(IMultiTenantStore<TTenantInfo>.AddAsync)}");
        }

        if (logger.IsEnabled(LogLevel.Debug))
        {
            if (result)
                logger.LogDebug(
                    $"{nameof(IMultiTenantStore<TTenantInfo>.AddAsync)}: Tenant added. Id: \"{{TenantId}}\", Identifier: \"{{TenantIdentifier}}\"",
                    tenantInfo.Id, tenantInfo.Identifier);
            else
                logger.LogDebug(
                    $"{nameof(IMultiTenantStore<TTenantInfo>.AddAsync)}: Unable to add Tenant. Id: \"{{TenantId}}\", Identifier: \"{{TenantIdentifier}}\"",
                    tenantInfo.Id, tenantInfo.Identifier);
        }

        return result;
    }

    private async Task<bool> UpdateStoreAsync(TTenantInfo tenantInfo, TTenantInfo? existing,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tenantInfo);
        ArgumentNullException.ThrowIfNull(tenantInfo.Id);

        var logger = GetLogger(_store);
        var result = false;

        try
        {
            if (existing is null)
            {
                if (logger.IsEnabled(LogLevel.Debug))
                    logger.LogDebug($"{nameof(IMultiTenantStore<TTenantInfo>.UpdateAsync)}: Tenant Id: \"{{TenantId}}\" not found", tenantInfo.Id);
            }
            else
                result = await _store.UpdateAsync(tenantInfo, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Exception in {nameof(IMultiTenantStore<TTenantInfo>.UpdateAsync)}");
        }

        if (logger.IsEnabled(LogLevel.Debug))
        {
            if (result)
                logger.LogDebug($"{nameof(IMultiTenantStore<TTenantInfo>.UpdateAsync)}: Tenant Id: \"{{TenantId}}\" updated",
                    tenantInfo.Id);
            else
                logger.LogDebug(
                    $"{nameof(IMultiTenantStore<TTenantInfo>.UpdateAsync)}: Unable to update Tenant Id: \"{{TenantId}}\"",
                    tenantInfo.Id);
        }

        return result;
    }

    private async Task<bool> RemoveFromStoreAsync(string id, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(id);

        var logger = GetLogger(_store);
        var result = false;

        try
        {
            result = await _store.RemoveAsync(id, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Exception in {nameof(IMultiTenantStore<TTenantInfo>.RemoveAsync)}");
        }

        if (logger.IsEnabled(LogLevel.Debug))
        {
            if (result)
                logger.LogDebug(
                    $"{nameof(IMultiTenantStore<TTenantInfo>.RemoveAsync)}: Tenant Id: \"{{TenantId}}\" removed",
                    id);
            else
                logger.LogDebug(
                    $"{nameof(IMultiTenantStore<TTenantInfo>.RemoveAsync)}: Unable to remove Tenant Id: \"{{TenantId}}\"",
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
            logger.LogError(e, $"Exception in {nameof(IMultiTenantStore<TTenantInfo>.RemoveByIdentifierAsync)}");
        }

        if (logger.IsEnabled(LogLevel.Debug))
        {
            if (result)
                logger.LogDebug(
                    $"{nameof(IMultiTenantStore<TTenantInfo>.RemoveByIdentifierAsync)}: Tenant Identifier: \"{{TenantIdentifier}}\" removed",
                    identifier);
            else
                logger.LogDebug(
                    $"{nameof(IMultiTenantStore<TTenantInfo>.RemoveByIdentifierAsync)}: Unable to remove Tenant Identifier: \"{{TenantIdentifier}}\"",
                    identifier);
        }

        return result;
    }

    private async Task<TTenantInfo?> GetFromCacheAsync(IMultiTenantStoreCache<TTenantInfo> cache, string id,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(id);

        try
        {
            return await cache.GetAsync(id, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            GetLogger(cache).LogError(e, $"Exception in {nameof(IMultiTenantStoreCache<TTenantInfo>.GetAsync)}");
            return default;
        }
    }

    private async Task<TTenantInfo?> GetByIdentifierFromCacheAsync(IMultiTenantStoreCache<TTenantInfo> cache,
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
                $"Exception in {nameof(IMultiTenantStoreCache<TTenantInfo>.GetByIdentifierAsync)}");
            return default;
        }
    }

    private async Task SetCacheAsync(IMultiTenantStoreCache<TTenantInfo> cache, TTenantInfo tenantInfo,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tenantInfo);

        try
        {
            await cache.SetAsync(tenantInfo, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            GetLogger(cache).LogError(e, $"Exception in {nameof(IMultiTenantStoreCache<TTenantInfo>.SetAsync)}");
        }
    }

    private async Task RemoveFromCacheAsync(IMultiTenantStoreCache<TTenantInfo> cache, string id,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(id);

        try
        {
            await cache.RemoveAsync(id, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            GetLogger(cache).LogError(e, $"Exception in {nameof(IMultiTenantStoreCache<TTenantInfo>.RemoveAsync)}");
        }
    }

    private async Task RemoveByIdentifierFromCacheAsync(IMultiTenantStoreCache<TTenantInfo> cache, string identifier,
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
                $"Exception in {nameof(IMultiTenantStoreCache<TTenantInfo>.RemoveByIdentifierAsync)}");
        }
    }

    private ILogger GetLogger(object source)
    {
        return _loggerFactory?.CreateLogger(source.GetType()) ?? NullLogger.Instance;
    }
}
