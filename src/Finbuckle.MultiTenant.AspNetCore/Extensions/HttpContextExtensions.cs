// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Finbuckle.MultiTenant.AspNetCore.Extensions;

/// <summary>
/// Finbuckle.MultiTenant.AspNetCore extensions to HttpContext.
/// </summary>
public static class HttpContextExtensions
{
    /// <param name="httpContext">The <see cref="HttpContext"/> instance.</param>
    extension(HttpContext httpContext)
    {
        /// <summary>
        /// Returns the current <see cref="ITenantContext"/>.
        /// </summary>
        public ITenantContext TenantContext =>
            httpContext.RequestServices.GetRequiredService<ITenantContext>();

        /// <summary>
        /// Returns the current <see cref="ITenantInfo"/> instance or null if there is none.
        /// </summary>
        public ITenantInfo? TenantInfo =>
            httpContext.TenantContext.TenantInfo;

        /// <summary>
        /// Returns the current <see cref="ITenantContext{TTenantInfo}"/>.
        /// </summary>
        /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
        public ITenantContext<TTenantInfo> GetTenantContext<TTenantInfo>()
            where TTenantInfo : ITenantInfo
        {
            return httpContext.RequestServices.GetRequiredService<ITenantContext<TTenantInfo>>();
        }

        /// <summary>
        /// Returns the current generic <typeparamref name="TTenantInfo"/> instance or null if there is none.
        /// </summary>
        /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
        public TTenantInfo? GetTenantInfo<TTenantInfo>()
            where TTenantInfo : ITenantInfo =>
            httpContext.GetTenantContext<TTenantInfo>().TenantInfo;

        /// <summary>
        /// Sets the provided <typeparamref name="TTenantInfo"/> for the current request.
        /// </summary>
        /// <param name="tenantInfo">The tenant info instance to set as current.</param>
        /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
        /// <remarks>This method will throw a <see cref="MultiTenantException"/> if the <see cref="ITenantContext{TTenantInfo}.TenantInfo"/> has already been set.</remarks>
        public void SetTenantInfo<TTenantInfo>(TTenantInfo tenantInfo)
            where TTenantInfo : ITenantInfo
        {
            var tenantContext = httpContext.GetTenantContext<TTenantInfo>();
            tenantContext.TenantInfo = tenantInfo;
        }

        /// <summary>
        /// Sets the provided <typeparamref name="TTenantInfo"/> for the current request if it has not already been set.
        /// </summary>
        /// <param name="tenantInfo">The tenant info instance to set as current.</param>
        /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
        public void TrySetTenantInfo<TTenantInfo>(TTenantInfo tenantInfo)
            where TTenantInfo : ITenantInfo
        {
            var tenantContext = httpContext.GetTenantContext<TTenantInfo>();
            if (!tenantContext.IsResolved)
            {
                tenantContext.TenantInfo = tenantInfo;
            }
        }
    }
}
