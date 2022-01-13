// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;
using System.Net.Http;
using System.Threading.Tasks;
#if NET5_0_OR_GREATER
using System.Text.Json;
#else
using Newtonsoft.Json;
#endif
namespace Finbuckle.MultiTenant.Stores
{
    public class HttpRemoteStoreClient<TTenantInfo> where TTenantInfo : class, ITenantInfo, new()
    {
        private readonly IHttpClientFactory clientFactory;

        public HttpRemoteStoreClient(IHttpClientFactory clientFactory)
        {
            this.clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        }

        public async Task<TTenantInfo?> TryGetByIdentifierAsync(string endpointTemplate, string identifier)
        {
            var client = clientFactory.CreateClient(typeof(HttpRemoteStoreClient<TTenantInfo>).FullName!);
            var uri = endpointTemplate.Replace(HttpRemoteStore<TTenantInfo>.defaultEndpointTemplateIdentifierToken, identifier);
            var response = await client.GetAsync(uri);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
#if NET5_0_OR_GREATER
            var result = JsonSerializer.Deserialize<TTenantInfo>(json);
#else
            var result = JsonConvert.DeserializeObject<TTenantInfo>(json);
#endif

            return result;
        }
    }
}