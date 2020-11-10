//    Copyright 2018-2020 Finbuckle LLC, Andrew White, and Contributors
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
    public class BasePathStrategy : IMultiTenantStrategy
    {
        public async Task<string> GetIdentifierAsync(object context)
        {
            if(!(context is HttpContext))
                throw new MultiTenantException(null,
                    new ArgumentException($"\"{nameof(context)}\" type must be of type HttpContext", nameof(context)));

            var path = (context as HttpContext).Request.Path;

            var pathSegments =
                path.Value.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (pathSegments.Length == 0)
                return null;

            string identifier = pathSegments[0];

            return await Task.FromResult(identifier); // Prevent the compliler warning that no await exists.
        }
    }
}