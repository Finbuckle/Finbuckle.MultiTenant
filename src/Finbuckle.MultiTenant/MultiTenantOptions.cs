// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant;

/// <summary>
/// Options for multi-tenant resolution.
/// </summary>
/// <typeparam name="TTenantInfo">The <see cref="ITenantInfo{TId}"/> implementation type.</typeparam>
/// <typeparam name="TId">The ID implementation type.</typeparam>
public class MultiTenantOptions<TTenantInfo, TId> where TTenantInfo : ITenantInfo<TId> where TId : IEquatable<TId>
{
    /// <summary>
    /// Gets or sets the type of <see cref="TenantInfo{TId}"/> derived.
    /// </summary>
    public required Type TenantInfoType { get; set; }

    /// <summary>
    /// Gets or sets the list of identifiers that should be ignored during tenant resolution.
    /// </summary>
    public IList<string> IgnoredIdentifiers { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the event handlers for tenant resolution.
    /// </summary>
    public MultiTenantEvents<TTenantInfo, TId> Events { get; set; } = new();
}