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
using Finbuckle.MultiTenant.Core;
using Microsoft.AspNetCore.Http;

namespace Finbuckle.MultiTenant.AspNetCore
{
    /// <summary>
    /// <c>Finbuckle.MultiTenant.AspNetCore</c> extensions to <c>HttpContext</c>.
    /// </summary>
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Asyncronously retrieves the current <c>TenantContext</c> or null if there is no valid tenant context.
        /// </summary>
        /// <param name="context">The <c>HttpContext<c/> instance the extension method applies to.</param>
        /// <returns>The <c>TenantContext</c> instance for the current tenant.</returns>
        [Obsolete("This method is obsolete. Use GetTenantContext instead.")]
        public static async Task<TenantContext> GetTenantContextAsync(this HttpContext context)
        {            
            return await Task.FromResult(context.GetTenantContext());
        }

        /// <summary>
        /// Returns the current <c>TenantContext</c> or null if there is none.
        /// </summary>
        /// <param name="context">The <c>HttpContext<c/> instance the extension method applies to.</param>
        /// <returns>The <c>TenantContext</c> instance for the current tenant.</returns>
        public static TenantContext GetTenantContext(this HttpContext context)
        {
            object tenantContext = null;
            context.Items.TryGetValue(Constants.HttpContextTenantContext, out tenantContext);
            
            return (TenantContext)tenantContext;
        }
    }
}