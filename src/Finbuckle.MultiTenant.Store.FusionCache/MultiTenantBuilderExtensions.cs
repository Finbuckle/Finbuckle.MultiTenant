using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Internal;
using Finbuckle.MultiTenant.Stores.DistributedCacheStore;
using Microsoft.Extensions.DependencyInjection;

namespace Finbuckle.MultiTenant.Store.FusionCache;

public static class MultiTenantBuilderExtensionsFusionCache
{
    /// <summary>
    /// Adds FusionCache to the application.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="slidingExpiration">The timespan for a cache entry's sliding expiration.</param>
    public static MultiTenantBuilder<TTenantInfo> WithFusionCacheStore<TTenantInfo>(this MultiTenantBuilder<TTenantInfo> builder, TimeSpan? slidingExpiration)
        where TTenantInfo : class, ITenantInfo, new()
    {
        var storeParams = slidingExpiration is null ? new object[] { Constants.TenantToken } : new object[] { Constants.TenantToken, slidingExpiration };

        return builder.WithStore<FusionCacheStore<TTenantInfo>>(ServiceLifetime.Transient, storeParams);
    }
}