// Copyright 2020 Andrew White
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Threading.Tasks;

namespace Finbuckle.MultiTenant.Stores
{
    public class HttpRemoteStore : IMultiTenantStore
    {
        internal const string defaultEndpointTemplateIdentifierToken = "{__tenant__}";
        private readonly HttpRemoteStoreClient client;
        private readonly string endpointTemplate;

        public HttpRemoteStore(HttpRemoteStoreClient client, string endpointTemplate)
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

        public Task<bool> TryAddAsync(TenantInfo tenantInfo)
        {
            throw new System.NotImplementedException();
        }

        public Task<TenantInfo> TryGetAsync(string id)
        {
            throw new System.NotImplementedException();
        }

        public async Task<TenantInfo> TryGetByIdentifierAsync(string identifier)
        {
            var result = await client.TryGetByIdentifierAsync(endpointTemplate, identifier);
            return result;
        }

        public Task<bool> TryRemoveAsync(string id)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> TryUpdateAsync(TenantInfo tenantInfo)
        {
            throw new System.NotImplementedException();
        }
    }
}