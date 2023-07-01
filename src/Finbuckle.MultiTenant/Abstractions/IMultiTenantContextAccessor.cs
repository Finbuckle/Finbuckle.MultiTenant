// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

// ReSharper disable once CheckNamespace

namespace Finbuckle.MultiTenant;

/// <summary>
///     Provides access the current MultiTenantContext.
/// </summary>
public interface IMultiTenantContextAccessor
{
    /// <summary>
    ///     Gets or sets the current MultiTenantContext.
    /// </summary>
    IMultiTenantContext? MultiTenantContext { get; set; }
}

/// <summary>
///     Provides access the current MultiTenantContext.
/// </summary>
/// <typeparam name="T">The ITenantInfo implementation type.</typeparam>
public interface IMultiTenantContextAccessor<T> where T : class, ITenantInfo, new()
{
    /// <summary>
    ///     Gets or sets the current MultiTenantContext.
    /// </summary>
    IMultiTenantContext<T>? MultiTenantContext { get; set; }
}