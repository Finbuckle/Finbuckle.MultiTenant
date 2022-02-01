// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Finbuckle.MultiTenant
{
    /// <summary>
    /// Finbuckle.MultiTenant.AspNetCore extensions to HttpContext.
    /// </summary>
    public static class FinbuckleHttpContextExtensions
    {
        /// <summary>
        /// Returns the current MultiTenantContext or null if there is none.
        /// </summary>
        public static IMultiTenantContext<T>? GetMultiTenantContext<T>(this HttpContext httpContext)
        where T : class, ITenantInfo, new()
        {
            var services = httpContext.RequestServices;
            var context = services.GetRequiredService<IMultiTenantContextAccessor<T>>();
            return context?.MultiTenantContext;
        }

        /// <summary>
        /// Sets the provided TenantInfo on the MultiTenantContext.
        /// Sets StrategyInfo and StoreInfo on the MultiTenant Context to null.
        /// Optionally resets the current dependency injection service provider.
        /// </summary>
        public static bool TrySetTenantInfo<T>(this HttpContext httpContext, T tenantInfo, bool resetServiceProviderScope)
            where T : class, ITenantInfo, new()
        {
            if (resetServiceProviderScope)
                httpContext.RequestServices = httpContext.RequestServices.CreateScope().ServiceProvider;

            var multitenantContext = new MultiTenantContext<T>
            {
                TenantInfo = tenantInfo,
                StrategyInfo = null,
                StoreInfo = null
            };

            var accessor = httpContext.RequestServices.GetRequiredService<IMultiTenantContextAccessor<T>>();
            accessor.MultiTenantContext = multitenantContext;

            return true;
        }
    }
}