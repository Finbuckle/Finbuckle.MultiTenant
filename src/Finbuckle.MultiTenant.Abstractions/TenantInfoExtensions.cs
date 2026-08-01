// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant.Abstractions;

/// <summary>
/// Extension methods for <see cref="ITenantInfo{TId}"/>.
/// </summary>
public static class TenantInfoExtensions
{
    /// <summary>
    /// Throws if the tenant is null, has a default (unset) <see cref="ITenantInfo{TId}.Id"/>, or a
    /// missing <see cref="ITenantInfo{TId}.Identifier"/>. A tenant id equal to <c>default(TId)</c>
    /// (for example <c>0</c>, <see cref="System.Guid.Empty"/>, or a null/whitespace string) is treated as unset.
    /// </summary>
    /// <param name="tenantInfo">The tenant information to validate.</param>
    /// <typeparam name="TId">The tenant id type.</typeparam>
    /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="tenantInfo"/> is null.</exception>
    /// <exception cref="MultiTenantException">Thrown when the id is unset or the identifier is missing.</exception>
    public static void EnsureValid<TId>(this ITenantInfo<TId>? tenantInfo) where TId : IEquatable<TId>
    {
        ArgumentNullException.ThrowIfNull(tenantInfo);

        if (EqualityComparer<TId>.Default.Equals(tenantInfo.Id, default!) ||
            tenantInfo.Id is string id && string.IsNullOrWhiteSpace(id))
            throw new MultiTenantException("Tenant Id is missing or the default value.");

        if (string.IsNullOrWhiteSpace(tenantInfo.Identifier))
            throw new MultiTenantException("Tenant Identifier is missing.");
    }
}
