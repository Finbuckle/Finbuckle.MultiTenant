// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant.Events;

/// <summary>
/// Context for when a MultiTenantStrategy has run.
/// </summary>
public class StrategyResolveCompletedContext
{
    /// <summary>
    /// Gets or sets the context used for attempted tenant resolution.
    /// </summary>
    public object? Context { get; set; }
    
    /// <summary>
    /// The MultiTenantStrategy instance that was run.
    /// </summary>
    public required IMultiTenantStrategy Strategy { get; init; }
    
    /// <summary>
    /// Gets or sets the identifier found by the strategy. Setting to null will cause the next strategy to run.
    /// </summary>
    public string? Identifier { get; set; }
    
    /// <summary>
    /// Returns true if a tenant identifier was found.
    /// </summary>
    public bool IdentifierFound => Identifier != null;
}