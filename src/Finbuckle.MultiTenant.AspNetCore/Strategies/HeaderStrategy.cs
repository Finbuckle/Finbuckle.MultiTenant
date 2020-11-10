//    Copyright 2020 Finbuckle LLC, Andrew White, and Contributors
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
    public class HeaderStrategy : IMultiTenantStrategy
    {
        private readonly string _headerKey;
        public HeaderStrategy(string headerKey)
        {
            _headerKey = headerKey;
        }

        public async Task<string> GetIdentifierAsync(object context)
        {
            if (!(context is HttpContext httpContext))
                throw new MultiTenantException(null,
                    new ArgumentException($"\"{nameof(context)}\" type must be of type HttpContext", nameof(context)));

            return await Task.FromResult(httpContext?.Request.Headers[_headerKey]);
        }
    }
}
