// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Runtime.CompilerServices;
using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant.Stores;

/// <summary>
/// Basic store that simply returns a tenant based on the identifier without any additional settings.
/// Note that add, update, and remove functionality is not implemented.
/// </summary>
/// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
public class EchoStore<TTenantInfo> : IMultiTenantStore<TTenantInfo> where TTenantInfo : ITenantInfo
{
    /// <inheritdoc />
    public Task<TTenantInfo?> GetByIdentifierAsync(string identifier)
    {
        var tenantInfo = (TTenantInfo?)RuntimeHelpers.GetUninitializedObject(typeof(TTenantInfo));
    
        // use reflection since the interfaces only has getters for id and identifier (design choice)
        var idProperty = typeof(TTenantInfo).GetProperty("Id");
        idProperty?.SetValue(tenantInfo, identifier);
        var identifierProperty = typeof(TTenantInfo).GetProperty("Identifier");
        identifierProperty?.SetValue(tenantInfo, identifier);

        return Task.FromResult(tenantInfo);
    }

    /// <inheritdoc />
    public Task<TTenantInfo?> GetAsync(string id)
    {
        var tenantInfo = (TTenantInfo?)RuntimeHelpers.GetUninitializedObject(typeof(TTenantInfo));
        
        // use reflection since the interfaces only has getters for id and identifier (design choice)
        var idProperty = typeof(TTenantInfo).GetProperty("Id");
        idProperty?.SetValue(tenantInfo, id);
        var identifierProperty = typeof(TTenantInfo).GetProperty("Identifier");
        identifierProperty?.SetValue(tenantInfo, id);

        return Task.FromResult(tenantInfo);
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