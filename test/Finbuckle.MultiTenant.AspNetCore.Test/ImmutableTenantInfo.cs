// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant.AspNetCore.Test;

/// <summary>
/// A constructor-only <see cref="ITenantInfo{TId}"/> implementation (no setters, not derived from
/// <see cref="TenantInfo{TId}"/>, not a record) used to prove the ASP.NET Core integration only depends on the
/// interface contract.
/// </summary>
public sealed class ImmutableTenantInfo(string id, string identifier, string? name = null) : ITenantInfo<string>
{
    public string Id { get; } = id;
    public string Identifier { get; } = identifier;
    public string? Name { get; } = name;
}
