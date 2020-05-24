//    Copyright 2018-2020 Andrew White
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Finbuckle.MultiTenant.Strategies
{
    public class SessionStrategy : IMultiTenantStrategy
    {
        private readonly string tenantKey;

        public SessionStrategy(string tenantKey)
        {
            if (string.IsNullOrWhiteSpace(tenantKey))
            {
                throw new ArgumentException("message", nameof(tenantKey));
            }

            this.tenantKey = tenantKey;
        }

        public async Task<string> GetIdentifierAsync(object context)
        {
            if(!(context is HttpContext))
                throw new MultiTenantException(null,
                    new ArgumentException($"\"{nameof(context)}\" type must be of type HttpContext", nameof(context)));

            var identifier = ((HttpContext)context).Session.GetString(tenantKey);
            return await Task.FromResult(identifier); // Prevent the compliler warning that no await exists.
        }
    }
}