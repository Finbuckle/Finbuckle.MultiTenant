using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Finbuckle.MultiTenant.Stores
{
    public class HttpRemoteStoreClient
    {
        private readonly HttpClient client;
        private readonly Uri endpoint;

        public HttpRemoteStoreClient(HttpClient client, string endpoint)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));

            if (!Uri.TryCreate(endpoint, UriKind.Absolute, out this.endpoint))
                throw new ArgumentException("Paramter 'endpoint' is not a well formed uri.", nameof(endpoint));

            if (!(this.endpoint.Scheme == "https" || this.endpoint.Scheme == "http"))
                throw new ArgumentException("Paramter 'endpoint' is not a an http or https uri.", nameof(endpoint));
        }

        public async Task<TenantInfo> TryGetByIdentifierAsync(string identifier)
        {
            if (!endpoint.AbsolutePath.EndsWith("/"))
            {
                identifier = "/" + Uri.EscapeDataString(identifier);
            }

            var builder = new UriBuilder(endpoint);
            builder.Path += identifier;

            var response = await client.GetAsync(builder.Uri);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            var anon = new { Id = "", Identifier = "", Name = "", ConnectionString = "" };
            var settings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            var result = JsonConvert.DeserializeAnonymousType(json, anon);

            return new TenantInfo(result.Id, result.Identifier, result.Name, result.ConnectionString, null);
        }
    }
}