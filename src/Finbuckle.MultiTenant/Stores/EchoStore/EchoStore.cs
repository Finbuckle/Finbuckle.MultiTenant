// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Runtime.CompilerServices;
using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant.Stores.EchoStore;

/// <summary>
/// Basic store that simply returns a tenant based on the identifier without any additional settings.
/// Note that add, update, and remove functionality is not implemented.
/// If underlying configuration supports reload-on-change then this store will reflect such changes.
/// </summary>
/// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
public class EchoStore<TTenantInfo> : IMultiTenantStore<TTenantInfo> where TTenantInfo : TenantInfo
{
    /// <inheritdoc />
    public async Task<TTenantInfo?> GetByIdentifierAsync(string identifier)
    {
        var tenantInfo = (TTenantInfo)RuntimeHelpers.GetUninitializedObject(typeof(TTenantInfo));
        tenantInfo = tenantInfo with { Id = identifier, Identifier = identifier };
        
        // Ensure TTenantInfo has a parameterless constructor.
        return await Task.FromResult(tenantInfo).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<TTenantInfo?> GetAsync(string id)
    {
        var tenantInfo = (TTenantInfo)RuntimeHelpers.GetUninitializedObject(typeof(TTenantInfo));
        tenantInfo = tenantInfo with { Id = id, Identifier = id };
        return await Task.FromResult(tenantInfo).ConfigureAwait(false);
    }
    
    /// <summary>
    /// Not implemented in this implementation.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public Task<bool> AddAsync(TTenantInfo tenantInfo)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Not implemented in this implementation.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public Task<bool> UpdateAsync(TTenantInfo tenantInfo)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Not implemented in this implementation.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public Task<bool> RemoveAsync(string identifier)
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

    /// <summary>
    /// Not implemented in this implementation.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public Task<IEnumerable<TTenantInfo>> GetAllAsync(int take, int skip)
    {
        throw new NotImplementedException();
    }
}