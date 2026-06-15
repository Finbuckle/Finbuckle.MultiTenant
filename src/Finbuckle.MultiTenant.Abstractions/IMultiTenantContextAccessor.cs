// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant.Abstractions;

/// <summary>
/// Provides access to the current <see cref="ITenantContext"/>.
/// </summary>
public interface IMultiTenantContextAccessor
{
    /// <summary>
    /// Gets the current <see cref="ITenantContext"/>.
    /// </summary>
    ITenantContext MultiTenantContext { get; }
}

/// <summary>
/// Provides access to the current <see cref="ITenantContext{TTenantInfo}"/>.
/// </summary>
/// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
public interface IMultiTenantContextAccessor<TTenantInfo> : IMultiTenantContextAccessor
    where TTenantInfo : ITenantInfo
{
    /// <summary>
    /// Gets the current <see cref="ITenantContext{TTenantInfo}"/>.
    /// </summary>
    new ITenantContext<TTenantInfo> MultiTenantContext { get; }
}