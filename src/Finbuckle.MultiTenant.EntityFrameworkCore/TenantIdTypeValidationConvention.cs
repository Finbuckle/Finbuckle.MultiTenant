// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Finbuckle.MultiTenant.EntityFrameworkCore;

/// <summary>
/// Throws when the model is finalized if a multi-tenant entity type has a TenantId property whose type does not match
/// the tenant ID type of the database context.
/// </summary>
internal sealed class TenantIdTypeValidationConvention(Type contextTenantIdType) : IModelFinalizingConvention
{
    public void ProcessModelFinalizing(IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            // entity types marked IsNotMultiTenant are not multi-tenant and are not checked
            if (!entityType.IsMultiTenant)
                continue;

            var property = entityType.FindProperty("TenantId");
            if (property is null || property.ClrType == contextTenantIdType)
                continue;

            // an inherited property is reported once, on the multi-tenant base type that declares it
            if (property.DeclaringType != entityType && entityType.BaseType is { IsMultiTenant: true })
                continue;

            throw new MultiTenantException(
                $"Entity type '{entityType.DisplayName()}' is configured as multi-tenant with a TenantId property of " +
                $"type {property.ClrType}, but the database context uses tenant ID type {contextTenantIdType}. " +
                "The tenant ID type passed to IsMultiTenant must match the database context's tenant ID type.");
        }
    }
}
