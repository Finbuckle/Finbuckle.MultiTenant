// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Text.Json.Serialization;
using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant.Test;

/// <summary>
/// A fully immutable, constructor-only <see cref="ITenantInfo{TId}"/> implementation that does not derive from
/// <see cref="TenantInfo{TId}"/> and is not a record. Used to prove the library only depends on the interface
/// contract and never requires settable properties.
/// </summary>
public sealed class ImmutableTenantInfo : ITenantInfo<string>
{
    [JsonConstructor]
    public ImmutableTenantInfo(string id, string identifier, string? name = null)
    {
        Id = id;
        Identifier = identifier;
        Name = name;
    }

    public string Id { get; }
    public string Identifier { get; }
    public string? Name { get; }
}

/// <summary>
/// A plain mutable <see cref="ITenantInfo{TId}"/> implementation with public setters, mirroring the style used by the
/// samples. Used to prove the library tolerates mutable implementations and that its own state cannot be corrupted
/// by mutation of instances it handed out.
/// </summary>
public sealed class MutableTenantInfo : ITenantInfo<string>
{
    public string Id { get; set; } = string.Empty;
    public string Identifier { get; set; } = string.Empty;
    public string? Name { get; set; }
}
