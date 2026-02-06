// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Linq.Expressions;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;

/// <summary>
/// Extension methods for configuring multi-tenant entity types.
/// </summary>
public static class EntityTypeBuilderExtensions
{
    private class ExpressionVariableScope
    {
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public IMultiTenantDbContext? Context { get; }
    }

    /// <summary>
    /// Marks an entity as non-multi-tenant, removing any tenant-based query filters.
    /// </summary>
    /// <param name="builder">The <see cref="EntityTypeBuilder"/> instance.</param>
    /// <returns>The same <see cref="EntityTypeBuilder"/> instance for method chaining.</returns>
    /// <remarks>
    /// This method is useful for excluding specific entities from tenant isolation in a multi-tenant context.
    /// It sets the multi-tenant annotation to false, removes the Tenant Id shadow property if applicable, and removes the tenant query filter if it exists.
    /// </remarks>
    public static EntityTypeBuilder IsNotMultiTenant(this EntityTypeBuilder builder)
    {
        if (builder.Metadata.FindAnnotation(Constants.MultiTenantAnnotationName) is IAnnotation { Value: true })
        {
            // remove the multi-tenant annotation
            builder.Metadata.SetAnnotation(Constants.MultiTenantAnnotationName, false);

            // remove the shadow tenant id property if it exists
            if (builder.Metadata.GetProperty("TenantId") is var property && property.IsShadowProperty())
                builder.Metadata.RemoveProperty(property);


            // remove the named query filter if it exists
            var existingFilter = builder.Metadata.FindDeclaredQueryFilter(Abstractions.Constants.TenantToken);
            if (existingFilter is not null)
                builder.Metadata.SetQueryFilter(Abstractions.Constants.TenantToken, null);
        }

        return builder;
    }

    /// <summary>
    /// Adds multi-tenant support for an entity via a named query filter.
    /// </summary>
    /// <param name="builder">The typed <see cref="EntityTypeBuilder"/> instance.</param>
    /// <returns>A <see cref="MultiTenantEntityTypeBuilder"/> instance.</returns>
    /// <remarks>A string property named TenantId is used in the query filter. If one does not already exist on the entity a shadow property is used.</remarks>
    public static MultiTenantEntityTypeBuilder IsMultiTenant(this EntityTypeBuilder builder)
    {
        if (builder.Metadata.IsMultiTenant())
            return new MultiTenantEntityTypeBuilder(builder);

        builder.HasAnnotation(Constants.MultiTenantAnnotationName, true);

        try
        {
            builder.Property<string>("TenantId").IsRequired();
        }
        catch (Exception ex)
        {
            throw new MultiTenantException($"{builder.Metadata.ClrType} unable to add TenantId property", ex);
        }

        // build expression tree for e => EF.Property<string>(e, "TenantId") == TenantInfo.Id

        // where e is one of our entity types
        // will need this ParameterExpression for next step and for final step
        var entityParamExp = Expression.Parameter(builder.Metadata.ClrType, "e");

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

        #region Fork Sirfull
        // Original code
        // this code will generate this expression : EF.Property<string>(e, "TenantId") == TenantInfo.Id
        // var rightExp = Expression.Property(contextTenantInfoExp, nameof(IMultiTenantDbContext.TenantInfo.Id));

        // On récupère toujours les entités avec TenantId = "*" car elles sont considérées comme des entités globales (non multi-tenant)

        // Generate expression: IsMultiTenantEnabled == False 
        var multiTenantDisabled = Expression.Equal(Expression.Property(contextMemberAccessExp, nameof(IMultiTenantDbContext.IsMultiTenantEnabled)), Expression.Constant(false));

        // Generate expression:  EF.Property<string>(e, "TenantId") == "*"
        var tenantIdIsWildcard = Expression.Equal(leftExp, Expression.Constant("*", typeof(string)));

        // Generate expression: TenantInfo == null
        var tenantInfoIsNull = Expression.Equal(contextTenantInfoExp, Expression.Constant(null));

        // Generate expression: EF.Property<string>(e, "TenantId") == TenantInfo == null ? "*" : TenantInfo.Id
        var rightExp = Expression.Condition(
            Expression.Equal(contextTenantInfoExp, Expression.Constant(null)),
            Expression.Constant("*", typeof(string)),
            Expression.Property(contextTenantInfoExp, nameof(IMultiTenantDbContext.TenantInfo.Id))
        );
        var tenantIdEqualsInfo = Expression.Equal(leftExp, rightExp);

        // Build the complete predicate with OR conditions
        //  IsMultiTenantEnabled == False 
        //   || EF.Property<string>(e, "TenantId") == "*"
        //   || TenantInfo == null
        //   || EF.Property<string>(e, "TenantId") == TenantInfo == null ? "*" : TenantInfo.Id
        var predicate = Expression.OrElse(
            multiTenantDisabled,
            Expression.OrElse(
                tenantIdIsWildcard,
                Expression.OrElse(
                    tenantInfoIsNull,
                    tenantIdEqualsInfo
                )
            )
        );
        #endregion

        // build the final expression tree
        var delegateType = Expression.GetDelegateType(builder.Metadata.ClrType, typeof(bool));
        var lambdaExp = Expression.Lambda(delegateType, predicate, entityParamExp);

        // set the filter
        builder.HasQueryFilter(Abstractions.Constants.TenantToken, lambdaExp);

        return new MultiTenantEntityTypeBuilder(builder);
    }
}