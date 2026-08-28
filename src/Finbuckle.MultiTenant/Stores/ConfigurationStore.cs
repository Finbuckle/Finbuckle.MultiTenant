// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Collections.Concurrent;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Finbuckle.MultiTenant.Stores;

/// <summary>
/// Basic store that uses .NET configuration to define tenants. Note that add, update, and remove functionality is not
/// implemented. If the underlying configuration supports reload-on-change, then this store will reflect such changes.
/// </summary>
/// <typeparam name="TTenantInfo">The <see cref="ITenantInfo{TId}"/> derived type.</typeparam>
/// <typeparam name="TId">The ID implementation type.</typeparam>
/// <remarks>
/// Each tenant is created by binding the merged <c>Defaults</c> and tenant sections with the standard configuration
/// binder, so <typeparamref name="TTenantInfo"/> may have settable properties (<c>set</c> or <c>init</c>) or a single
/// public constructor whose parameter names match the configuration keys. For types the binder cannot construct,
/// pass a <c>tenantInfoFactory</c> to the constructor (or to the <c>WithConfigurationStore</c> overload that accepts
/// one).
/// </remarks>
public class ConfigurationStore<TTenantInfo, TId> : IMultiTenantStore<TTenantInfo, TId> where TTenantInfo : ITenantInfo<TId> where TId : IEquatable<TId>
{
    private const string DefaultSectionName = "Finbuckle:MultiTenant:Stores:ConfigurationStore";
    private readonly IConfigurationSection section;
    private readonly Func<IConfiguration, TTenantInfo> tenantInfoFactory;
    private ConcurrentDictionary<string, TTenantInfo> tenantMap = new();

    // ReSharper disable once IntroduceOptionalParameters.Global
    /// <summary>
    /// Constructor for ConfigurationStore. Uses a section name of "Finbuckle:MultiTenant:Stores:ConfigurationStore".
    /// </summary>
    /// <param name="configuration"><see cref="IConfiguration"/> instance containing tenant information.</param>
    public ConfigurationStore(IConfiguration configuration) : this(configuration, DefaultSectionName)
    {
    }

    // ReSharper disable once IntroduceOptionalParameters.Global
    /// <summary>
    /// Constructor for ConfigurationStore.
    /// </summary>
    /// <param name="configuration"><see cref="IConfiguration"/> instance containing tenant information.</param>
    /// <param name="sectionName">Name of the section within the configuration containing tenant information.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="sectionName"/> is null or empty.</exception>
    /// <exception cref="MultiTenantException">Thrown when the section name is invalid or a tenant cannot be bound.</exception>
    public ConfigurationStore(IConfiguration configuration, string sectionName)
        : this(configuration, sectionName, BindTenantInfo, validateFactory: false)
    {
    }

    /// <summary>
    /// Constructor for ConfigurationStore with a custom tenant info factory.
    /// </summary>
    /// <param name="configuration"><see cref="IConfiguration"/> instance containing tenant information.</param>
    /// <param name="sectionName">Name of the section within the configuration containing tenant information.</param>
    /// <param name="tenantInfoFactory">
    /// Creates a <typeparamref name="TTenantInfo"/> from the configuration of a single tenant. The configuration
    /// passed in contains the tenant's own keys overlaid on the <c>Defaults</c> section. Use this when the standard
    /// configuration binder cannot construct <typeparamref name="TTenantInfo"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> or <paramref name="tenantInfoFactory"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="sectionName"/> is null or empty.</exception>
    /// <exception cref="MultiTenantException">Thrown when the section name is invalid or the factory returns null.</exception>
    public ConfigurationStore(IConfiguration configuration, string sectionName,
        Func<IConfiguration, TTenantInfo> tenantInfoFactory)
        : this(configuration, sectionName, tenantInfoFactory, validateFactory: true)
    {
    }

    private ConfigurationStore(IConfiguration configuration, string sectionName,
        Func<IConfiguration, TTenantInfo> tenantInfoFactory, bool validateFactory)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        if (validateFactory)
            ArgumentNullException.ThrowIfNull(tenantInfoFactory);

        if (string.IsNullOrEmpty(sectionName))
        {
            throw new ArgumentException("Section name provided to the Configuration Store is null or empty.",
                nameof(sectionName));
        }

        section = configuration.GetSection(sectionName);
        if (!section.Exists())
        {
            throw new MultiTenantException("Section name provided to the Configuration Store is invalid.");
        }

        this.tenantInfoFactory = tenantInfoFactory;

        UpdateTenantMap();
        ChangeToken.OnChange(() => section.GetReloadToken(), UpdateTenantMap);
    }

    private void UpdateTenantMap()
    {
        var newMap = new ConcurrentDictionary<string, TTenantInfo>(StringComparer.OrdinalIgnoreCase);
        var tenants = section.GetSection("Tenants").GetChildren();
        var defaults = section.GetSection("Defaults");

        foreach (var tenantSection in tenants)
        {
            // The tenant's own keys take precedence over the Defaults section.
            var merged = new ConfigurationBuilder()
                .AddConfiguration(defaults)
                .AddConfiguration(tenantSection)
                .Build();

            var newTenant = tenantInfoFactory(merged) ??
                            throw new MultiTenantException(
                                $"The tenant info factory returned null for configuration section '{tenantSection.Path}'.");

            newTenant.EnsureValid();
            newMap.TryAdd(newTenant.Identifier, newTenant);
        }

        tenantMap = newMap;
    }

    private static TTenantInfo BindTenantInfo(IConfiguration tenantConfiguration)
    {
        try
        {
            return tenantConfiguration.Get<TTenantInfo>(options => options.BindNonPublicProperties = true) ??
                   throw new MultiTenantException(
                       $"Unable to bind a {typeof(TTenantInfo).Name} from configuration: the tenant section is empty.");
        }
        catch (InvalidOperationException e)
        {
            throw new MultiTenantException(
                $"Unable to bind a {typeof(TTenantInfo).Name} from configuration. The type must have settable Id " +
                "and Identifier properties or a single public constructor whose parameter names match the " +
                "configuration keys; otherwise provide a tenantInfoFactory to the ConfigurationStore constructor or " +
                $"the WithConfigurationStore overload. Binder error: {e.Message}", e);
        }
    }

    /// <summary>
    /// Not implemented in this implementation.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public Task<bool> AddAsync(TTenantInfo tenantInfo, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<TTenantInfo?> GetAsync(TId id, CancellationToken cancellationToken = default)
    {
        if (EqualityComparer<TId>.Default.Equals(id, default!))
            throw new ArgumentException("Tenant id cannot be the default value.", nameof(id));
        return Task.FromResult(tenantMap.Values.SingleOrDefault(v => v.Id.Equals(id)));
    }

    /// <inheritdoc />
    public Task<IEnumerable<TTenantInfo>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(tenantMap.Values.ToList().AsEnumerable());
    }

    /// <inheritdoc />
    public Task<IEnumerable<TTenantInfo>> GetAllAsync(int take, int skip, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(tenantMap.Values.Skip(skip).Take(take).ToList().AsEnumerable());
    }

    /// <inheritdoc />
    public Task<TTenantInfo?> GetByIdentifierAsync(string identifier, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(identifier);
        return Task.FromResult(tenantMap.GetValueOrDefault(identifier));
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
    public Task<bool> UpdateAsync(TTenantInfo tenantInfo, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
