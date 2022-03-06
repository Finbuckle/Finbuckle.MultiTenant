// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;

namespace Finbuckle.MultiTenant.Options
{
    [Obsolete]
    public class TenantConfigureOptions<TOptions, TTenantInfo> : ITenantConfigureOptions<TOptions, TTenantInfo>
        where TOptions : class, new()
        where TTenantInfo : class, ITenantInfo, new()
    {
        private readonly Action<TOptions, TTenantInfo> configureOptions;

        public TenantConfigureOptions(Action<TOptions, TTenantInfo> configureOptions)
        {
            this.configureOptions = configureOptions;
        }

        public void Configure(TOptions options, TTenantInfo tenantInfo)
        {
            configureOptions(options, tenantInfo);
        }
    }
}