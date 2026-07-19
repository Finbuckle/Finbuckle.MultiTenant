// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Reflection;

namespace Finbuckle.MultiTenant.Abstractions.Extensions;

/// <summary>
/// Extension methods for Type operations.
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    /// Determines whether the specified type has the MultiTenant attribute.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type has the MultiTenant attribute; otherwise, false.</returns>
    public static bool HasMultiTenantAttribute(this Type type)
    {
        return type.GetCustomAttribute<MultiTenantAttribute>() != null;
    }
}
