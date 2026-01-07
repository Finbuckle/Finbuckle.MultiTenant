// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant.Stores;

/// <summary>
/// Basic store that can only retrieve tenant via HTTP calls. Note that add, update, and remove functionality is not
/// implemented. Any changes to the tenant store must occur on the server.
/// </summary>
/// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
public class HttpRemoteStore<TTenantInfo> : IMultiTenantStore<TTenantInfo>
    where TTenantInfo : ITenantInfo
{
    // internal for testing, static for use in the client
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
            if (endpointTemplate.EndsWith('/'))
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
    public Task<bool> AddAsync(TTenantInfo tenantInfo)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Not implemented in this implementation.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public Task<TTenantInfo?> GetAsync(string id)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    /// <exception cref="NotImplementedException">When a not-found (404) status code is encountered</exception>
    public async Task<IEnumerable<TTenantInfo>> GetAllAsync()
    {
        return await _client.GetAllAsync(endpointTemplate).ConfigureAwait(false);
    }

    /// <summary>
    /// Not implemented in this implementation.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public Task<IEnumerable<TTenantInfo>> GetAllAsync(int take, int skip)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async Task<TTenantInfo?> GetByIdentifierAsync(string identifier)
    {
        var result = await _client.GetByIdentifierAsync(endpointTemplate, identifier).ConfigureAwait(false);
        return result;
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