// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.AspNetCore.Options;
using Finbuckle.MultiTenant.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Finbuckle.MultiTenant.AspNetCore;

/// <summary>
/// Middleware for resolving the <see cref="ITenantContext{TId}"/> for the request.
/// </summary>
public class MultiTenantMiddleware<TId> where TId : IEquatable<TId>
{
    private readonly RequestDelegate next;
    private readonly BypassWhenOptions bypassOptions;
    private readonly ShortCircuitWhenOptions<TId> shortCircuitWhenOptions;

    /// <summary>
    /// Initializes a new instance of MultiTenantMiddleware.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="bypassOptions">Options for bypassing tenant resolution before it runs.</param>
    /// <param name="shortCircuitWhenOptions">Options for short-circuiting the middleware pipeline after resolution.</param>
    public MultiTenantMiddleware(RequestDelegate next,
        IOptions<BypassWhenOptions> bypassOptions,
        IOptions<ShortCircuitWhenOptions<TId>> shortCircuitWhenOptions)
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
    /// <param name="tenantScopeProvider">The provider used to verify that an ambient tenant scope is active.</param>
    /// <exception cref="MultiTenantException">Thrown when no ambient tenant scope is active for the request.</exception>
    /// <remarks>
    /// The ambient tenant scope is begun when the <see cref="HttpContext"/> is created, which is configured by
    /// <c>AddHttpAmbientTenantScope</c>. If a tenant was already set earlier in the pipeline, tenant resolution is
    /// skipped and that tenant is kept.
    /// </remarks>
    public async Task Invoke(HttpContext context, ITenantContext<TId> tenantContext, ITenantResolver<TId> tenantResolver, ITenantScopeProvider tenantScopeProvider)
    {
        if (!tenantScopeProvider.HasScope)
            throw new MultiTenantException(HttpAmbientTenantScopeMarker.NoScopeMessage);

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

        if (!tenantContext.IsResolved)
        {
            var resolvedTenantInfo = await tenantResolver.ResolveAsync(context).ConfigureAwait(false);
            tenantContext.TenantInfo = resolvedTenantInfo;
        }

        if (shortCircuitWhenOptions.Predicate is null || !shortCircuitWhenOptions.Predicate(tenantContext))
            await next(context);
        else if (shortCircuitWhenOptions.RedirectTo is not null)
            context.Response.Redirect(shortCircuitWhenOptions.RedirectTo.ToString());
    }
}
