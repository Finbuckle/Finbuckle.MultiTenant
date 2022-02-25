// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;

namespace Finbuckle.MultiTenant.Options
{
    public class TenantConfigureNamedOptions<TOptions, TTenantInfo> : ITenantConfigureNamedOptions<TOptions, TTenantInfo>
        where TOptions : class, new()
        where TTenantInfo : class, ITenantInfo, new()
    {
        private readonly Action<string, TOptions, TTenantInfo> configureOptions;

        public TenantConfigureNamedOptions(Action<string, TOptions, TTenantInfo> configureOptions)
        {
            this.configureOptions = configureOptions;
        }

        public void Configure(string name, TOptions options, TTenantInfo tenantInfo)
        {
            configureOptions(name, options, tenantInfo);
        }
    }
}