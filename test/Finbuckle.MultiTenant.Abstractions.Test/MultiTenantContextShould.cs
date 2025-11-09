// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Xunit;

namespace Finbuckle.MultiTenant.Options.Test;

public class MultiTenantContextShould
{
    [Fact]
    public void CloneTenantInfoWhenSetting()
    {
        var ti = new TenantInfo { Id = "test", Name = "Test Tenant", Identifier = "test-identifier" };
        var mtc = new MultiTenantContext<TenantInfo>(tenantInfo: ti);

        // Modify original tenant info after setting it in the context
        ti.Name = "Modified Tenant";

        // The tenant info in the context should not be affected
        Assert.Equal("Test Tenant", mtc.TenantInfo!.Name);
    }

    [Fact]
    public void CloneTenantInfoWhenGetting()
    {
        var ti = new TenantInfo { Id = "test", Name = "Test Tenant", Identifier = "test-identifier" };
        var mtc = new MultiTenantContext<TenantInfo>(tenantInfo: ti);

        var retrievedTenantInfo = mtc.TenantInfo!;

        // Modify the retrieved tenant info
        retrievedTenantInfo.Name = "Modified Tenant";

        // The tenant info in the context should not be affected
        Assert.Equal("Test Tenant", mtc.TenantInfo!.Name);
    }
}