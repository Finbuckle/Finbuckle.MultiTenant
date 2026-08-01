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
        /// Returns the current <see cref="ITenantContext{TId}"/>.
        /// </summary>
        public ITenantContext<TId> TenantContext<TId>() where TId : IEquatable<TId> =>
            httpContext.RequestServices.GetRequiredService<ITenantContext<TId>>();

        /// <summary>
        /// Returns the current <see cref="ITenantInfo{TId}"/> instance or null if there is none.
        /// </summary>
        public ITenantInfo<TId>? TenantInfo<TId>() where TId : IEquatable<TId> =>
            httpContext.TenantContext<TId>().TenantInfo;

        /// <summary>
        /// Returns the current <see cref="ITenantContext{TTenantInfo}"/>.
        /// </summary>
        /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo{TId}"/> implementation type.</typeparam>
        /// <typeparam name="TId">The ID implementation type.</typeparam>
        public ITenantContext<TTenantInfo, TId> GetTenantContext<TTenantInfo, TId>()
            where TTenantInfo : ITenantInfo<TId> where TId : IEquatable<TId>
        {
            return httpContext.RequestServices.GetRequiredService<ITenantContext<TTenantInfo, TId>>();
        }

        /// <summary>
        /// Returns the current generic <typeparamref name="TTenantInfo"/> instance or null if there is none.
        /// </summary>
        /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo{TId}"/> implementation type.</typeparam>
        /// <typeparam name="TId">The ID implementation type.</typeparam>
        public TTenantInfo? GetTenantInfo<TTenantInfo, TId>()
            where TTenantInfo : ITenantInfo<TId> where TId : IEquatable<TId> =>
            httpContext.GetTenantContext<TTenantInfo, TId>().TenantInfo;

        /// <summary>
        /// Sets the provided <typeparamref name="TTenantInfo"/> for the current request.
        /// </summary>
        /// <param name="tenantInfo">The tenant info instance to set as current.</param>
        /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo{TId}"/> implementation type.</typeparam>
        /// <typeparam name="TId">The ID implementation type.</typeparam>
        /// <remarks>This method will throw a <see cref="MultiTenantException"/> if the <see cref="ITenantContext{TTenantInfo}.TenantInfo"/> has already been set.</remarks>
        public void SetTenantInfo<TTenantInfo, TId>(TTenantInfo tenantInfo)
            where TTenantInfo : ITenantInfo<TId> where TId : IEquatable<TId>
        {
            var tenantContext = httpContext.GetTenantContext<TTenantInfo, TId>();
            tenantContext.TenantInfo = tenantInfo;
        }

        /// <summary>
        /// Sets the provided <typeparamref name="TTenantInfo"/> for the current request if it has not already been set.
        /// </summary>
        /// <param name="tenantInfo">The tenant info instance to set as current.</param>
        /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo{TId}"/> implementation type.</typeparam>
        /// <typeparam name="TId">The ID implementation type.</typeparam>
        public void TrySetTenantInfo<TTenantInfo, TId>(TTenantInfo tenantInfo)
            where TTenantInfo : ITenantInfo<TId> where TId : IEquatable<TId>
        {
            var tenantContext = httpContext.GetTenantContext<TTenantInfo, TId>();
            if (!tenantContext.IsResolved)
            {
                tenantContext.TenantInfo = tenantInfo;
            }
        }
    }
}
