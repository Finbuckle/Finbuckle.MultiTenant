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

#if NETSTANDARD2_0

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Finbuckle.MultiTenant.AspNetCore
{
    internal class MultiTenantAuthenticationService : AuthenticationService
    {
        public MultiTenantAuthenticationService(IAuthenticationSchemeProvider schemes,
                                                IAuthenticationHandlerProvider handlers,
                                                IClaimsTransformation transform) : base(schemes, handlers, transform)
        {
        }

        public override async Task ChallengeAsync(HttpContext context, string scheme, AuthenticationProperties properties)
        {
            // Add tenant identifier to the properties so on the callback we can use it to set the multitenant context.
            var multiTenantContext = context.GetMultiTenantContext();
            if (multiTenantContext.TenantInfo != null)
            {
                properties = properties ?? new AuthenticationProperties();
                properties.Items.Add("tenantIdentifier", multiTenantContext.TenantInfo.Identifier);
            }

            await base.ChallengeAsync(context, scheme, properties);
        }
    }
}

#else

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Finbuckle.MultiTenant.AspNetCore
{
    internal class MultiTenantAuthenticationService : AuthenticationService
    {
        public MultiTenantAuthenticationService(IAuthenticationSchemeProvider schemes,
                                                IAuthenticationHandlerProvider handlers,
                                                IClaimsTransformation transform,
                                                IOptions<AuthenticationOptions> options) : base(schemes, handlers, transform, options)
        {
        }

        public override async Task ChallengeAsync(HttpContext context, string scheme, AuthenticationProperties properties)
        {
            // Add tenant identifier to the properties so on the callback we can use it to set the multitenant context.
            var multiTenantContext = context.GetMultiTenantContext();
            if (multiTenantContext.TenantInfo != null)
            {
                properties = properties ?? new AuthenticationProperties();
                properties.Items.Add("tenantIdentifier", multiTenantContext.TenantInfo.Identifier);
            }

            await base.ChallengeAsync(context, scheme, properties);
        }
    }
}

#endif