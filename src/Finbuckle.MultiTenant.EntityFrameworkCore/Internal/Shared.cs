//    Copyright 2018 Andrew White
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Finbuckle.MultiTenant.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Finbuckle.MultiTenant.EntityFrameworkCore
{
    /// <summary>
    /// A static class containing static methods shared between
    /// MultiTenantDbContext and MultiTenantIdentityDbContext.
    /// </summary>
    public static class Shared
    {
        internal static LambdaExpression GetQueryFilter(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder r)
        {
            #if NETSTANDARD2_1
                return r.Metadata.GetQueryFilter();
            #elif NETSTANDARD2_0
                return r.Metadata.QueryFilter;
            #else
                #error No valid path!
            #endif
        }

        public static bool HasMultiTenantAttribute(Type t)
        {
            return t.GetCustomAttribute<MultiTenantAttribute>() != null;
        }

        public static bool HasMultiTenantAnnotation(IEntityType t)
        {
            return (bool?)t.FindAnnotation(Constants.MultiTenantAnnotation)?.Value ?? false;
        }
    }
}
