// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant;

/// <summary>
/// Marks a class as multitenant. Currently only used in EFCore support but included here to reduce dependencies where
/// this might be needed.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class MultiTenantAttribute : Attribute
{

}