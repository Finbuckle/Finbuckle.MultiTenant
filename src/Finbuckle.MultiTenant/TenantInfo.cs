// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Internal;

namespace Finbuckle.MultiTenant;

/// <summary>
/// Default implementation of ITenantInfo.
/// </summary>
public class TenantInfo : ITenantInfo
{
    private string? id;

    /// <summary>
    /// Initializes a new instance of TenantInfo.
    /// </summary>
    public TenantInfo()
    {
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public string? Identifier { get; set; }
    
    /// <inheritdoc />
    public string? Name { get; set; }
}