// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Net;
using System.Text.Json;
using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant.Stores;

/// <summary>
/// HTTP client for retrieving tenant information from a remote endpoint.
/// </summary>
/// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
public class HttpRemoteStoreClient<TTenantInfo> where TTenantInfo : ITenantInfo
{
    private readonly IHttpClientFactory clientFactory;
    private readonly JsonSerializerOptions _defaultSerializerOptions;

    /// <summary>
    /// Initializes a new instance of HttpRemoteStoreClient.
    /// </summary>
    /// <param name="clientFactory">The HTTP client factory.</param>
    /// <param name="serializerOptions">Optional JSON serializer options.</param>
    /// <exception cref="ArgumentNullException">Thrown when clientFactory is null.</exception>
    public HttpRemoteStoreClient(IHttpClientFactory clientFactory, JsonSerializerOptions? serializerOptions = null)
    {
        this.clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));

        _defaultSerializerOptions = serializerOptions ?? new JsonSerializerOptions(JsonSerializerDefaults.Web);
    }

    /// <summary>
    /// Attempts to retrieve tenant information by identifier from the remote endpoint.
    /// </summary>
    /// <param name="endpointTemplate">The endpoint template containing the identifier token.</param>
    /// <param name="identifier">The tenant identifier.</param>
    /// <returns>The tenant information if found, otherwise null.</returns>
    public async Task<TTenantInfo?> GetByIdentifierAsync(string endpointTemplate, string identifier)
    {
        var client = clientFactory.CreateClient(typeof(HttpRemoteStoreClient<TTenantInfo>).FullName!);
        var uri = endpointTemplate.Replace(HttpRemoteStore<TTenantInfo>.DefaultEndpointTemplateIdentifierToken,
            identifier);
        var response = await client.GetAsync(uri).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            return default;

        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var result = JsonSerializer.Deserialize<TTenantInfo>(json, _defaultSerializerOptions);

        return result;
    }

    /// <summary>
    /// Retrieves all tenants from the remote endpoint.
    /// </summary>
    /// <param name="endpointTemplate">The endpoint template.</param>
    /// <returns>An IEnumerable of all tenant information.</returns>
    /// <exception cref="NotImplementedException">Thrown when the remote endpoint returns a 404 status code.</exception>
    public async Task<IEnumerable<TTenantInfo>> GetAllAsync(string endpointTemplate)
    {
        var client = clientFactory.CreateClient(typeof(HttpRemoteStoreClient<TTenantInfo>).FullName!);
        var uri = endpointTemplate.Replace(HttpRemoteStore<TTenantInfo>.DefaultEndpointTemplateIdentifierToken,
            string.Empty);
        var response = await client.GetAsync(uri).ConfigureAwait(false);


        if (!response.IsSuccessStatusCode)
        {
            // Backwards compatibility check for service implementations that do not include this route.
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new NotImplementedException();
            }

            return Enumerable.Empty<TTenantInfo>();
        }

        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var result = JsonSerializer.Deserialize<IEnumerable<TTenantInfo>>(json, _defaultSerializerOptions);

        return result!;
    }
}