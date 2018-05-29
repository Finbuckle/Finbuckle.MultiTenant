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
using Finbuckle.MultiTenant.AspNetCore;
using Finbuckle.MultiTenant.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Configure <c>Finbuckle.MultiTenant</c> services for the application.
        /// </summary>
        /// <param name="services">The <c>IServiceCollection<c/> instance the extension method applies to.</param>
        /// <returns>An new instance of <c>MultiTenantBuilder</c>.</returns>
        public static MultiTenantBuilder AddMultiTenant(this IServiceCollection services)
        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddSingleton<TenantResolver>();
            services.TryAddScoped<TenantContext>(sp => sp.GetRequiredService<IHttpContextAccessor>().HttpContext.GetTenantContextAsync().Result);
            
            return new MultiTenantBuilder(services);
        }
    }
}