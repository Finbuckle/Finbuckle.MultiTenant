// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Linq;
using System.Linq.Expressions;
using Finbuckle.MultiTenant.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Finbuckle.MultiTenant;

public static class EntityTypeBuilderExtensions
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
                .HasMaxLength(Internal.Constants.TenantIdMaxLength);
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

        return new MultiTenantEntityTypeBuilder(builder);
    }
}