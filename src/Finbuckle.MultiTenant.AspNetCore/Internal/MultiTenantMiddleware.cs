// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Threading.Tasks;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Finbuckle.MultiTenant.AspNetCore;

/// <summary>
/// Middleware for resolving the MultiTenantContext and storing it in HttpContext.
/// </summary>
internal class MultiTenantMiddleware
{
    private readonly RequestDelegate next;

    public MultiTenantMiddleware(RequestDelegate next)
    {
            this.next = next;
        }

    public async Task Invoke(HttpContext context)
    {
            var mtcAccessor = context.RequestServices.GetRequiredService<IMultiTenantContextAccessor>();
            var mtcSetter = context.RequestServices.GetRequiredService<IMultiTenantContextSetter>();
            
            var resolver = context.RequestServices.GetRequiredService<ITenantResolver>();
            
            var multiTenantContext = await resolver.ResolveAsync(context);
            mtcSetter.MultiTenantContext = multiTenantContext;
            context.Items[typeof(IMultiTenantContext)] = multiTenantContext;

            await next(context);
        }
}