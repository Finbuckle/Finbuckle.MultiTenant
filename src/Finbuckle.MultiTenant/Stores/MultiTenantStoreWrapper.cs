// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.Logging;

namespace Finbuckle.MultiTenant.Stores;

/// <summary>
/// Multi-tenant store decorator that handles exception handling and logging.
/// </summary>
/// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
public class MultiTenantStoreWrapper<TTenantInfo> : IMultiTenantStore<TTenantInfo>
    where TTenantInfo : ITenantInfo
{
    // ReSharper disable once MemberCanBePrivate.Global
    /// <summary>
    /// The internal <see cref="IMultiTenantStore{TTenantInfo}"/> instance.
    /// </summary>
    public IMultiTenantStore<TTenantInfo> Store { get; }

    private readonly ILogger _logger;

    /// <summary>
    /// Constructor for MultiTenantStoreWrapper.
    /// </summary>
    /// <param name="store"><see cref="IMultiTenantStore{TTenantInfo}"/> instance to wrap.</param>
    /// <param name="logger">Logger instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="store"/> or <paramref name="logger"/> is null.</exception>
    public MultiTenantStoreWrapper(IMultiTenantStore<TTenantInfo> store, ILogger logger)
    {
        Store = store ?? throw new ArgumentNullException(nameof(store));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<TTenantInfo?> GetAsync(string id)
    {
        ArgumentNullException.ThrowIfNull(id);

        TTenantInfo? result = default;

        try
        {
            result = await Store.GetAsync(id).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Exception in {nameof(GetAsync)}");
        }

        if (result != null)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug($"{nameof(GetAsync)}: Tenant Id \"{{TenantId}}\" found.", id);
            }
        }
        else
        {
            _logger.LogDebug($"{nameof(GetAsync)}: Unable to find Tenant Id \"{{TenantId}}\".", id);
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TTenantInfo>> GetAllAsync()
    {
        IEnumerable<TTenantInfo> result = new List<TTenantInfo>();

        try
        {
            result = await Store.GetAllAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Exception in {nameof(GetAllAsync)}");
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TTenantInfo>> GetAllAsync(int take, int skip)
    {
        IEnumerable<TTenantInfo> result = new List<TTenantInfo>();

        try
        {
            result = await Store.GetAllAsync(take, skip).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Exception in {nameof(GetAllAsync)}");
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<TTenantInfo?> GetByIdentifierAsync(string identifier)
    {
        ArgumentNullException.ThrowIfNull(identifier);

        TTenantInfo? result = default;

        try
        {
            result = await Store.GetByIdentifierAsync(identifier).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Exception in {nameof(GetByIdentifierAsync)}");
        }

        if (result != null)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(
                    $"{nameof(GetByIdentifierAsync)}: Tenant found with identifier \"{{TenantIdentifier}}\"",
                    identifier);
            }
        }
        else
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(
                    $"{nameof(GetByIdentifierAsync)}: Unable to find Tenant with identifier \"{{TenantIdentifier}}\"",
                    identifier);
            }
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<bool> AddAsync(TTenantInfo tenantInfo)
    {
        ArgumentNullException.ThrowIfNull(tenantInfo);
        ArgumentNullException.ThrowIfNull(tenantInfo.Id);
        ArgumentNullException.ThrowIfNull(tenantInfo.Identifier);

        var result = false;

        try
        {
            var existing = await GetAsync(tenantInfo.Id).ConfigureAwait(false);
            if (existing != null)
            {
                _logger.LogDebug(
                    $"{nameof(AddAsync)}: Tenant already exists. Id: \"{{TenantId}}\", Identifier: \"{{TenantIdentifier}}\"",
                    tenantInfo.Id, tenantInfo.Identifier);
            }
            else
            {
                existing = await GetByIdentifierAsync(tenantInfo.Identifier).ConfigureAwait(false);
                if (existing != null)
                {
                    _logger.LogDebug(
                        $"{nameof(AddAsync)}: Tenant already exists. Id: \"{{TenantId}}\", Identifier: \"{{TenantIdentifier}}\"",
                        tenantInfo.Id, tenantInfo.Identifier);
                }
                else
                    result = await Store.AddAsync(tenantInfo).ConfigureAwait(false);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Exception in {nameof(AddAsync)}");
        }

        if (result)
        {
            _logger.LogDebug(
                $"{nameof(AddAsync)}: Tenant added. Id: \"{{TenantId}}\", Identifier: \"{{TenantIdentifier}}\"",
                tenantInfo.Id, tenantInfo.Identifier);
        }
        else
        {
            _logger.LogDebug(
                $"{nameof(AddAsync)}: Unable to add Tenant. Id: \"{{TenantId}}\", Identifier: \"{{TenantIdentifier}}\"",
                tenantInfo.Id, tenantInfo.Identifier);
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<bool> RemoveAsync(string identifier)
    {
        ArgumentNullException.ThrowIfNull(identifier);

        var result = false;

        try
        {
            result = await Store.RemoveAsync(identifier).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Exception in {nameof(RemoveAsync)}");
        }

        if (result)
        {
            _logger.LogDebug($"{nameof(RemoveAsync)}: Tenant Identifier: \"{{TenantIdentifier}}\" removed", identifier);
        }
        else
        {
            _logger.LogDebug($"{nameof(RemoveAsync)}: Unable to remove Tenant Identifier: \"{{TenantIdentifier}}\"",
                identifier);
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateAsync(TTenantInfo tenantInfo)
    {
        ArgumentNullException.ThrowIfNull(tenantInfo);
        ArgumentNullException.ThrowIfNull(tenantInfo.Id);

        var result = false;

        try
        {
            var existing = await GetAsync(tenantInfo.Id).ConfigureAwait(false);
            if (existing == null)
            {
                _logger.LogDebug($"{nameof(UpdateAsync)}: Tenant Id: \"{{TenantId}}\" not found", tenantInfo.Id);
            }
            else
                result = await Store.UpdateAsync(tenantInfo).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Exception in {nameof(UpdateAsync)}");
        }

        if (result)
        {
            _logger.LogDebug($"{nameof(UpdateAsync)}: Tenant Id: \"{{TenantId}}\" updated", tenantInfo.Id);
        }
        else
        {
            _logger.LogDebug($"{nameof(UpdateAsync)}: Unable to update Tenant Id: \"{{TenantId}}\"", tenantInfo.Id);
        }

        return result;
    }
}