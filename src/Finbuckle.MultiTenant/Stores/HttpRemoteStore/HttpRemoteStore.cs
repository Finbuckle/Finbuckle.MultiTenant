// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.Internal;

namespace Finbuckle.MultiTenant.Stores
{
    public class HttpRemoteStore<TTenantInfo> : IMultiTenantStore<TTenantInfo>
        where TTenantInfo : class, ITenantInfo, new()
    {
        internal static readonly string defaultEndpointTemplateIdentifierToken = $"{{{Constants.TenantToken}}}";
        private readonly HttpRemoteStoreClient<TTenantInfo> client;
        private readonly string endpointTemplate;

        public HttpRemoteStore(HttpRemoteStoreClient<TTenantInfo> client, string endpointTemplate)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
            if (!endpointTemplate.Contains(defaultEndpointTemplateIdentifierToken))
            {
                if(endpointTemplate.EndsWith("/"))
                    endpointTemplate += defaultEndpointTemplateIdentifierToken;
                else
                    endpointTemplate += $"/{defaultEndpointTemplateIdentifierToken}";
            }

            if (Uri.IsWellFormedUriString(endpointTemplate, UriKind.Absolute))
                throw new ArgumentException("Paramter 'endpointTemplate' is not a well formed uri.", nameof(endpointTemplate));

            if (!endpointTemplate.StartsWith("https", StringComparison.OrdinalIgnoreCase)
                && !endpointTemplate.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Paramter 'endpointTemplate' is not a an http or https uri.", nameof(endpointTemplate));

            this.endpointTemplate = endpointTemplate;
        }

        public Task<bool> TryAddAsync(TTenantInfo tenantInfo)
        {
            throw new System.NotImplementedException();
        }

        public Task<TTenantInfo?> TryGetAsync(string id)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<TTenantInfo>> GetAllAsync()
        {
	        throw new NotImplementedException();
        }

        public async Task<TTenantInfo?> TryGetByIdentifierAsync(string identifier)
        {
            var result = await client.TryGetByIdentifierAsync(endpointTemplate, identifier);
            return result;
        }

        public Task<bool> TryRemoveAsync(string id)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> TryUpdateAsync(TTenantInfo tenantInfo)
        {
            throw new System.NotImplementedException();
        }
    }
}