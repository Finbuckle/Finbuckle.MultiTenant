// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant.Abstractions;

/// <summary>
/// Marks a multi-tenant type whose entities can be made visible to every tenant while remaining
/// owned by the tenant that created them.
/// </summary>
/// <remarks>
/// The type must also carry <see cref="MultiTenantAttribute"/> and declare a boolean IsShared property.
/// An entity with IsShared set is readable from any tenant, but its TenantId still designates its owner,
/// so writes from another tenant are rejected according to the context TenantMismatchMode.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class MultiTenantShareableAttribute : Attribute
{
}
