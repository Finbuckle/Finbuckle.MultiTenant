// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Finbuckle.MultiTenant.EntityFrameworkCore
{
    // public class TenantIdGenerator : ValueGenerator<string?>
    // {
    //     public override string? Next(EntityEntry entry)
    //     {
    //         return ((IMultiTenantDbContext)entry.Context).TenantInfo.Id;
    //     }
    //
    //     public override bool GeneratesTemporaryValues => false;
    // }

    public static class FinbuckleEntityTypeBuilderExtensions
    {
        private class ExpressionVariableScope
        {
            public IMultiTenantDbContext? Context { get; }
        }

        private static LambdaExpression? GetQueryFilter(this EntityTypeBuilder builder)
        {
            return builder.Metadata.GetQueryFilter();
        }

        /// <summary>
        /// Adds MultiTenant support for an entity. Call <see cref="IsMultiTenant" /> after
        /// <see cref="EntityTypeBuilder.HasQueryFilter" /> to merge query filters.
        /// </summary>
        /// <typeparam name="T">The specific type of <see cref="EntityTypeBuilder"/></typeparam>
        /// <param name="builder">The typed EntityTypeBuilder instance.</param>
        /// <returns>A MultiTenantEntityTypeBuilder instance.</returns>
        public static MultiTenantEntityTypeBuilder IsMultiTenant(this EntityTypeBuilder builder)
        {
            if (builder.Metadata.IsMultiTenant())
                return new MultiTenantEntityTypeBuilder(builder);

            builder.HasAnnotation(Constants.MultiTenantAnnotationName, true);

            try
            {
                builder.Property<string>("TenantId")
                       .IsRequired()
                       .HasMaxLength(Finbuckle.MultiTenant.Internal.Constants.TenantIdMaxLength);
//                       .HasValueGenerator<TenantIdGenerator>();
            }
            catch (Exception ex)
            {
                throw new MultiTenantException($"{builder.Metadata.ClrType} unable to add TenantId property", ex);
            }

            // build expression tree for e => EF.Property<string>(e, "TenantId") == TenantInfo.Id

            // where e is one of our entity types
            // will need this ParameterExpression for next step and for final step
            var entityParamExp = Expression.Parameter(builder.Metadata.ClrType, "e");

            var existingQueryFilter = builder.GetQueryFilter();

            // override to match existing query parameter if applicable
            if (existingQueryFilter != null)
            {
                entityParamExp = existingQueryFilter.Parameters.First();
            }

            // build up expression tree for: EF.Property<string>(e, "TenantId")
            var tenantIdExp = Expression.Constant("TenantId", typeof(string));
            var efPropertyExp = Expression.Call(typeof(EF), nameof(EF.Property), new[] { typeof(string) }, entityParamExp, tenantIdExp);
            var leftExp = efPropertyExp;

            // build up express tree for: TenantInfo.Id
            // EF will magically sub the current db context in for scope.Context
            var scopeConstantExp = Expression.Constant(new ExpressionVariableScope());
            var contextMemberInfo = typeof(ExpressionVariableScope).GetMember(nameof(ExpressionVariableScope.Context))[0];
            var contextMemberAccessExp = Expression.MakeMemberAccess(scopeConstantExp, contextMemberInfo);
            var contextTenantInfoExp = Expression.Property(contextMemberAccessExp, nameof(IMultiTenantDbContext.TenantInfo));
            var rightExp = Expression.Property(contextTenantInfoExp, nameof(IMultiTenantDbContext.TenantInfo.Id));

            // build expression tree for EF.Property<string>(e, "TenantId") == TenantInfo.Id'
            var predicate = Expression.Equal(leftExp, rightExp);

            // combine with existing filter
            if (existingQueryFilter != null)
            {
                predicate = Expression.AndAlso(existingQueryFilter.Body, predicate);
            }

            // build the final expression tree
            var delegateType = Expression.GetDelegateType(builder.Metadata.ClrType, typeof(bool));
            var lambdaExp = Expression.Lambda(delegateType, predicate, entityParamExp);

            // set the filter
            builder.HasQueryFilter(lambdaExp);

            // TODO: Legacy code for Identity types. Should be covered by adjustUniqueIndexes etc in the future.
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

                // This is a special case that should still occur.
                // Note the index below is not unique;
                if (clrType.ImplementsOrInheritsUnboundGeneric(typeof(IdentityUserLogin<>)))
                {
                    UpdateIdentityUserLoginPrimaryKey(builder);
                    AddIdentityUserLoginIndex(builder);
                }
            }

            return new MultiTenantEntityTypeBuilder(builder);
        }

        private static void UpdateIdentityUserIndex(this EntityTypeBuilder builder)
        {
            builder.RemoveIndex("NormalizedUserName");
#if NET // Covers .NET 5.0 and later.
            builder.HasIndex("NormalizedUserName", "TenantId").HasDatabaseName("UserNameIndex").IsUnique();
#elif NETSTANDARD2_1 // .NET Core 3.1
            builder.HasIndex("NormalizedUserName", "TenantId").HasName("UserNameIndex").IsUnique();
#endif
        }

        private static void UpdateIdentityRoleIndex(this EntityTypeBuilder builder)
        {
            builder.RemoveIndex("NormalizedName");
#if NET // Covers .NET 5.0 and later.
            builder.HasIndex("NormalizedName", "TenantId").HasDatabaseName("RoleNameIndex").IsUnique();
#elif NETSTANDARD2_1 // .NET Core 3.1
            builder.HasIndex("NormalizedName", "TenantId").HasName("RoleNameIndex").IsUnique();
#endif
        }

        private static void UpdateIdentityUserLoginPrimaryKey(this EntityTypeBuilder builder)
        {
            var pk = builder.Metadata.FindPrimaryKey();

            // Remove the key if it exists.
            if (pk != null)
            {
                builder.Metadata.RemoveKey(pk.Properties);
            }

            // Create a new ID and a unique index to replace the old pk.
            builder.Property<string>("Id").ValueGeneratedOnAdd();
        }

        private static void AddIdentityUserLoginIndex(this EntityTypeBuilder builder)
        {
            builder.HasIndex("LoginProvider", "ProviderKey", "TenantId").IsUnique();
        }

        private static void RemoveIndex(this EntityTypeBuilder builder, string propName)
        {
            var prop = builder.Metadata.FindProperty(propName);
            var index = prop is null ? null : builder.Metadata.FindIndex(prop);

            // Remove the index if one is found.
            if (index != null)
            {
                builder.Metadata.RemoveIndex(index);
            }
        }
    }
}