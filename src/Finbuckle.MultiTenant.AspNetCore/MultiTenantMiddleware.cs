// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.AspNetCore.Options;
using Finbuckle.MultiTenant.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Finbuckle.MultiTenant.AspNetCore;

/// <summary>
/// Middleware for resolving the <see cref="IMultiTenantContext"/> and storing it in <see cref="HttpContext"/>.
/// </summary>
public class MultiTenantMiddleware
{
    private readonly RequestDelegate next;
    private readonly ShortCircuitWhenOptions? options;

    /// <summary>
    /// Initializes a new instance of MultiTenantMiddleware.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    public MultiTenantMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    /// <summary>
    /// Initializes a new instance of MultiTenantMiddleware with short-circuit options.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="options">Options for short-circuiting the middleware pipeline.</param>
    public MultiTenantMiddleware(RequestDelegate next, IOptions<ShortCircuitWhenOptions> options)
    {
        this.next = next;
        this.options = options.Value;
    }

    /// <summary>
    /// Invokes the middleware to resolve the tenant and continue the request pipeline.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    public async Task Invoke(HttpContext context)
    {
        if (context.GetEndpoint()?.Metadata.GetMetadata<IExcludeFromMultiTenantResolutionMetadata>() is
            { ExcludeFromResolution: true })
        {
            await next(context);
            return;
        }

        context.RequestServices.GetRequiredService<IMultiTenantContextAccessor>();
        var mtcSetter = context.RequestServices.GetRequiredService<IMultiTenantContextSetter>();

        var resolver = context.RequestServices.GetRequiredService<ITenantResolver>();

        var multiTenantContext = await resolver.ResolveAsync(context).ConfigureAwait(false);
        mtcSetter.MultiTenantContext = multiTenantContext;
        context.Items[typeof(IMultiTenantContext)] = multiTenantContext;

        if (options?.Predicate is null || !options.Predicate(multiTenantContext))
            await next(context);
        else if (options.RedirectTo is not null)
            context.Response.Redirect(options.RedirectTo.ToString());
    }
}