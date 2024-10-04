// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

// ReSharper disable once CheckNamespace

using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant.Stores.EchoStore;

/// <summary>
/// Basic store that simply returns a tenant based on the identifier without any additional settings.
/// Note that add, update, and remove functionality is not implemented.
/// If underlying configuration supports reload-on-change then this store will reflect such changes.
/// </summary>
/// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
public class EchoStore<TTenantInfo> : IMultiTenantStore<TTenantInfo> where TTenantInfo : class, ITenantInfo, new()
{
    /// <inheritdoc />
    public async Task<TTenantInfo?> TryGetByIdentifierAsync(string identifier)
    {
        return await Task.FromResult(new TTenantInfo { Id = identifier, Identifier = identifier });
    }

    /// <inheritdoc />
    public async Task<TTenantInfo?> TryGetAsync(string id)
    {
        return await Task.FromResult(new TTenantInfo { Id = id, Identifier = id });
    }
    
    /// <summary>
    /// Not implemented in this implementation.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public Task<bool> TryAddAsync(TTenantInfo tenantInfo)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Not implemented in this implementation.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public Task<bool> TryUpdateAsync(TTenantInfo tenantInfo)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Not implemented in this implementation.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public Task<bool> TryRemoveAsync(string identifier)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Not implemented in this implementation.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public Task<IEnumerable<TTenantInfo>> GetAllAsync()
    {
        throw new NotImplementedException();
    }
}