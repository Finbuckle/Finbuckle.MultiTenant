// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

// TODO move to options folder/namespace on future major release

using Finbuckle.MultiTenant.Events;

namespace Finbuckle.MultiTenant;

/// <summary>
/// Represents the options for a multi-tenant application.
/// </summary>
public class MultiTenantOptions
{
    /// <summary>
    /// Gets or sets the type of the tenant information.
    /// </summary>
    public Type? TenantInfoType { get; internal set; }

    /// <summary>
    /// Gets or sets the list of identifiers that should be ignored.
    /// </summary>
    public IList<string> IgnoredIdentifiers { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the events that can be used to customize the multi-tenant application behavior.
    /// </summary>
    public MultiTenantEvents Events { get; set; } = new();
}