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
/// implemented. If underlying configuration supports reload-on-change then this store will reflect such changes.
/// </summary>
/// <typeparam name="TTenantInfo">The <see cref="TenantInfo"/> derived type.</typeparam>
public class ConfigurationStore<TTenantInfo> : IMultiTenantStore<TTenantInfo> where TTenantInfo : TenantInfo
{
    private const string DefaultSectionName = "Finbuckle:MultiTenant:Stores:ConfigurationStore";
    private readonly IConfigurationSection section;
    private ConcurrentDictionary<string, TTenantInfo>? tenantMap;

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
    /// <exception cref="MultiTenantException">Thrown when the section name is invalid.</exception>
    public ConfigurationStore(IConfiguration configuration, string sectionName)
    {
        ArgumentNullException.ThrowIfNull(configuration);

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
            var newTenant = (TTenantInfo)RuntimeHelpers.GetUninitializedObject(typeof(TTenantInfo));

            defaults.Bind(newTenant, options => options.BindNonPublicProperties = true);
            tenantSection.Bind(newTenant, options => options.BindNonPublicProperties = true);

            newMap.TryAdd(newTenant.Identifier, newTenant);
        }

        tenantMap = newMap;
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
    public async Task<TTenantInfo?> GetAsync(string id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return await Task.FromResult(tenantMap?.Where(kv => kv.Value.Id == id).SingleOrDefault().Value)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TTenantInfo>> GetAllAsync()
    {
        return await Task.FromResult(tenantMap?.Select(x => x.Value).ToList() ?? new List<TTenantInfo>())
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TTenantInfo>> GetAllAsync(int take, int skip)
    {
        return await Task
            .FromResult(tenantMap?.Select(x => x.Value).Take(take).Skip(skip).ToList() ?? new List<TTenantInfo>())
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<TTenantInfo?> GetByIdentifierAsync(string identifier)
    {
        ArgumentNullException.ThrowIfNull(identifier);

        if (tenantMap is null)
        {
            return null;
        }

        return await Task.FromResult(tenantMap.TryGetValue(identifier, out var result) ? result : null)
            .ConfigureAwait(false);
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