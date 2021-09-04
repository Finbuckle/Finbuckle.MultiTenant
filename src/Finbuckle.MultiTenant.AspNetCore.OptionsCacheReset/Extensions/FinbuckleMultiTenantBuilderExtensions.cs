using System;
using Finbuckle.MultiTenant.AspNetCore.OptionsCacheReset.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Finbuckle.MultiTenant.AspNetCore.OptionsCacheReset
{
    public static class FinbuckleMultiTenantBuilderExtensions
    {
        
        /// <summary>
        /// Adds per-tenant configuration for an options class with cache reset .
        /// </summary>
        /// <param name="tenantConfigureOptions">The configuration action to be run for each tenant.</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder<TVersionTenantInfo>
            WithPerTenantManagedCacheOptions<TVersionTenantInfo, TOptions>(
            FinbuckleMultiTenantBuilder<TVersionTenantInfo> builder,
            Action<TOptions, TVersionTenantInfo> tenantConfigureOptions)
        where TOptions : class, new()
        where TVersionTenantInfo : class, IVersionTenantInfo, new()

    {
        builder.WithPerTenantOptions(tenantConfigureOptions);
        builder.Services.TryAddSingleton<TenantVersionStore>();
        builder.Services.TryAddSingleton(new MultiTenantOptionMark(typeof(TOptions)));
        return builder;
        }
    }
}