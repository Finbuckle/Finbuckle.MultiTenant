// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

namespace Finbuckle.MultiTenant.AspNetCore
{
    public class MultiTenantAuthenticationOptions
    {
        public bool SkipChallengeIfTenantNotResolved { get; set; } = false;
    }
}
