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
/// <remarks>
/// The store has to construct <typeparamref name="TTenantInfo"/> instances itself. By default it does so by
/// reflection, which requires <c>Id</c> and <c>Identifier</c> to have a setter (<c>set</c> or <c>init</c>, public or
/// not). For constructor-only implementations pass a <c>tenantInfoFactory</c> to the constructor (or to the
/// <c>WithEchoStore</c> overload that accepts one).
/// </remarks>
public class EchoStore<TTenantInfo> : IMultiTenantStore<TTenantInfo> where TTenantInfo : ITenantInfo
{
    private static readonly Lazy<Func<string, TTenantInfo>> ReflectionFactory = new(CreateReflectionFactory);

    private readonly Func<string, TTenantInfo>? tenantInfoFactory;

    /// <summary>
    /// Constructor for EchoStore.
    /// </summary>
    public EchoStore()
    {
    }

    /// <summary>
    /// Constructor for EchoStore with a custom tenant info factory.
    /// </summary>
    /// <param name="tenantInfoFactory">
    /// Creates the <typeparamref name="TTenantInfo"/> instance from the identifier, which is used as both the tenant
    /// id and identifier. Use this when <typeparamref name="TTenantInfo"/> has no settable <c>Id</c>/<c>Identifier</c>
    /// properties, e.g. a constructor-only immutable implementation.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tenantInfoFactory"/> is null.</exception>
    public EchoStore(Func<string, TTenantInfo> tenantInfoFactory)
    {
        this.tenantInfoFactory = tenantInfoFactory ?? throw new ArgumentNullException(nameof(tenantInfoFactory));
    }

    /// <inheritdoc />
    public Task<TTenantInfo?> GetByIdentifierAsync(string identifier)
    {
        return Task.FromResult<TTenantInfo?>(Create(identifier));
    }

    /// <inheritdoc />
    public Task<TTenantInfo?> GetAsync(string id)
    {
        return Task.FromResult<TTenantInfo?>(Create(id));
    }

    private TTenantInfo Create(string identifier) => (tenantInfoFactory ?? ReflectionFactory.Value)(identifier);

    private static Func<string, TTenantInfo> CreateReflectionFactory()
    {
        var type = typeof(TTenantInfo);
        var idProperty = type.GetProperty(nameof(ITenantInfo.Id));
        var identifierProperty = type.GetProperty(nameof(ITenantInfo.Identifier));

        if (type.IsAbstract || idProperty?.SetMethod is null || identifierProperty?.SetMethod is null)
        {
            throw new MultiTenantException(
                $"EchoStore cannot create instances of {type.FullName}: the type must be concrete with settable Id " +
                "and Identifier properties (set or init). For constructor-only implementations provide a " +
                "tenantInfoFactory to the EchoStore constructor or the WithEchoStore overload.");
        }

        return identifier =>
        {
            var tenantInfo = (TTenantInfo)RuntimeHelpers.GetUninitializedObject(type);
            idProperty.SetValue(tenantInfo, identifier);
            identifierProperty.SetValue(tenantInfo, identifier);
            return tenantInfo;
        };
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
