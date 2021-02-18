// Copyright 2018-2020 Finbuckle LLC, Andrew White, and Contributors
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Linq;
using System.Linq.Expressions;

#if NETSTANDARD2_0
using Microsoft.EntityFrameworkCore.Metadata;
using System.Collections.Generic;
using Remotion.Linq.Parsing.ExpressionVisitors;
#endif

namespace Finbuckle.MultiTenant.EntityFrameworkCore
{
    public static class FinbuckleEntityTypeBuilderExtensions
    {
        private class ExpressionVariableScope
        {
            public IMultiTenantDbContext Context { get; }
        }

        internal static LambdaExpression GetQueryFilter(this EntityTypeBuilder builder)
        {         
#if NETSTANDARD2_0
            return builder.Metadata.QueryFilter;
#else
            return builder.Metadata.GetQueryFilter();
#endif
        }

        /// <summary>
        /// Adds MultiTenant support for an entity. Call <see cref="IsMultiTenant" /> after 
        /// <see cref="EntityTypeBuilder.HasQueryFilter" /> to merge query filters.
        /// </summary>
        /// <typeparam name="T">The specific type of <see cref="EntityTypeBuilder"/></typeparam>
        /// <param name="builder">The entity's type builder</param>
        /// <returns>The original type builder reference for chaining</returns>
        public static EntityTypeBuilder<T> IsMultiTenant<T>(this EntityTypeBuilder<T> builder) where T : class
        {
            if(builder.Metadata.FindAnnotation(Constants.MultiTenantAnnotationName) != null)
                return builder;

            builder.HasAnnotation(Constants.MultiTenantAnnotationName, true);

            try
            {
                builder.Property<string>("TenantId").IsRequired().HasMaxLength(Finbuckle.MultiTenant.Internal.Constants.TenantIdMaxLength);
            }
            catch (Exception ex)
            {
                throw new MultiTenantException($"{builder.Metadata.ClrType} unable to add TenantId property", ex);
            }

            // build expression tree for e => EF.Property<string>(e, "TenantId") == TenantInfo.Id
            Expression<Func<T, bool>> tenantFilter = e => EF.Property<string>(e, "TenantId") == (new ExpressionVariableScope()).Context.TenantInfo.Id;
            
            // combine withany existing query filter if it exists
            var existingQueryFilter = builder.GetQueryFilter();

            if(existingQueryFilter != null)
            {
                // replace the parameter node in the tenant filter with the one in the existing filter
                var filterParam = existingQueryFilter.Parameters.Single();
                var adjustedTenantFilterBody = ReplacingExpressionVisitor.Replace(tenantFilter.Parameters.Single(),
                                                                              filterParam,
                                                                              tenantFilter.Body);

                var combinedExp = Expression.AndAlso(existingQueryFilter.Body, adjustedTenantFilterBody);
                tenantFilter = (Expression<Func<T, bool>>)Expression.Lambda(combinedExp, filterParam);
            }
            
            builder.HasQueryFilter(tenantFilter);

            Type clrType = builder.Metadata.ClrType;

            if (clrType != null)
            {
                if (clrType.ImplementsOrInheritsUnboundGeneric(typeof(IdentityUser<>)))
                {
                    UpdateIdentityUserIndex(builder);
                }

                if (clrType.ImplementsOrInheritsUnboundGeneric(typeof(IdentityRole<>)))
                {
                    UpdateIdentityRoleIndex(builder);
                }

                if (clrType.ImplementsOrInheritsUnboundGeneric(typeof(IdentityUserLogin<>)))
                {
                    UpdateIdentityUserLoginPrimaryKey(builder);
                    AddIdentityUserLoginIndex(builder);
                }
            }

            return builder;
        }        

        private static void UpdateIdentityUserIndex(this EntityTypeBuilder builder)
        {
            builder.RemoveIndex("NormalizedUserName");
#if NET // Covers .NET 5.0 and later.
            builder.HasIndex("NormalizedUserName", "TenantId").HasDatabaseName("UserNameIndex").IsUnique();
#else   // .NET Core 2.1 and 3.1
            builder.HasIndex("NormalizedUserName", "TenantId").HasName("UserNameIndex").IsUnique();
#endif
        }

        private static void UpdateIdentityRoleIndex(this EntityTypeBuilder builder)
        {
            builder.RemoveIndex("NormalizedName");
#if NET // Covers .NET 5.0 and later.
            builder.HasIndex("NormalizedName", "TenantId").HasDatabaseName("RoleNameIndex").IsUnique();
#else // .NET Core 2.1 and 3.1
            builder.HasIndex("NormalizedName", "TenantId").HasName("RoleNameIndex").IsUnique();
#endif
        }

        private static void UpdateIdentityUserLoginPrimaryKey(this EntityTypeBuilder builder)
        {
            var pk = builder.Metadata.FindPrimaryKey();
            builder.Metadata.RemoveKey(pk.Properties);

            // Create a new ID and a unique index to replace the old pk.
            builder.Property<string>("Id").ValueGeneratedOnAdd();
        }

        private static void AddIdentityUserLoginIndex(this EntityTypeBuilder builder) 
        { 
            builder.HasIndex("LoginProvider", "ProviderKey", "TenantId").IsUnique();
        }

        private static void RemoveIndex(this EntityTypeBuilder builder, string propName)
        {        
#if NETSTANDARD2_0
            var props = new List<IProperty>(new[] { builder.Metadata.FindProperty(propName) });
            builder.Metadata.RemoveIndex(props);
#else
            var prop = builder.Metadata.FindProperty(propName);
            var index = builder.Metadata.FindIndex(prop);
            builder.Metadata.RemoveIndex(index);
#endif
        }
    }
}