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
/// <remarks>
/// The store has to construct <typeparamref name="TTenantInfo"/> instances itself. By default it does so by
/// reflection, which requires <c>Id</c> and <c>Identifier</c> to have a setter (<c>set</c> or <c>init</c>, public or
/// not). For constructor-only implementations pass a <c>tenantInfoFactory</c> to the constructor (or to the
/// <c>WithEchoStore</c> overload that accepts one).
/// </remarks>
public class EchoStore<TTenantInfo, TId> : IMultiTenantStore<TTenantInfo, TId> where TTenantInfo : ITenantInfo<TId> where TId : IEquatable<TId>
{
    private static readonly Lazy<Func<TId, string, TTenantInfo>> ReflectionFactory = new(CreateReflectionFactory);

    private readonly Func<string, TId> idFromIdentifier;
    private readonly Func<TId, string, TTenantInfo>? tenantInfoFactory;

    /// <summary>
    /// Constructor for EchoStore.
    /// </summary>
    /// <param name="idFromIdentifier">
    /// Converts a tenant identifier to a tenant id. Required because there is no universal way to convert a string
    /// identifier to an arbitrary <typeparamref name="TId"/>. For a string id this is simply <c>identifier => identifier</c>.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="idFromIdentifier"/> is null.</exception>
    public EchoStore(Func<string, TId> idFromIdentifier)
    {
        this.idFromIdentifier = idFromIdentifier ?? throw new ArgumentNullException(nameof(idFromIdentifier));
    }

    /// <summary>
    /// Constructor for EchoStore with a custom tenant info factory.
    /// </summary>
    /// <param name="idFromIdentifier">
    /// Converts a tenant identifier to a tenant id. For a string id this is simply <c>identifier => identifier</c>.
    /// </param>
    /// <param name="tenantInfoFactory">
    /// Creates the <typeparamref name="TTenantInfo"/> instance from a tenant id and identifier. Use this when
    /// <typeparamref name="TTenantInfo"/> has no settable <c>Id</c>/<c>Identifier</c> properties, e.g. a
    /// constructor-only immutable implementation.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when either argument is null.</exception>
    public EchoStore(Func<string, TId> idFromIdentifier, Func<TId, string, TTenantInfo> tenantInfoFactory)
        : this(idFromIdentifier)
    {
        this.tenantInfoFactory = tenantInfoFactory ?? throw new ArgumentNullException(nameof(tenantInfoFactory));
    }

    /// <inheritdoc />
    public Task<TTenantInfo?> GetByIdentifierAsync(string identifier, CancellationToken cancellationToken = default)
    {
        var tenantInfo = Create(idFromIdentifier(identifier), identifier);
        tenantInfo.EnsureValid();
        return Task.FromResult<TTenantInfo?>(tenantInfo);
    }

    /// <inheritdoc />
    public Task<TTenantInfo?> GetAsync(TId id, CancellationToken cancellationToken = default)
    {
        if (EqualityComparer<TId>.Default.Equals(id, default!))
            throw new ArgumentException("Tenant id cannot be the default value.", nameof(id));

        // the identifier is just the string form of the id; ToString is the natural inverse of idFromIdentifier
        var tenantInfo = Create(id, id.ToString() ?? string.Empty);
        tenantInfo.EnsureValid();
        return Task.FromResult<TTenantInfo?>(tenantInfo);
    }

    private TTenantInfo Create(TId id, string identifier) =>
        (tenantInfoFactory ?? ReflectionFactory.Value)(id, identifier);

    private static Func<TId, string, TTenantInfo> CreateReflectionFactory()
    {
        var type = typeof(TTenantInfo);
        var idProperty = type.GetProperty(nameof(ITenantInfo<TId>.Id));
        var identifierProperty = type.GetProperty(nameof(ITenantInfo<TId>.Identifier));

        if (type.IsAbstract || idProperty?.SetMethod is null || identifierProperty?.SetMethod is null)
        {
            throw new MultiTenantException(
                $"EchoStore cannot create instances of {type.FullName}: the type must be concrete with settable Id " +
                "and Identifier properties (set or init). For constructor-only implementations provide a " +
                "tenantInfoFactory to the EchoStore constructor or the WithEchoStore overload.");
        }

        return (id, identifier) =>
        {
            var tenantInfo = (TTenantInfo)RuntimeHelpers.GetUninitializedObject(type);
            idProperty.SetValue(tenantInfo, id);
            identifierProperty.SetValue(tenantInfo, identifier);
            return tenantInfo;
        };
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
