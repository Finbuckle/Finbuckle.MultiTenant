// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Finbuckle.MultiTenant.AspNetCore;

/// <summary>
/// An <see cref="IHttpContextFactory"/> decorator that begins a new ambient tenant scope when an
/// <see cref="HttpContext"/> is created and ends it when the context is disposed.
/// </summary>
/// <remarks>
/// The ASP.NET Core host creates the <see cref="HttpContext"/> on the same asynchronous execution context that runs
/// the request pipeline, so the scope begun here is in effect for every middleware, endpoint, and service in the
/// request, including middleware that runs before tenant resolution.
/// </remarks>
public class TenantScopeHttpContextFactory : IHttpContextFactory
{
    private readonly IHttpContextFactory inner;
    private readonly ITenantScopeProvider tenantScopeProvider;

    /// <summary>
    /// Initializes a new instance of <see cref="TenantScopeHttpContextFactory"/>.
    /// </summary>
    /// <param name="inner">The <see cref="IHttpContextFactory"/> being decorated.</param>
    /// <param name="tenantScopeProvider">The provider used to begin and end the ambient tenant scope.</param>
    public TenantScopeHttpContextFactory(IHttpContextFactory inner, ITenantScopeProvider tenantScopeProvider)
    {
        ArgumentNullException.ThrowIfNull(inner);
        ArgumentNullException.ThrowIfNull(tenantScopeProvider);

        this.inner = inner;
        this.tenantScopeProvider = tenantScopeProvider;
    }

    /// <inheritdoc />
    public HttpContext Create(IFeatureCollection featureCollection)
    {
        tenantScopeProvider.BeginScope();
        return inner.Create(featureCollection);
    }

    /// <inheritdoc />
    public void Dispose(HttpContext httpContext)
    {
        try
        {
            inner.Dispose(httpContext);
        }
        finally
        {
            tenantScopeProvider.EndScope();
        }
    }
}
