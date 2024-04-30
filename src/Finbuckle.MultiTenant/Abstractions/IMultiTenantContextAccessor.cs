// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant.Abstractions;

/// <summary>
/// Provides access the current MultiTenantContext.
/// </summary>
public interface IMultiTenantContextAccessor
{
    /// <summary>
    /// Gets the current MultiTenantContext.
    /// </summary>
    IMultiTenantContext MultiTenantContext { get; }
}

/// <summary>
/// Provides access the current MultiTenantContext.
/// </summary>
/// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
public interface IMultiTenantContextAccessor<TTenantInfo> : IMultiTenantContextAccessor
    where TTenantInfo : class, ITenantInfo, new()
{
    /// <summary>
    /// Gets the current MultiTenantContext.
    /// </summary>
    new IMultiTenantContext<TTenantInfo> MultiTenantContext { get; }
}