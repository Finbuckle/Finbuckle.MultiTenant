// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;
using Xunit;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test.Extensions.ModelBuilderExtensions;

public class ModelBuilderExtensionShould
{
    [Fact]
    public void OnConfigureMultiTenantSetMultiTenantOnTypeWithMultiTenantAttribute()
    {
        using var db = new TestDbContext();
        Assert.True(db.Model.FindEntityType(typeof(MyMultiTenantThing)).IsMultiTenant());
    }

    [Fact]
    public void OnConfigureMultiTenantDoNotSetMultiTenantOnTypeWithoutMultiTenantAttribute()
    {
        using var db = new TestDbContext();
        Assert.False(db.Model.FindEntityType(typeof(MyThing)).IsMultiTenant());
    }
}