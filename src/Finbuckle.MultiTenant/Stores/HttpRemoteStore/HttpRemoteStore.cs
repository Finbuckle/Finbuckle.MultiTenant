// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.Internal;

// ReSharper disable once CheckNamespace
namespace Finbuckle.MultiTenant.Stores;

/// <summary>
/// Basic store that can only retrieve tenant via HTTP calls. Note that add, update, and remove functionality is not
/// implemented. Any changes to the tenant store must occur on the server.
/// </summary>
/// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
public class HttpRemoteStore<TTenantInfo> : IMultiTenantStore<TTenantInfo>
    where TTenantInfo : class, ITenantInfo, new()
{
    // ReSharper disable once StaticMemberInGenericType
    // (also used on HttpRemoteStoreClient)
    internal static readonly string DefaultEndpointTemplateIdentifierToken = $"{{{Constants.TenantToken}}}";
    private readonly HttpRemoteStoreClient<TTenantInfo> _client;
    private readonly string endpointTemplate;

    /// <summary>
    /// Constructor for HttpRemoteStore.
    /// </summary>
    /// <param name="client">HttpRemoteStoreClient instance used to retrieve tenant information.</param>
    /// <param name="endpointTemplate">Template string for the remote endpoint.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public HttpRemoteStore(HttpRemoteStoreClient<TTenantInfo> client, string endpointTemplate)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        if (!endpointTemplate.Contains(DefaultEndpointTemplateIdentifierToken))
        {
            if (endpointTemplate.EndsWith("/"))
                endpointTemplate += DefaultEndpointTemplateIdentifierToken;
            else
                endpointTemplate += $"/{DefaultEndpointTemplateIdentifierToken}";
        }

        if (Uri.IsWellFormedUriString(endpointTemplate, UriKind.Absolute))
            throw new ArgumentException("Parameter 'endpointTemplate' is not a well formed uri.",
                nameof(endpointTemplate));

        if (!endpointTemplate.StartsWith("https", StringComparison.OrdinalIgnoreCase)
            && !endpointTemplate.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Parameter 'endpointTemplate' is not a an http or https uri.",
                nameof(endpointTemplate));

        this.endpointTemplate = endpointTemplate;
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
    public Task<TTenantInfo?> TryGetAsync(string id)
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

    /// <inheritdoc />
    public async Task<TTenantInfo?> TryGetByIdentifierAsync(string identifier)
    {
        var result = await _client.TryGetByIdentifierAsync(endpointTemplate, identifier);
        return result;
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