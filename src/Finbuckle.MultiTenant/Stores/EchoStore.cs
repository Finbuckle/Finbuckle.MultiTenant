// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Runtime.CompilerServices;
using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant.Stores;

/// <summary>
/// Basic store that simply returns a tenant based on the identifier without any additional settings.
/// Note that add, update, and remove functionality is not implemented.
/// </summary>
/// <typeparam name="TTenantInfo">The <see cref="ITenantInfo{TId}"/> implementation type.</typeparam>
/// <typeparam name="TId">The ID implementation type.</typeparam>
public class EchoStore<TTenantInfo, TId> : IMultiTenantStore<TTenantInfo, TId> where TTenantInfo : ITenantInfo<TId> where TId : IEquatable<TId>
{
    private readonly Func<string, TId> idFromIdentifier;
    private readonly Func<TId, string> identifierFromId;

    /// <summary>
    /// Constructor for EchoStore.
    /// </summary>
    /// <param name="idFromIdentifier">Converts a tenant identifier to a tenant id.</param>
    /// <param name="identifierFromId">Converts a tenant id to a tenant identifier.</param>
    /// <exception cref="ArgumentNullException">Thrown when either conversion delegate is null.</exception>
    public EchoStore(Func<string, TId> idFromIdentifier, Func<TId, string> identifierFromId)
    {
        this.idFromIdentifier = idFromIdentifier ?? throw new ArgumentNullException(nameof(idFromIdentifier));
        this.identifierFromId = identifierFromId ?? throw new ArgumentNullException(nameof(identifierFromId));
    }

    /// <inheritdoc />
    public Task<TTenantInfo?> GetByIdentifierAsync(string identifier, CancellationToken cancellationToken = default)
    {
        var tenantInfo = (TTenantInfo?)RuntimeHelpers.GetUninitializedObject(typeof(TTenantInfo));

        // use reflection since the interfaces only has getters for id and identifier (design choice)
        var idProperty = typeof(TTenantInfo).GetProperty("Id");
        idProperty?.SetValue(tenantInfo, idFromIdentifier(identifier));
        var identifierProperty = typeof(TTenantInfo).GetProperty("Identifier");
        identifierProperty?.SetValue(tenantInfo, identifier);

        return Task.FromResult(tenantInfo);
    }

    /// <inheritdoc />
    public Task<TTenantInfo?> GetAsync(TId id, CancellationToken cancellationToken = default)
    {
        var tenantInfo = (TTenantInfo?)RuntimeHelpers.GetUninitializedObject(typeof(TTenantInfo));

        // use reflection since the interfaces only has getters for id and identifier (design choice)
        var idProperty = typeof(TTenantInfo).GetProperty("Id");
        idProperty?.SetValue(tenantInfo, id);
        var identifierProperty = typeof(TTenantInfo).GetProperty("Identifier");
        identifierProperty?.SetValue(tenantInfo, identifierFromId(id));

        return Task.FromResult(tenantInfo);
    }

    /// <summary>
    /// Not implemented in this implementation.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public Task<bool> AddAsync(TTenantInfo tenantInfo, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Not implemented in this implementation.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public Task<bool> UpdateAsync(TTenantInfo tenantInfo, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Not implemented in this implementation.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public Task<bool> RemoveAsync(TId id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Not implemented in this implementation.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public Task<bool> RemoveByIdentifierAsync(string identifier, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Not implemented in this implementation.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public Task<IEnumerable<TTenantInfo>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Not implemented in this implementation.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public Task<IEnumerable<TTenantInfo>> GetAllAsync(int take, int skip, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
