// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Finbuckle.MultiTenant.AspNetCore.Strategies;

public class SessionStrategy : IMultiTenantStrategy
{
    private readonly string tenantKey;

    public SessionStrategy(string tenantKey)
    {
        if (string.IsNullOrWhiteSpace(tenantKey))
        {
            throw new ArgumentException("message", nameof(tenantKey));
        }

        this.tenantKey = tenantKey;
    }

    public Task<string?> GetIdentifierAsync(object context)
    {
        if (context is not HttpContext httpContext)
            return Task.FromResult<string?>(null);

        var identifier = httpContext.Session.GetString(tenantKey);
        return Task.FromResult(identifier);
    }
}