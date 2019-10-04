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

using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Finbuckle.MultiTenant.AspNetCore
{
    internal class MultiTenantAuthenticationService : IAuthenticationService
    {
        private readonly IAuthenticationService inner;

        public MultiTenantAuthenticationService(IAuthenticationService inner)
        {
            this.inner = inner ?? throw new System.ArgumentNullException(nameof(inner));
        }

        public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string scheme)
            => inner.AuthenticateAsync(context, scheme);

        public Task ChallengeAsync(HttpContext context, string scheme, AuthenticationProperties properties)
        {
            // Add tenant identifier to the properties so on the callback we can use it to set the multitenant context.
            var multiTenantContext = context.GetMultiTenantContext();
            if (multiTenantContext.TenantInfo != null)
            {
                properties = properties ?? new AuthenticationProperties();
                properties.Items.Add("tenantIdentifier", multiTenantContext.TenantInfo.Identifier);
            }

            return inner.ChallengeAsync(context, scheme, properties);
        }

        public Task ForbidAsync(HttpContext context, string scheme, AuthenticationProperties properties)
            => inner.ForbidAsync(context, scheme, properties);

        public Task SignInAsync(HttpContext context, string scheme, ClaimsPrincipal principal, AuthenticationProperties properties)
            => inner.SignInAsync(context, scheme, principal, properties);

        public Task SignOutAsync(HttpContext context, string scheme, AuthenticationProperties properties)
            => inner.SignOutAsync(context, scheme, properties);
    }
}
