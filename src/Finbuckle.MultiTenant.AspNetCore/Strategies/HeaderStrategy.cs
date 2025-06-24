// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Diagnostics.CodeAnalysis;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Finbuckle.MultiTenant.AspNetCore.Strategies;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class HeaderStrategy : IMultiTenantStrategy
{
    private readonly string _headerKey;
    public HeaderStrategy(string headerKey)
    {
            _headerKey = headerKey;
        }

    public Task<string?> GetIdentifierAsync(object context)
    {
            if (context is not HttpContext httpContext)
                return Task.FromResult<string?>(null);

            return Task.FromResult(httpContext?.Request.Headers[_headerKey].FirstOrDefault());
        }
}