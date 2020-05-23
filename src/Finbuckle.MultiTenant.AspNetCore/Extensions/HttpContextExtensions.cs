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
using Finbuckle.MultiTenant.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Finbuckle.MultiTenant
{
    /// <summary>
    /// Finbuckle.MultiTenant.AspNetCore extensions to HttpContext.
    /// </summary>
    public static class FinbuckleHttpContextExtensions
    {
        /// <summary>
        /// Returns the current MultiTenantContext or null if there is none.
        /// </summary>
        public static IMultiTenantContext<TTenantInfo> GetMultiTenantContext<TTenantInfo>(this HttpContext httpContext)
        where TTenantInfo : class, ITenantInfo, new()
        {
            return httpContext.RequestServices.GetRequiredService<IMultiTenantContextAccessor<TTenantInfo>>().MultiTenantContext;
        }

        /// <summary>
        /// Sets the provided TenantInfo on the MultiTenantContext.
        /// Sets StrategyInfo and StoreInfo on the MultiTenant Context to null.
        /// Optionally resets the current dependency injection service provider.
        /// </summary>
        public static bool TrySetTenantInfo<TTenantInfo>(this HttpContext httpContext, TTenantInfo tenantInfo, bool resetServiceProviderScope)
            where TTenantInfo : class, ITenantInfo, new()
        {
            if (resetServiceProviderScope)
                httpContext.RequestServices = httpContext.RequestServices.CreateScope().ServiceProvider;

            var multitenantContext = new MultiTenantContext<TTenantInfo>
            {
                TenantInfo = tenantInfo,
                StrategyInfo = null,
                StoreInfo = null
            };


            var accessor = httpContext.RequestServices.GetRequiredService<IMultiTenantContextAccessor<TTenantInfo>>();
            accessor.MultiTenantContext = multitenantContext;

            return true;
        }
    }
}