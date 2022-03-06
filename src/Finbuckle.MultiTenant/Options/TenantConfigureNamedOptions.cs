// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;

namespace Finbuckle.MultiTenant.Options
{
    public class TenantConfigureNamedOptions<TOptions, TTenantInfo> : ITenantConfigureNamedOptions<TOptions, TTenantInfo>
        where TOptions : class, new()
        where TTenantInfo : class, ITenantInfo, new()
    {
        public string? Name { get; }
        private readonly Action<TOptions, TTenantInfo> configureOptions;

        public TenantConfigureNamedOptions(string? name, Action<TOptions, TTenantInfo> configureOptions)
        {
            Name = name;
            this.configureOptions = configureOptions;
        }

        public void Configure(string name, TOptions options, TTenantInfo tenantInfo)
        {
            // Null name is used to configure all named options.
            if (Name == null || name == Name)
            {
                configureOptions(options, tenantInfo);
            }
        }
    }
}