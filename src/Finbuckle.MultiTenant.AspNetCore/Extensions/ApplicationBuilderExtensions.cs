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
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Finbuckle.MultiTenant
{
    /// <summary>
    /// Extension methods for using <c>Finbuckle.MultiTenant.AspNetCore</c>.
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Use <c>Finbuckle.MultiTenant</c> middleware in processing the request.
        /// </summary>
        /// <param name="builder">The <c>IApplicationBuilder<c/> instance the extension method applies to.</param>
        /// <returns>The same <c>IApplicationBuilder</c> passed into the method.</returns>
        public static IApplicationBuilder UseMultiTenant(this IApplicationBuilder builder) =>
                builder.UseMiddleware<MultiTenantMiddleware>();
    }
}