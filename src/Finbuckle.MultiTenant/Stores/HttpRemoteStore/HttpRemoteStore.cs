// Copyright 2018-2020 Finbuckle LLC, Andrew White, and Contributors
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

        public Task<bool> TryAddAsync(TTenantInfo TtenantInfo)
        {
            throw new System.NotImplementedException();
        }

        public Task<TTenantInfo> TryGetAsync(string id)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<TTenantInfo>> GetAllAsync()
        {
	        throw new NotImplementedException();
        }

        public async Task<TTenantInfo> TryGetByIdentifierAsync(string identifier)
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