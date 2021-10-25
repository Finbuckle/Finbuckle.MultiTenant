// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

namespace Finbuckle.MultiTenant.Options
{
    public interface ITenantConfigureOptions<TOptions, TTenantInfo>
        where TOptions : class, new()
        where TTenantInfo : class, ITenantInfo, new()
    {
        void Configure(TOptions options, TTenantInfo tenantInfo);
    }
}