// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.AspNetCore.Options;
using Finbuckle.MultiTenant.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Finbuckle.MultiTenant.AspNetCore;

/// <summary>
/// Middleware for resolving the <see cref="ITenantContext"/> for the request.
/// </summary>
public class MultiTenantMiddleware
{
    private readonly RequestDelegate next;
    private readonly BypassWhenOptions bypassOptions;
    private readonly ShortCircuitWhenOptions shortCircuitWhenOptions;

    /// <summary>
    /// Initializes a new instance of MultiTenantMiddleware.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="bypassOptions">Options for bypassing tenant resolution before it runs.</param>
    /// <param name="shortCircuitWhenOptions">Options for short-circuiting the middleware pipeline after resolution.</param>
    public MultiTenantMiddleware(RequestDelegate next,
        IOptions<BypassWhenOptions> bypassOptions,
        IOptions<ShortCircuitWhenOptions> shortCircuitWhenOptions)
    {
        this.next = next;
        this.bypassOptions = bypassOptions.Value;
        this.shortCircuitWhenOptions = shortCircuitWhenOptions.Value;
    }

    /// <summary>
    /// Invokes the middleware to resolve the tenant and continue the request pipeline.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="tenantContext">The tenant context.</param>
    /// <param name="tenantResolver">The tenant resolver.</param>
    public async Task Invoke(HttpContext context, ITenantContext tenantContext, ITenantResolver tenantResolver)
    {
        if (bypassOptions.Predicate?.Invoke(context) == true)
        {
            await next(context);
            return;
        }

        if (context.GetEndpoint()?.Metadata.GetMetadata<IExcludeFromMultiTenantResolutionMetadata>() is
            { ExcludeFromResolution: true })
        {
            await next(context);
            return;
        }

        var resolvedTenantContext = await tenantResolver.ResolveAsync(context).ConfigureAwait(false);
        tenantContext.TenantInfo = resolvedTenantContext.TenantInfo;

        if (shortCircuitWhenOptions.Predicate is null || !shortCircuitWhenOptions.Predicate(resolvedTenantContext))
            await next(context);
        else if (shortCircuitWhenOptions.RedirectTo is not null)
            context.Response.Redirect(shortCircuitWhenOptions.RedirectTo.ToString());
    }
}