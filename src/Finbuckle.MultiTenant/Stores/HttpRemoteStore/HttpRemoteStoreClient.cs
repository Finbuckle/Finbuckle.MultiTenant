// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Net;
using System.Text.Json;
using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant.Stores.HttpRemoteStore;

public class HttpRemoteStoreClient<TTenantInfo> where TTenantInfo : class, ITenantInfo, new()
{
    private readonly IHttpClientFactory clientFactory;
    private readonly JsonSerializerOptions _defaultSerializerOptions;

    public HttpRemoteStoreClient(IHttpClientFactory clientFactory, JsonSerializerOptions? serializerOptions = default)
    {
        this.clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));

        _defaultSerializerOptions = serializerOptions ?? new JsonSerializerOptions(JsonSerializerDefaults.Web);
    }

    public async Task<TTenantInfo?> TryGetByIdentifierAsync(string endpointTemplate, string identifier)
    {
        var client = clientFactory.CreateClient(typeof(HttpRemoteStoreClient<TTenantInfo>).FullName!);
        var uri = endpointTemplate.Replace(HttpRemoteStore<TTenantInfo>.DefaultEndpointTemplateIdentifierToken, identifier);
        var response = await client.GetAsync(uri).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var result = JsonSerializer.Deserialize<TTenantInfo>(json, _defaultSerializerOptions);

        return result;
    }

    public async Task<IEnumerable<TTenantInfo>> GetAllAsync(string endpointTemplate)
    {
        var client = clientFactory.CreateClient(typeof(HttpRemoteStoreClient<TTenantInfo>).FullName!);
        var uri = endpointTemplate.Replace(HttpRemoteStore<TTenantInfo>.DefaultEndpointTemplateIdentifierToken, string.Empty);
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
