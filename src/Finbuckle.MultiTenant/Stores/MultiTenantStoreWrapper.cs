// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Finbuckle.MultiTenant.Stores;

/// <summary>
/// Multitenant store decorator that handles exception handling and logging.
/// </summary>
public class MultiTenantStoreWrapper<TTenantInfo> : IMultiTenantStore<TTenantInfo>
    where TTenantInfo : class, ITenantInfo, new()
{
    // ReSharper disable once MemberCanBePrivate.Global
    /// <summary>
    /// The internal IMultiTenantStore instance.
    /// </summary>
    public IMultiTenantStore<TTenantInfo> Store { get; }

    private readonly ILogger _logger;

    /// <summary>
    /// Constructor for MultiTenantStoreWrapper
    /// </summary>
    /// <param name="store">IMultiTenantStore instance to wrap.</param>
    /// <param name="logger">Logger instance.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public MultiTenantStoreWrapper(IMultiTenantStore<TTenantInfo> store, ILogger logger)
    {
        Store = store ?? throw new ArgumentNullException(nameof(store));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<TTenantInfo?> TryGetAsync(string id)
    {
        if (id == null)
        {
            throw new ArgumentNullException(nameof(id));
        }

        TTenantInfo? result = null;

        try
        {
            result = await Store.TryGetAsync(id);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception in TryGetAsync");
        }

        if (result != null)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("TryGetAsync: Tenant Id \"{TenantId}\" found.", id);
            }
        }
        else
        {
            _logger.LogDebug("TryGetAsync: Unable to find Tenant Id \"{TenantId}\".", id);
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TTenantInfo>> GetAllAsync()
    {
        IEnumerable<TTenantInfo> result = new List<TTenantInfo>();

        try
        {
            result = await Store.GetAllAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception in GetAllAsync");
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<TTenantInfo?> TryGetByIdentifierAsync(string identifier)
    {
        if (identifier == null)
        {
            throw new ArgumentNullException(nameof(identifier));
        }

        TTenantInfo? result = null;

        try
        {
            result = await Store.TryGetByIdentifierAsync(identifier);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception in TryGetByIdentifierAsync");
        }

        if (result != null)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("TryGetByIdentifierAsync: Tenant found with identifier \"{TenantIdentifier}\"",
                    identifier);
            }
        }
        else
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(
                    "TryGetByIdentifierAsync: Unable to find Tenant with identifier \"{TenantIdentifier}\"",
                    identifier);
            }
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<bool> TryAddAsync(TTenantInfo tenantInfo)
    {
        if (tenantInfo == null)
        {
            throw new ArgumentNullException(nameof(tenantInfo));
        }

        if (tenantInfo.Id == null)
        {
            throw new ArgumentNullException(nameof(tenantInfo.Id));
        }

        if (tenantInfo.Identifier == null)
        {
            throw new ArgumentNullException(nameof(tenantInfo.Identifier));
        }

        var result = false;

        try
        {
            var existing = await TryGetAsync(tenantInfo.Id);
            if (existing != null)
            {
                _logger.LogDebug(
                    "TryAddAsync: Tenant already exists. Id: \"{TenantId}\", Identifier: \"{TenantIdentifier}\"",
                    tenantInfo.Id, tenantInfo.Identifier);
            }
            else
            {
                existing = await TryGetByIdentifierAsync(tenantInfo.Identifier);
                if (existing != null)
                {
                    _logger.LogDebug(
                        "TryAddAsync: Tenant already exists. Id: \"{TenantId}\", Identifier: \"{TenantIdentifier}\"",
                        tenantInfo.Id, tenantInfo.Identifier);
                }
                else
                    result = await Store.TryAddAsync(tenantInfo);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception in TryAddAsync");
        }

        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (result)
        {
            _logger.LogDebug("TryAddAsync: Tenant added. Id: \"{TenantId}\", Identifier: \"{TenantIdentifier}\"",
                tenantInfo.Id, tenantInfo.Identifier);
        }
        else
        {
            _logger.LogDebug(
                "TryAddAsync: Unable to add Tenant. Id: \"{TenantId}\", Identifier: \"{TenantIdentifier}\"",
                tenantInfo.Id, tenantInfo.Identifier);
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<bool> TryRemoveAsync(string identifier)
    {
        if (identifier == null)
        {
            throw new ArgumentNullException(nameof(identifier));
        }

        var result = false;

        try
        {
            result = await Store.TryRemoveAsync(identifier);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception in TryRemoveAsync");
        }

        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (result)
        {
            _logger.LogDebug("TryRemoveAsync: Tenant Identifier: \"{TenantIdentifier}\" removed", identifier);
        }
        else
        {
            _logger.LogDebug("TryRemoveAsync: Unable to remove Tenant Identifier: \"{TenantIdentifier}\"", identifier);
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<bool> TryUpdateAsync(TTenantInfo tenantInfo)
    {
        if (tenantInfo == null)
        {
            throw new ArgumentNullException(nameof(tenantInfo));
        }

        if (tenantInfo.Id == null)
        {
            throw new ArgumentNullException(nameof(tenantInfo.Id));
        }

        var result = false;

        try
        {
            var existing = await TryGetAsync(tenantInfo.Id);
            if (existing == null)
            {
                _logger.LogDebug("TryUpdateAsync: Tenant Id: \"{TenantId}\" not found", tenantInfo.Id);
            }
            else
                result = await Store.TryUpdateAsync(tenantInfo);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception in TryUpdateAsync");
        }

        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (result)
        {
            _logger.LogDebug("TryUpdateAsync: Tenant Id: \"{TenantId}\" updated", tenantInfo.Id);
        }
        else
        {
            _logger.LogDebug("TryUpdateAsync: Unable to update Tenant Id: \"{TenantId}\"", tenantInfo.Id);
        }

        return result;
    }
}