// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;
using Xunit;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test.Extensions.ModelExtensions;

public class ModelExtensionsShould
{
    [Fact]
    public void ReturnMultiTenantTypes()
    {
        using var db = new TestDbContext();
        Assert.Contains(typeof(MyMultiTenantThing), db.Model.GetMultiTenantEntityTypes().Select(et => et.ClrType));
    }

    [Fact]
    public void NotReturnNonMultiTenantTypes()
    {
        using var db = new TestDbContext();
        Assert.DoesNotContain(typeof(MyThing), db.Model.GetMultiTenantEntityTypes().Select(et => et.ClrType));
    }
}