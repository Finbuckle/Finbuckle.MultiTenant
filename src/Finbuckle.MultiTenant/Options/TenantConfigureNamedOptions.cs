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
        private readonly Action<IServiceProvider, TOptions, TTenantInfo> configureOptions;
        private readonly IServiceProvider serviceProvider;

        public TenantConfigureNamedOptions(string? name, Action<IServiceProvider, TOptions, TTenantInfo> configureOptions, IServiceProvider serviceProvider)
        {
            Name = name;
            this.configureOptions = configureOptions;
            this.serviceProvider = serviceProvider;
        }

        public void Configure(string name, TOptions options, TTenantInfo tenantInfo)
        {
            // Null name is used to configure all named options.
            if (Name == null || name == Name)
            {
                configureOptions(serviceProvider, options, tenantInfo);
            }
        }
    }
}