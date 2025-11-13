// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant.Abstractions;

/// <summary>
/// Default implementation of TenantInfo.
/// </summary>
/// <param name="Id">A unique identifier for the tenant. Typically used as the primary key.</param>
/// <param name="Identifier">A externally-facing identifier used for tenant resolution.</param>
/// <param name="Name">A friendly name for the tenant.</param>
public record TenantInfo(string Id, string Identifier, string? Name = null)
{
}