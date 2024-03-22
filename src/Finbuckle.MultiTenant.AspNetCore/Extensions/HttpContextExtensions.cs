// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System;
using Finbuckle.MultiTenant.Abstractions;
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
        /// Returns the current MultiTenantContext.
        /// </summary>
        /// <param name="httpContext">The HttpContext instance.</param>
        /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
        public static IMultiTenantContext<TTenantInfo> GetMultiTenantContext<TTenantInfo>(this HttpContext httpContext)
            where TTenantInfo : class, ITenantInfo, new()
        {
            if (httpContext.Items.TryGetValue(typeof(IMultiTenantContext), out var mtc) && mtc is not null)
                return (IMultiTenantContext<TTenantInfo>)mtc;
            
            mtc = new MultiTenantContext<TTenantInfo>();
            httpContext.Items[typeof(IMultiTenantContext)] = mtc;

            return (IMultiTenantContext<TTenantInfo>)mtc;
        }

        /// <summary>
        /// Returns the current generic TTenantInfo instance or null if there is none.
        /// </summary>
        /// <param name="httpContext">The HttpContext instance.</param>
        /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
        public static TTenantInfo? GetTenantInfo<TTenantInfo>(this HttpContext httpContext)
            where TTenantInfo : class, ITenantInfo, new() =>
            httpContext.GetMultiTenantContext<TTenantInfo>().TenantInfo;

        
        /// <summary>
        /// Sets the provided TenantInfo on the MultiTenantContext.
        /// Sets StrategyInfo and StoreInfo on the MultiTenant Context to null.
        /// Optionally resets the current dependency injection service provider.
        /// </summary>
        /// <param name="httpContext">The HttpContext instance.</param>
        /// <param name="tenantInfo">The tenant info instance to set as current.</param>
        /// <param name="resetServiceProviderScope">Creates a new service provider scope if true.</param>
        /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
        public static void SetTenantInfo<TTenantInfo>(this HttpContext httpContext, TTenantInfo tenantInfo,
            bool resetServiceProviderScope)
            where TTenantInfo : class, ITenantInfo, new()
        {
            if (resetServiceProviderScope)
                httpContext.RequestServices = httpContext.RequestServices.CreateScope().ServiceProvider;

            var multiTenantContext = new MultiTenantContext<TTenantInfo>
            {
                TenantInfo = tenantInfo,
                StrategyInfo = null,
                StoreInfo = null
            };

            var setter = httpContext.RequestServices.GetRequiredService<IMultiTenantContextSetter>();
            setter.MultiTenantContext = multiTenantContext;

            httpContext.Items[typeof(IMultiTenantContext)] = multiTenantContext;
        }
    }
}