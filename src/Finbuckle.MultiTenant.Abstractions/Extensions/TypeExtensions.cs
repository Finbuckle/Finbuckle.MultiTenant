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
    /// Determines whether the source type implements or inherits from an unbound generic type.
    /// </summary>
    /// <param name="source">The source type to check.</param>
    /// <param name="unboundGeneric">The unbound generic type to check against.</param>
    /// <returns>True if the source type implements or inherits from the unbound generic type, otherwise false.</returns>
    public static bool ImplementsOrInheritsUnboundGeneric(this Type source, Type unboundGeneric)
    {
        if (unboundGeneric.IsInterface)
        {
            return source.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == unboundGeneric);
        }

        Type? toCheck = source;

        if (unboundGeneric != toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var current = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;

                if (unboundGeneric == current)
                {
                    return true;
                }

                toCheck = toCheck.BaseType;
            }
        }

        return false;
    }

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