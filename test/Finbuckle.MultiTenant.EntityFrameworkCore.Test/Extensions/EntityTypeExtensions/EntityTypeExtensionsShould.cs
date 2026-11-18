// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;
using Xunit;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test.Extensions.EntityTypeExtensions;

public class EntityTypeExtensionShould
{
    [Fact]
    public void ReturnTrueOnIsMultiTenantOnIfMultiTenant()
    {
        using var db = new TestDbContext();
        var entityType = db.Model.FindEntityType(typeof(MyMultiTenantThing));
        Assert.NotNull(entityType);
        Assert.True(entityType!.IsMultiTenant);
    }

    [Fact]
    public void ReturnTrueOnIsMultiTenantOnIfAncestorIsMultiTenant()
    {
        using var db = new TestDbContext();
        var entityType = db.Model.FindEntityType(typeof(MyMultiTenantChildThing));
        Assert.NotNull(entityType);
        Assert.True(entityType!.IsMultiTenant);
    }

    [Fact]
    public void ReturnFalseOnIsMultiTenantOnIfNotMultiTenant()
    {
        using var db = new TestDbContext();
        var entityType = db.Model.FindEntityType(typeof(MyThing));
        Assert.NotNull(entityType);
        Assert.False(entityType!.IsMultiTenant);
    }
}