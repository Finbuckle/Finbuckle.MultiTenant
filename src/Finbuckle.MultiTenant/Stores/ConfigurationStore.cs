// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Finbuckle.MultiTenant.Stores;

/// <summary>
/// Basic store that uses .NET configuration to define tenants. Note that add, update, and remove functionality is not
/// implemented. If the underlying configuration supports reload-on-change, then this store will reflect such changes.
/// </summary>
/// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> derived type.</typeparam>
/// <remarks>
/// <para>
/// By default each tenant is created by binding the <c>Defaults</c> section and then the tenant section onto a new
/// instance, which requires settable properties (<c>set</c> or <c>init</c>, public or non-public). Because the two
/// sections are bound one after the other, collection properties present in both are appended: the tenant's entries
/// follow the defaults' entries. Scalar values and dictionary keys in the tenant section replace those from
/// <c>Defaults</c>.
/// </para>
/// <para>
/// For constructor-only implementations pass a <c>tenantInfoFactory</c> to the constructor (or to the
/// <c>WithConfigurationStore</c> overload that accepts one). The factory receives a single configuration with the
/// tenant section layered over <c>Defaults</c>, so collection entries present in both are overlaid by index rather
/// than appended.
/// </para>
/// </remarks>
public class ConfigurationStore<TTenantInfo> : IMultiTenantStore<TTenantInfo> where TTenantInfo : ITenantInfo
{
    private const string DefaultSectionName = "Finbuckle:MultiTenant:Stores:ConfigurationStore";
    private readonly IConfigurationSection section;
    private readonly Func<IConfiguration, TTenantInfo>? tenantInfoFactory;
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
        : this(configuration, sectionName, tenantInfoFactory: null, validateFactory: false)
    {
    }

    /// <summary>
    /// Constructor for ConfigurationStore with a custom tenant info factory.
    /// </summary>
    /// <param name="configuration"><see cref="IConfiguration"/> instance containing tenant information.</param>
    /// <param name="sectionName">Name of the section within the configuration containing tenant information.</param>
    /// <param name="tenantInfoFactory">
    /// Creates a <typeparamref name="TTenantInfo"/> from the configuration of a single tenant. The configuration
    /// passed in contains the tenant's own keys layered over the <c>Defaults</c> section, so collection entries present
    /// in both are overlaid by index rather than appended (unlike default binding). Use this when
    /// <typeparamref name="TTenantInfo"/> has no settable properties, e.g. a constructor-only immutable implementation.
    /// Read the configuration during this call and return a self-contained tenant. The supplied configuration is
    /// disposed after the factory returns and must not be retained for ongoing use.
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
        Func<IConfiguration, TTenantInfo>? tenantInfoFactory, bool validateFactory)
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
            var newTenant = tenantInfoFactory is null
                ? BindTenantInfo(defaults, tenantSection)
                : CreateTenantInfo(tenantInfoFactory, defaults, tenantSection);

            if (string.IsNullOrWhiteSpace(newTenant.Id) || string.IsNullOrWhiteSpace(newTenant.Identifier))
            {
                throw new MultiTenantException(
                    $"The tenant in configuration section '{tenantSection.Path}' has no Id or Identifier after " +
                    $"binding to {typeof(TTenantInfo).Name}. Check the configuration keys; if the type has no settable " +
                    "Id and Identifier properties provide a tenantInfoFactory to the ConfigurationStore constructor " +
                    "or the WithConfigurationStore overload.");
            }

            newMap.TryAdd(newTenant.Identifier, newTenant);
        }

        tenantMap = newMap;
    }

    // Binds Defaults and then the tenant section onto the same instance. Binding twice (rather than binding a merged
    // configuration) preserves the established behavior that collection entries from both sections are appended.
    private static TTenantInfo BindTenantInfo(IConfigurationSection defaults, IConfigurationSection tenantSection)
    {
        var newTenant = (TTenantInfo)RuntimeHelpers.GetUninitializedObject(typeof(TTenantInfo));
        defaults.Bind(newTenant, options => options.BindNonPublicProperties = true);
        tenantSection.Bind(newTenant, options => options.BindNonPublicProperties = true);
        return newTenant;
    }

    // Layers the tenant section over Defaults into one configuration (the tenant's keys take precedence) and passes it
    // to the factory. The temporary configuration is disposed afterward to release its reload token registrations.
    private static TTenantInfo CreateTenantInfo(Func<IConfiguration, TTenantInfo> factory,
        IConfigurationSection defaults, IConfigurationSection tenantSection)
    {
        var merged = new ConfigurationBuilder()
            .AddConfiguration(defaults)
            .AddConfiguration(tenantSection)
            .Build();

        using var mergedLifetime = merged as IDisposable;
        return factory(merged) ??
               throw new MultiTenantException(
                   $"The tenant info factory returned null for configuration section '{tenantSection.Path}'.");
    }

    /// <summary>
    /// Not implemented in this implementation.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public Task<bool> AddAsync(TTenantInfo tenantInfo)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<TTenantInfo?> GetAsync(string id)
    {
        ArgumentNullException.ThrowIfNull(id);
        return Task.FromResult(tenantMap.Values.SingleOrDefault(v => v.Id == id));
    }

    /// <inheritdoc />
    public Task<IEnumerable<TTenantInfo>> GetAllAsync()
    {
        return Task.FromResult(tenantMap.Values.ToList().AsEnumerable());
    }

    /// <inheritdoc />
    public Task<IEnumerable<TTenantInfo>> GetAllAsync(int take, int skip)
    {
        return Task.FromResult(tenantMap.Values.Skip(skip).Take(take).ToList().AsEnumerable());
    }

    /// <inheritdoc />
    public Task<TTenantInfo?> GetByIdentifierAsync(string identifier)
    {
        ArgumentNullException.ThrowIfNull(identifier);
        return Task.FromResult(tenantMap.GetValueOrDefault(identifier));
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
    public Task<bool> UpdateAsync(TTenantInfo tenantInfo)
    {
        throw new NotImplementedException();
    }
}
