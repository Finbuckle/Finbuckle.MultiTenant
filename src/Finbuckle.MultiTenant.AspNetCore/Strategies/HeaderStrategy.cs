// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Diagnostics.CodeAnalysis;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Finbuckle.MultiTenant.AspNetCore.Strategies;

/// <summary>
/// A strategy that determines the tenant identifier from a request header.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class HeaderStrategy : IMultiTenantStrategy
{
    private readonly string _headerKey;

    /// <summary>
    /// Initializes a new instance of HeaderStrategy.
    /// </summary>
    /// <param name="headerKey">The name of the header containing the tenant identifier.</param>
    public HeaderStrategy(string headerKey)
    {
        _headerKey = headerKey;
    }

    /// <inheritdoc />
    public Task<string?> GetIdentifierAsync(object context)
    {
        if (context is not HttpContext httpContext)
            return Task.FromResult<string?>(null);

        return Task.FromResult(httpContext?.Request.Headers[_headerKey].FirstOrDefault());
    }
}