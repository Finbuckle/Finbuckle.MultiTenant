// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant.Abstractions;

/// <summary>
/// Default implementation of <see cref="ITenantInfo{TId}"/>.
/// </summary>
public class TenantInfo<TId> : ITenantInfo<TId> where TId : IEquatable<TId>
{
    /// <inheritdoc />
    public required TId Id { get; init; }

    /// <inheritdoc />
    public required string Identifier { get; init; }
}