// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Collections.Generic;

namespace Finbuckle.MultiTenant.Options;

class TenantConfigureNamedOptionsWrapper<TOptions, TTenantInfo> : ITenantConfigureNamedOptionsWrapper<TOptions>
    where TOptions : class, new()
    where TTenantInfo : class, ITenantInfo, new()
{
    private readonly IMultiTenantContextAccessor<TTenantInfo> multiTenantContextAccessor;
    private readonly ITenantConfigureOptions<TOptions, TTenantInfo>[] tenantConfigureOptions;
    private readonly ITenantConfigureNamedOptions<TOptions, TTenantInfo>[] tenantConfigureNamedOptions;

    public TenantConfigureNamedOptionsWrapper(
        IMultiTenantContextAccessor<TTenantInfo> multiTenantContextAccessor,
        IEnumerable<ITenantConfigureOptions<TOptions, TTenantInfo>> tenantConfigureOptions,
        IEnumerable<ITenantConfigureNamedOptions<TOptions, TTenantInfo>> tenantConfigureNamedOptions)
    {
        this.multiTenantContextAccessor = multiTenantContextAccessor;
        this.tenantConfigureOptions = tenantConfigureOptions as ITenantConfigureOptions<TOptions, TTenantInfo>[] ??
            new List<ITenantConfigureOptions<TOptions, TTenantInfo>>(tenantConfigureOptions).ToArray();
        this.tenantConfigureNamedOptions = tenantConfigureNamedOptions as ITenantConfigureNamedOptions<TOptions, TTenantInfo>[] ??
            new List<ITenantConfigureNamedOptions<TOptions, TTenantInfo>>(tenantConfigureNamedOptions).ToArray();
    }

    public void Configure(string name, TOptions options)
    {
        if (multiTenantContextAccessor.MultiTenantContext?.HasResolvedTenant ?? false)
        {
            foreach (var tenantConfigureOption in tenantConfigureOptions)
                tenantConfigureOption.Configure(options, multiTenantContextAccessor.MultiTenantContext.TenantInfo!);

            // Configure tenant named options.
            foreach (var tenantConfigureNamedOption in tenantConfigureNamedOptions)
                tenantConfigureNamedOption.Configure(name, options,
                    multiTenantContextAccessor.MultiTenantContext.TenantInfo!);
        }
    }

    public void Configure(TOptions options)
    {
        if (multiTenantContextAccessor.MultiTenantContext?.HasResolvedTenant ?? false)
        {
            foreach (var tenantConfigureOption in tenantConfigureOptions)
                tenantConfigureOption.Configure(options, multiTenantContextAccessor.MultiTenantContext.TenantInfo!);

            // Configure tenant named options.
            foreach (var tenantConfigureNamedOption in tenantConfigureNamedOptions)
                tenantConfigureNamedOption.Configure(Microsoft.Extensions.Options.Options.DefaultName, options,
                    multiTenantContextAccessor.MultiTenantContext.TenantInfo!);
        }
    }
}
