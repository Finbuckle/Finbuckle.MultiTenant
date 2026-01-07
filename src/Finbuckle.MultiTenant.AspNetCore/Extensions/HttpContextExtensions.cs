// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Finbuckle.MultiTenant.AspNetCore.Extensions;

/// <summary>
/// Finbuckle.MultiTenant.AspNetCore extensions to HttpContext.
/// </summary>
public static class FinbuckleHttpContextExtensions
{
    /// <summary>
    /// Returns the current <see cref="IMultiTenantContext{TTenantInfo}"/>.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/> instance.</param>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    public static IMultiTenantContext<TTenantInfo> GetMultiTenantContext<TTenantInfo>(this HttpContext httpContext)
        where TTenantInfo : ITenantInfo
    {
        if (httpContext.Items.TryGetValue(typeof(IMultiTenantContext), out var mtc) && mtc is not null)
            return (IMultiTenantContext<TTenantInfo>)mtc;

        mtc = new MultiTenantContext<TTenantInfo>(default);
        httpContext.Items[typeof(IMultiTenantContext)] = mtc;

        return (IMultiTenantContext<TTenantInfo>)mtc;
    }

    /// <summary>
    /// Returns the current generic <typeparamref name="TTenantInfo"/> instance or null if there is none.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/> instance.</param>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    public static TTenantInfo? GetTenantInfo<TTenantInfo>(this HttpContext httpContext)
        where TTenantInfo : ITenantInfo =>
        httpContext.GetMultiTenantContext<TTenantInfo>().TenantInfo;


    /// <summary>
    /// Sets the provided <typeparamref name="TTenantInfo"/> on the <see cref="IMultiTenantContext{TTenantInfo}"/>.
    /// Sets <see cref="StrategyInfo"/> and <see cref="StoreInfo{TTenantInfo}"/> on the <see cref="IMultiTenantContext{TTenantInfo}"/> to null.
    /// Optionally resets the current dependency injection service provider.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/> instance.</param>
    /// <param name="tenantInfo">The tenant info instance to set as current.</param>
    /// <param name="resetServiceProviderScope">Creates a new service provider scope if true.</param>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
    public static void SetTenantInfo<TTenantInfo>(this HttpContext httpContext, TTenantInfo tenantInfo,
        bool resetServiceProviderScope)
        where TTenantInfo : ITenantInfo
    {
        if (resetServiceProviderScope)
            httpContext.RequestServices = httpContext.RequestServices.CreateScope().ServiceProvider;

        var multiTenantContext =
            new MultiTenantContext<TTenantInfo>(tenantInfo: tenantInfo, strategyInfo: null, storeInfo: null);

        var setter = httpContext.RequestServices.GetRequiredService<IMultiTenantContextSetter>();
        setter.MultiTenantContext = multiTenantContext;

        httpContext.Items[typeof(IMultiTenantContext)] = multiTenantContext;
    }
}