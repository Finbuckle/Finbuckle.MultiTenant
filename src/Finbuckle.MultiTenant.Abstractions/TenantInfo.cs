// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant;

/// <summary>
/// Default implementation of ITenantInfo.
/// </summary>
public class TenantInfo : ITenantInfo
{
    /// <summary>
    /// Initializes a new instance of TenantInfo.
    /// </summary>
    public TenantInfo()
    {
    }

    /// <inheritdoc />
    public string? Id { get; set; }

    /// <inheritdoc />
    public string? Identifier { get; set; }
    
    /// <inheritdoc />
    public string? Name { get; set; }
}