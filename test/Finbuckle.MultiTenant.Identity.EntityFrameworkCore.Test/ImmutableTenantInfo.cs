// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant.Identity.EntityFrameworkCore.Test;

/// <summary>
/// A constructor-only <see cref="ITenantInfo{TId}"/> implementation (no setters, not derived from
/// <see cref="TenantInfo{TId}"/>, not a record) used to prove the Identity integration only depends on the
/// interface contract.
/// </summary>
public sealed class ImmutableTenantInfo(string id, string identifier) : ITenantInfo<string>
{
    public string Id { get; } = id;
    public string Identifier { get; } = identifier;
}
