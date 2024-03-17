// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

// ReSharper disable once CheckNamespace
namespace Finbuckle.MultiTenant.Stores;

/// <summary>
/// Basic store that uses .NET configuration to define tenants. Note that add, update, and remove functionality is not
/// implemented. If underlying configuration supports reload-on-change then this store will reflect such changes.
/// </summary>
/// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
public class ConfigurationStore<TTenantInfo> : IMultiTenantStore<TTenantInfo> where TTenantInfo : class, ITenantInfo, new()
{
    private const string DefaultSectionName = "Finbuckle:MultiTenant:Stores:ConfigurationStore";
    private readonly IConfigurationSection section;
    private ConcurrentDictionary<string, TTenantInfo>? tenantMap;

    // ReSharper disable once IntroduceOptionalParameters.Global
    /// <summary>
    /// Constructor for ConfigurationStore. Uses a section name of "Finbuckle:MultiTenant:Stores:ConfigurationStore".
    /// </summary>
    /// <param name="configuration">IConfiguration instance containing tenant information.</param>
    public ConfigurationStore(IConfiguration configuration) : this(configuration, DefaultSectionName)
    {
    }

    // ReSharper disable once IntroduceOptionalParameters.Global
    /// <summary>
    /// Constructor for ConfigurationStore.
    /// </summary>
    /// <param name="configuration">IConfiguration instance containing tenant information.</param>
    /// <param name="sectionName">Name of the section within the configuration containing tenant information.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="MultiTenantException"></exception>
    public ConfigurationStore(IConfiguration configuration, string sectionName)
    {
        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        if (string.IsNullOrEmpty(sectionName))
        {
            throw new ArgumentException("Section name provided to the Configuration Store is null or empty.", nameof(sectionName));
        }

        section = configuration.GetSection(sectionName);
        if(!section.Exists())
        {
            throw new MultiTenantException("Section name provided to the Configuration Store is invalid.");
        }

        UpdateTenantMap();
        ChangeToken.OnChange(() => section.GetReloadToken(), UpdateTenantMap);
    }

    private void UpdateTenantMap()
    {
        var newMap = new ConcurrentDictionary<string, TTenantInfo>(StringComparer.OrdinalIgnoreCase);
        var tenants = section.GetSection("Tenants").GetChildren();

        foreach(var tenantSection in tenants)
        {
            var newTenant = section.GetSection("Defaults").Get<TTenantInfo>(options => options.BindNonPublicProperties = true) ?? new TTenantInfo();
            tenantSection.Bind(newTenant, options => options.BindNonPublicProperties = true);

            // Throws an ArgumentNullException if the identifier is null.
            newMap.TryAdd(newTenant.Identifier!, newTenant);
        }

        tenantMap = newMap;
    }

    /// <summary>
    /// Not implemented in this implementation.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public Task<bool> TryAddAsync(TTenantInfo tenantInfo)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async Task<TTenantInfo?> TryGetAsync(string id)
    {
        if (id is null)
        {
            throw new ArgumentNullException(nameof(id));
        }

        return await Task.FromResult(tenantMap?.Where(kv => kv.Value.Id == id).SingleOrDefault().Value);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TTenantInfo>> GetAllAsync()
    {
        return await Task.FromResult(tenantMap?.Select(x => x.Value).ToList() ?? new List<TTenantInfo>());
    }

    /// <inheritdoc />
    public async Task<TTenantInfo?> TryGetByIdentifierAsync(string identifier)
    {
        if (identifier is null)
        {
            throw new ArgumentNullException(nameof(identifier));
        }

        if (tenantMap is null)
        {
            return null;
        }

        return await Task.FromResult(tenantMap.TryGetValue(identifier, out var result) ? result : null);
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
    public Task<bool> TryUpdateAsync(TTenantInfo tenantInfo)
    {
        throw new NotImplementedException();
    }
}