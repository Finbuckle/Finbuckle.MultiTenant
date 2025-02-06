// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Threading.Tasks;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.AspNetCore.Options;
using Finbuckle.MultiTenant.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Finbuckle.MultiTenant.AspNetCore.Internal;

/// <summary>
/// Middleware for resolving the MultiTenantContext and storing it in HttpContext.
/// </summary>
public class MultiTenantMiddleware
{
    private readonly RequestDelegate next;
    private readonly ShortCircuitWhenOptions? options;

    public MultiTenantMiddleware(RequestDelegate next)
    {
            this.next = next;
        }

    public MultiTenantMiddleware(RequestDelegate next, IOptions<ShortCircuitWhenOptions> options)
    {
            this.next = next;
            this.options = options.Value;
        }

    public async Task Invoke(HttpContext context)
    {
            if (context.GetEndpoint()?.Metadata.GetMetadata<IExcludeFromMultiTenantResolutionMetadata>() is { ExcludeFromResolution: true })
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
        }
}