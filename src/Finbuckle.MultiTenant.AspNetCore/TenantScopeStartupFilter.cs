// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Finbuckle.MultiTenant.AspNetCore;

/// <summary>
/// An <see cref="IStartupFilter"/> that adds a middleware at the very start of the request pipeline to begin a new
/// ambient tenant scope for each request.
/// </summary>
/// <remarks>
/// The scope is begun inside an async middleware, so it flows to every later middleware, endpoint, and service in the
/// request and is discarded automatically when the middleware returns. Unlike decorating
/// <c>IHttpContextFactory</c>, this keeps ASP.NET Core's per-connection <c>HttpContext</c> reuse.
/// </remarks>
internal sealed class TenantScopeStartupFilter(ITenantScopeProvider tenantScopeProvider) : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
    {
        app.Use(async (context, nextMiddleware) =>
        {
            // Changes to the ambient scope made in this async lambda are reverted for the caller when it returns, so
            // no EndScope call is needed.
            tenantScopeProvider.BeginScope();
            await nextMiddleware(context);
        });

        next(app);
    };
}
