// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant;

/// <summary>
/// Options for multi-tenant resolution.
/// </summary>
/// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
public class MultiTenantOptions<TTenantInfo> where TTenantInfo : ITenantInfo
{
    /// <summary>
    /// Gets or sets the type of <see cref="TenantInfo"/> derived.
    /// </summary>
    public required Type TenantInfoType { get; set; }

    /// <summary>
    /// Gets or sets the list of identifiers that should be ignored during tenant resolution.
    /// </summary>
    public IList<string> IgnoredIdentifiers { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the event handlers for tenant resolution.
    /// </summary>
    public MultiTenantEvents<TTenantInfo> Events { get; set; } = new();
}