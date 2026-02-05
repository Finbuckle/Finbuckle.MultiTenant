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
        Assert.True(db.Model.FindEntityType(typeof(MyMultiTenantThing)).IsMultiTenant());
    }

    [Fact]
    public void ReturnTrueOnIsMultiTenantOnIfAncestorIsMultiTenant()
    {
        using var db = new TestDbContext();
        Assert.True(db.Model.FindEntityType(typeof(MyMultiTenantChildThing)).IsMultiTenant());
    }

    [Fact]
    public void ReturnFalseOnIsMultiTenantOnIfNotMultiTenant()
    {
        using var db = new TestDbContext();
        Assert.False(db.Model.FindEntityType(typeof(MyThing)).IsMultiTenant());
    }
}