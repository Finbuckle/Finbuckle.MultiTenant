// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Text.Json;
using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant.Stores.HttpRemoteStore;

/// <summary>
/// The HttpRemoteStoreClient class is a generic class that is used to interact with a remote HTTP store.
/// </summary>
/// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
public class HttpRemoteStoreClient<TTenantInfo> where TTenantInfo : class, ITenantInfo, new()
{
    private readonly IHttpClientFactory clientFactory;

    /// <summary>
    /// Initializes a new instance of the HttpRemoteStoreClient class.
    /// </summary>
    /// <param name="clientFactory">An instance of IHttpClientFactory.</param>
    /// <exception cref="ArgumentNullException">Thrown when clientFactory is null.</exception>
    public HttpRemoteStoreClient(IHttpClientFactory clientFactory)
    {
        this.clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
    }

    /// <summary>
    /// Tries to get the tenant information by identifier from the remote HTTP store.
    /// </summary>
    /// <param name="endpointTemplate">The endpoint template to use when making the HTTP request.</param>
    /// <param name="identifier">The identifier of the tenant.</param>
    /// <returns>The tenant information if found; otherwise, null.</returns>
    public async Task<TTenantInfo?> TryGetByIdentifierAsync(string endpointTemplate, string identifier)
    {
        var client = clientFactory.CreateClient(typeof(HttpRemoteStoreClient<TTenantInfo>).FullName!);
        var uri = endpointTemplate.Replace(HttpRemoteStore<TTenantInfo>.DefaultEndpointTemplateIdentifierToken, identifier);
        var response = await client.GetAsync(uri);

        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TTenantInfo>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        return result;
    }
}