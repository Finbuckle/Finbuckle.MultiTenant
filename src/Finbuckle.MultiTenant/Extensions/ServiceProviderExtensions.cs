// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Finbuckle.MultiTenant.Extensions;

/// <summary>
/// <see cref="IServiceProvider"/> extension methods for Finbuckle.MultiTenant.
/// </summary>
public static class ServiceProviderExtensions
{
    /// <summary>
    /// Begins a new ambient tenant scope for the current asynchronous execution context.
    /// </summary>
    /// <param name="services">The <see cref="IServiceProvider"/> instance the extension method applies to.</param>
    public static void BeginTenantScope(this IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.GetRequiredService<ITenantScopeProvider>().BeginScope();
    }

    /// <summary>
    /// Begins a new ambient tenant scope for the current asynchronous execution context and sets its tenant information.
    /// </summary>
    /// <param name="services">The <see cref="IServiceProvider"/> instance the extension method applies to.</param>
    /// <param name="tenantInfo">The tenant information for the new scope.</param>
    public static void BeginTenantScope(this IServiceProvider services, ITenantInfo tenantInfo)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(tenantInfo);

        var tenantScopeProvider = services.GetRequiredService<ITenantScopeProvider>();
        tenantScopeProvider.BeginScope();
        var tenantContext = services.GetRequiredService<ITenantContext>();
        tenantContext.TenantInfo = tenantInfo;
    }
}
