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

using System.Threading.Tasks;
using Finbuckle.MultiTenant.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Finbuckle.MultiTenant.AspNetCore
{
    /// <summary>
    /// Middleware for resolving the <c>TenantContext</c> and storing it in <c>HttpContext</c>.
    /// </summary>
    public class MultiTenantMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IRouter router;

        public MultiTenantMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public MultiTenantMiddleware(RequestDelegate next, IRouter router)
        {
            this.next = next;
            this.router = router;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.Items.ContainsKey(Constants.HttpContextTenantContext))
            {
                var sp = context.RequestServices;
                var resolver = sp.GetRequiredService<TenantResolver>();

                if(router != null)
                {
                    await router.RouteAsync(new RouteContext(context)).ConfigureAwait(false);
                }
                
                var tc = await resolver.ResolveAsync(context).ConfigureAwait(false);
                if (tc != null)
                    context.Items.Add(Constants.HttpContextTenantContext, tc);
            }

            if(next != null)
                await next(context);
        }
    }
}