// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant.Abstractions;

/// <summary>
/// Interface for basic tenant information.
/// </summary>
public interface ITenantInfo
{
    
    /// <summary>
    /// Gets or sets a unique id for the tenant.
    /// </summary>
    /// <remarks>
    /// Unlike the Identifier, the id is never intended to be changed.
    /// </remarks>
    string? Id { get; set; }
    
    /// <summary>
    /// Gets or sets a unique identifier for the tenant.
    /// </summary>
    /// <remarks>
    /// The Identifier is intended for use during tenant resolution and format is determined by convention. For example
    /// a web based strategy may require URL friendly identifiers. Identifiers can be changed if needed.
    /// </remarks>
    string? Identifier { get; set;  }
    
    /// <summary>
    /// Gets or sets a display friendly name for the tenant.
    /// </summary>
    string? Name { get; set; }
}