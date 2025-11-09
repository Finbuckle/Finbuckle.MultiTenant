using Xunit;

namespace Finbuckle.MultiTenant.Abstractions.Test;

public class MultiTenantContextShould
{
    [Fact]
    public void ReturnCopyOfTenantInfoOnGet()
    {
        var tenantInfo = new TenantInfo(Id: "tenant1", Identifier: "tenant1", Name: "Tenant 1");
        var context = new MultiTenantContext<TenantInfo>(tenantInfo);

        var returnedTenantInfo = context.TenantInfo!;

        Assert.Equal(tenantInfo.Id, returnedTenantInfo.Id);
        Assert.Equal(tenantInfo.Identifier, returnedTenantInfo.Identifier);
        Assert.Equal(tenantInfo.Name, returnedTenantInfo.Name);
        Assert.NotSame(tenantInfo, returnedTenantInfo);
    }
    
    [Fact]
    public void AssignCopyTenantInfoOnSet()
    {
        var tenantInfo = new TenantInfo(Id: "tenant1", Identifier: "tenant1", Name: "Tenant 1");
        var context = new MultiTenantContext<TenantInfo>(tenantInfo);
        
        Assert.Equal(tenantInfo.Id, context._tenantInfo!.Id);
        Assert.Equal(tenantInfo.Identifier, context._tenantInfo.Identifier);
        Assert.Equal(tenantInfo.Name, context._tenantInfo.Name);
        Assert.NotSame(tenantInfo, context._tenantInfo);
    }
}