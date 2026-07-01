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
    /// <summary>
    /// Returns the current <see cref="ITenantContext{TTenantInfo}"/>.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/> instance.</param>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    public static ITenantContext<TTenantInfo> GetTenantContext<TTenantInfo>(this HttpContext httpContext)
        where TTenantInfo : ITenantInfo
    {
        return httpContext.RequestServices.GetRequiredService<ITenantContext<TTenantInfo>>();
    }

    /// <summary>
    /// Returns the current generic <typeparamref name="TTenantInfo"/> instance or null if there is none.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/> instance.</param>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    public static TTenantInfo? GetTenantInfo<TTenantInfo>(this HttpContext httpContext)
        where TTenantInfo : ITenantInfo =>
        httpContext.GetTenantContext<TTenantInfo>().TenantInfo;
    
    /// <summary>
    /// Sets the provided <typeparamref name="TTenantInfo"/> for the current request.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/> instance.</param>
    /// <param name="tenantInfo">The tenant info instance to set as current.</param>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    /// <remarks>This method will throw a <see cref="MultiTenantException"/> if the <see cref="ITenantContext{TTenantInfo}.TenantInfo"/> has already been set.</remarks>
    public static void SetTenantInfo<TTenantInfo>(this HttpContext httpContext, TTenantInfo tenantInfo)
        where TTenantInfo : ITenantInfo
    {
        var tenantContext = httpContext.GetTenantContext<TTenantInfo>();
        tenantContext.TenantInfo = tenantInfo;
    }
    
    /// <summary>
    /// Sets the provided <typeparamref name="TTenantInfo"/> for the current request if it has not already been set.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/> instance.</param>
    /// <param name="tenantInfo">The tenant info instance to set as current.</param>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    public static void TrySetTenantInfo<TTenantInfo>(this HttpContext httpContext, TTenantInfo tenantInfo)
        where TTenantInfo : ITenantInfo
    {
        var tenantContext = httpContext.GetTenantContext<TTenantInfo>();
        if (!tenantContext.IsResolved)
        {
            tenantContext.TenantInfo = tenantInfo;
        }
    }
}