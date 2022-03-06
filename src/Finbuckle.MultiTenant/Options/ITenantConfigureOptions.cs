// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;

namespace Finbuckle.MultiTenant.Options
{
    [Obsolete]
    public interface ITenantConfigureOptions<TOptions, TTenantInfo>
        where TOptions : class, new()
        where TTenantInfo : class, ITenantInfo, new()
    {
        void Configure(TOptions options, TTenantInfo tenantInfo);
    }
}