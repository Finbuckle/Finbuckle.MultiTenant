// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Finbuckle.MultiTenant.AspNetCore.Strategies;

/// <summary>
/// A strategy that determines the tenant identifier from the session state.
/// </summary>
public class SessionStrategy : IMultiTenantStrategy
{
    private readonly string tenantKey;

    /// <summary>
    /// Initializes a new instance of SessionStrategy.
    /// </summary>
    /// <param name="tenantKey">The session key containing the tenant identifier.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="tenantKey"/> is null or whitespace.</exception>
    public SessionStrategy(string tenantKey)
    {
        if (string.IsNullOrWhiteSpace(tenantKey))
        {
            throw new ArgumentException("Tenant key must not be null or whitespace.", nameof(tenantKey));
        }

        this.tenantKey = tenantKey;
    }

    /// <inheritdoc />
    public Task<string?> GetIdentifierAsync(object context)
    {
        if (context is not HttpContext httpContext)
            return Task.FromResult<string?>(null);

        var identifier = httpContext.Session.GetString(tenantKey);
        return Task.FromResult(identifier);
    }
}