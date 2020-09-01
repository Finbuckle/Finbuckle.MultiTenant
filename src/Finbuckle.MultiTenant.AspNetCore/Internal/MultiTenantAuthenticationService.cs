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

using System.Security.Claims;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.Internal;
using Finbuckle.MultiTenant.Strategies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Finbuckle.MultiTenant.AspNetCore
{
    internal class MultiTenantAuthenticationService<TTenantInfo> : IAuthenticationService
        where TTenantInfo : class, ITenantInfo, new()
    {
        private readonly IAuthenticationService inner;

        public MultiTenantAuthenticationService(IAuthenticationService inner)
        {
            this.inner = inner ?? throw new System.ArgumentNullException(nameof(inner));
        }

        private static void AddTenantIdentiferToProperties(HttpContext context, ref AuthenticationProperties properties)
        {
            // Add tenant identifier to the properties so on the callback we can use it to set the multitenant context.
            var multiTenantContext = context.GetMultiTenantContext<TTenantInfo>();
            if (multiTenantContext?.TenantInfo != null)
            {
                properties = properties ?? new AuthenticationProperties();
                if(!properties.Items.Keys.Contains(Constants.TenantToken))
                    properties.Items.Add(Constants.TenantToken, multiTenantContext.TenantInfo.Identifier);
            }
        }

        public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string scheme)
            => inner.AuthenticateAsync(context, scheme);

        public async Task ChallengeAsync(HttpContext context, string scheme, AuthenticationProperties properties)
        {
            AddTenantIdentiferToProperties(context, ref properties);
            await inner.ChallengeAsync(context, scheme, properties);
        }

        public async Task ForbidAsync(HttpContext context, string scheme, AuthenticationProperties properties)
        {
            AddTenantIdentiferToProperties(context, ref properties);
            await inner.ForbidAsync(context, scheme, properties);
        }

        public async Task SignInAsync(HttpContext context, string scheme, ClaimsPrincipal principal, AuthenticationProperties properties)
        {
            AddTenantIdentiferToProperties(context, ref properties);
            await inner.SignInAsync(context, scheme, principal, properties);
        }

        public async Task SignOutAsync(HttpContext context, string scheme, AuthenticationProperties properties)
        {
            AddTenantIdentiferToProperties(context, ref properties);
            await inner.SignOutAsync(context, scheme, properties);
        }
    }
}
