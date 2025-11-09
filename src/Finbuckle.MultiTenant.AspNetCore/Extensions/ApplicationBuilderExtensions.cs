// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Microsoft.AspNetCore.Builder;

namespace Finbuckle.MultiTenant.AspNetCore.Extensions;

/// <summary>
/// Extension methods for using Finbuckle.MultiTenant.AspNetCore.
/// </summary>
public static class FinbuckleMultiTenantApplicationBuilderExtensions
{
    /// <summary>
    /// Use Finbuckle.MultiTenant middleware in processing the request.
    /// </summary>
    /// <param name="builder">The <c>IApplicationBuilder</c> instance the extension method applies to.</param>
    /// <returns>The same IApplicationBuilder passed into the method.</returns>
    public static IApplicationBuilder UseMultiTenant(this IApplicationBuilder builder)
        => builder.UseMiddleware<MultiTenantMiddleware>();
}