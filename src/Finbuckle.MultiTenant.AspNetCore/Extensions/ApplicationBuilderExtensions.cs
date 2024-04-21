﻿// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.AspNetCore.Internal;
using Microsoft.AspNetCore.Builder;

//ReSharper disable once CheckNamespace
namespace Finbuckle.MultiTenant;

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