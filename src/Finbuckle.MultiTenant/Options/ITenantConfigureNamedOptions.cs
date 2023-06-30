// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

namespace Finbuckle.MultiTenant.Options;

/// <summary>
/// Configures options per-tenant.
/// </summary>
/// <typeparam name="TOptions">Options type being configured.</typeparam>
/// <typeparam name="T">A type implementing ITenantInfo.</typeparam>
public interface ITenantConfigureNamedOptions<TOptions, TTenantInfo>
    where TOptions : class, new()
    where TTenantInfo : class, ITenantInfo, new()
{
    /// <summary>
    /// Invoked to configure per-tenant options.
    /// </summary>
    /// <param name="name">The name of the option to be configured.</param>
    /// <param name="options">The options class instance to be configured.</param>
    /// <typeparam name="T">A type implementing ITenantInfo.</typeparam>
    void Configure(string name, TOptions options, TTenantInfo tenantInfo);
}