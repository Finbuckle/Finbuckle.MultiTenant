// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant.Abstractions;

/// <summary>
/// Provides access to the current <see cref="IMultiTenantContext"/>.
/// </summary>
public interface IMultiTenantContextAccessor
{
    /// <summary>
    /// Gets the current <see cref="IMultiTenantContext"/>.
    /// </summary>
    IMultiTenantContext MultiTenantContext { get; }
}

/// <summary>
/// Provides access to the current <see cref="IMultiTenantContext{TTenantInfo}"/>.
/// </summary>
/// <typeparam name="TTenantInfo">The <see cref="TenantInfo"/> derived type.</typeparam>
public interface IMultiTenantContextAccessor<TTenantInfo> : IMultiTenantContextAccessor
    where TTenantInfo : TenantInfo
{
    /// <summary>
    /// Gets the current <see cref="IMultiTenantContext{TTenantInfo}"/>.
    /// </summary>
    new IMultiTenantContext<TTenantInfo> MultiTenantContext { get; }
}