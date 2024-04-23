// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Internal;

namespace Finbuckle.MultiTenant;

/// <summary>
/// The TenantInfo class implements the ITenantInfo interface and represents a basic tenant in a multi-tenant application.
/// </summary>
public class TenantInfo : ITenantInfo
{
    private string? id;

    /// <summary>
    /// Initializes a new instance of the TenantInfo class.
    /// </summary>
    public TenantInfo()
    {
    }

    /// <summary>
    /// Gets or sets the ID of the tenant. The setter validates the length of the ID.
    /// </summary>
    /// <exception cref="MultiTenantException">Thrown when the ID exceeds the maximum length.</exception>
    public string? Id
    {
        get
        {
            return id;
        }
        set
        {
            if (value != null)
            {
                if (value.Length > Constants.TenantIdMaxLength)
                {
                    throw new MultiTenantException($"The tenant id cannot exceed {Constants.TenantIdMaxLength} characters.");
                }
                id = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets the identifier of the tenant.
    /// </summary>
    public string? Identifier { get; set; }

    /// <summary>
    /// Gets or sets the name of the tenant.
    /// </summary>
    public string? Name { get; set; }
}