using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Finbuckle.MultiTenant.EntityFrameworkCore
{
    static class TypeExtensions
    {
        public static bool ImplementsOrInheritsUnboundGeneric(this Type source, Type unboundGeneric)
        {
            if (unboundGeneric.IsInterface)
            {
                return source.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == unboundGeneric);
            }

            Type toCheck = source;

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
    }
}
