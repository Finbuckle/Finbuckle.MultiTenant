// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant.Abstractions;

/// <summary>
/// Contains constant values for internal use.
/// </summary>
// TODO rethink
internal static class Constants
{
    /// <summary>
    /// Maximum length for tenant identifiers.
    /// </summary>
    public static readonly int TenantIdMaxLength = 64;
    
    /// <summary>
    /// The token placeholder for tenant in templates.
    /// </summary>
    public static readonly string TenantToken = "__tenant__";
}