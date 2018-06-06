using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Finbuckle.MultiTenant.AspNetCore
{
    public class MultiTenantAuthenticationService : AuthenticationService
    {
        public MultiTenantAuthenticationService(IAuthenticationSchemeProvider schemes, IAuthenticationHandlerProvider handlers, IClaimsTransformation transform) : base(schemes, handlers, transform)
        {
        }

        public override async Task ChallengeAsync(HttpContext context, string scheme, AuthenticationProperties properties)
        {
            // Add tenant identifier to the properties so on the callback we can use it to set the tenant context.
            var tenantContext = context.GetTenantContext();
            if (tenantContext != null)
            {
                properties = new AuthenticationProperties();
                properties.Items.Add("tenantIdentifier", tenantContext.Identifier);
            }

            await base.ChallengeAsync(context, scheme, properties);
        }
    }
}
