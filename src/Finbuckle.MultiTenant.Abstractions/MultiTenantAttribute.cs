// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant.Abstractions;

/// <summary>
/// Marks a type as multi-tenant.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class MultiTenantAttribute : Attribute
{
}