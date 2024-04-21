// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant.Events;

/// <summary>
/// Context for when a tenant is not resolved.
/// </summary>
public class TenantNotResolvedContext
{
    /// <summary>
    /// Gets or sets the context used for attempted tenant resolution.
    /// </summary>
    public object? Context { get; set; }
    
    
    /// <summary>
    /// Gets or sets the last identifier used for attempted tenant resolution.
    /// </summary>
    public string? Identifier { get; set; }
}