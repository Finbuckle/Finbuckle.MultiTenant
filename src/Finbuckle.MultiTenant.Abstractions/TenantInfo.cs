// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant.Abstractions;

/// <summary>
/// Default implementation of <see cref="ITenantInfo"/>.
/// </summary>
public class TenantInfo : ITenantInfo
{
    /// <inheritdoc />
    public required string Id { get; init; }

    /// <inheritdoc />
    public required string Identifier { get; init; }
}