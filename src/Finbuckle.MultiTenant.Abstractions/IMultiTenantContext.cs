// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant.Abstractions;

/// <summary>
/// Non-generic interface for the <see cref="MultiTenantContext{TTenantInfo}"/>.
/// </summary>
public interface IMultiTenantContext
{
    /// <summary>
    /// Information about the tenant for this context.
    /// </summary>
    ITenantInfo? TenantInfo { get; init; }

    /// <summary>
    /// True if a tenant has been resolved and <see cref="ITenantInfo"/> is not null.
    /// </summary>
    bool IsResolved { get; }

    /// <summary>
    /// Information about the <see cref="IMultiTenantStrategy"/> for this context.
    /// </summary>
    StrategyInfo? StrategyInfo { get; init; }
}

/// <summary>
/// Generic interface for the multi-tenant context.
/// </summary>
/// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
public interface IMultiTenantContext<TTenantInfo> : IMultiTenantContext
    where TTenantInfo : ITenantInfo
{
    /// <summary>
    /// Information about the tenant for this context.
    /// </summary>
    new TTenantInfo? TenantInfo { get; }

    /// <summary>
    /// Information about the <see cref="IMultiTenantStore{ITenantInfo}"/> for this context.
    /// </summary>
    StoreInfo<TTenantInfo>? StoreInfo { get; }
}