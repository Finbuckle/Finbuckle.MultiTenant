// Copyright 2018-2020 Andrew White
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
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Finbuckle.MultiTenant.Stores
{
    public class HttpRemoteStoreClient<TTenantInfo> where TTenantInfo : class, ITenantInfo, new()
    {
        private readonly IHttpClientFactory clientFactory;

        public HttpRemoteStoreClient(IHttpClientFactory clientFactory)
        {
            this.clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        }

        public async Task<TTenantInfo> TryGetByIdentifierAsync(string endpointTemplate, string identifier)
        {
            var client = clientFactory.CreateClient(typeof(HttpRemoteStoreClient<TTenantInfo>).FullName);
            var uri = endpointTemplate.Replace(HttpRemoteStore<TTenantInfo>.defaultEndpointTemplateIdentifierToken, identifier);
            var response = await client.GetAsync(uri);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<TTenantInfo>(json);

            return result;
        }
    }
}