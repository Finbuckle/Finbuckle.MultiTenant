// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant.Abstractions;

/// <summary>
/// Contains contextual multi-tenant information.
/// </summary>
/// <typeparam name="TTenantInfo">The <see cref="TenantInfo"/> derived type.</typeparam>
/// <remarks>The <see cref="TenantInfo"/> property can only be set once. If you attempt to set it more than once, a <see cref="MultiTenantException"/> will be thrown.</remarks>
public class TenantContext<TTenantInfo> : ITenantContext<TTenantInfo>
    where TTenantInfo : ITenantInfo
{
    /// <inheritdoc />
    /// <remarks>This property can only be set once. If you attempt to set it more than once, a <see cref="MultiTenantException"/> will be thrown. Setting this property will clear the <see cref="Items"/> dictionary.</remarks>
    public TTenantInfo? TenantInfo
    {
        get;
        set
        {
            if (TenantInfo != null)
                throw new MultiTenantException("TenantInfo is already set. It cannot be set more than once.");
            field = value;
            Items.Clear();
        }
    }

    /// <inheritdoc />
    public IDictionary<object, object> Items { get; } = new Dictionary<object, object>();

    /// <inheritdoc />
    ITenantInfo? ITenantContext.TenantInfo
    {
        get => TenantInfo;
        set => TenantInfo = (TTenantInfo?)value;
    }
}