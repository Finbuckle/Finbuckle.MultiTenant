namespace Finbuckle.MultiTenant.AspNetCore
{
    public class MultiTenantAuthenticationOptions
    {
        public bool SkipChallengeIfTenantNotResolved { get; set; } = false;
    }
}
