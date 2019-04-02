//    Copyright 2018 Andrew White
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
using Finbuckle.MultiTenant.AspNetCore;
using Microsoft.AspNetCore.Http;

namespace Finbuckle.MultiTenant
{
    /// <summary>
    /// Finbuckle.MultiTenant.AspNetCore extensions to HttpContext.
    /// </summary>
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Returns the current MultiTenantContext or null if there is none.
        /// </summary>
        /// <param name="context">The HttpContext<c/> instance the extension method applies to.</param>
        /// <returns>The MultiTenantContext instance for the current tenant.</returns>
        public static MultiTenantContext GetMultiTenantContext(this HttpContext context)
        {
            object multiTenantContext = null;
            context.Items.TryGetValue(Constants.HttpContextMultiTenantContext, out multiTenantContext);
            
            return (MultiTenantContext)multiTenantContext;
        }
    }
}