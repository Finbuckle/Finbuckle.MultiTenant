// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;
using System.Linq;
using System.Reflection;

namespace Finbuckle.MultiTenant
{
    public static class TypeExtensions
    {
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

        public static bool HasMultiTenantAttribute(this Type type)
        {
            return type.GetCustomAttribute<MultiTenantAttribute>() != null;
        }
    }
}
