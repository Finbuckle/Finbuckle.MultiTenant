// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant.Abstractions;

/// <summary>
/// Contains contextual multi-tenant information.
/// </summary>
/// <typeparam name="TTenantInfo">The <see cref="TenantInfo"/> derived type.</typeparam>
/// <typeparam name="TId">The ID implementation type.</typeparam>
/// <remarks>The <see cref="TenantInfo"/> property can only be set once. If you attempt to set it more than once, a <see cref="MultiTenantException"/> will be thrown.</remarks>
internal class InternalTenantContext<TTenantInfo, TId> : ITenantContext<TTenantInfo, TId>
    where TTenantInfo : ITenantInfo<TId> where TId : IEquatable<TId>
{
    /// <inheritdoc />
    /// <remarks>This property can only be set once. If you attempt to set it more than once, a <see cref="MultiTenantException"/> will be thrown.</remarks>
    public TTenantInfo? TenantInfo
    {
        get;
        set
        {
            // Ensure that TenantInfo is only set once.
            if (field != null)
                throw new MultiTenantException("TenantInfo is already set. It cannot be set more than once.");

            field = value;
        }
    }

    /// <inheritdoc />
    public bool IsResolved => TenantInfo != null;

    /// <inheritdoc />
    ITenantInfo<TId>? ITenantContext<TId>.TenantInfo
    {
        get => TenantInfo;
        set => TenantInfo = (TTenantInfo?)value;
    }

}
