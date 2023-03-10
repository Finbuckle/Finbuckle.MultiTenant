using Xunit;

namespace Finbuckle.MultiTenant.Test;

public class MultiTenantContextShould
{
    [Fact]
    public void ReturnFalseInHasResolvedTenantIfTenantInfoIsNull()
    {
        IMultiTenantContext<TenantInfo> context = new MultiTenantContext<TenantInfo>();
        Assert.False(context.HasResolvedTenant);
    }
        
    [Fact]
    public void ReturnTrueInHasResolvedTenantIfTenantInfoIsNotNull()
    {
        IMultiTenantContext<TenantInfo> context = new MultiTenantContext<TenantInfo>();
        context.TenantInfo = new TenantInfo();
        Assert.True(context.HasResolvedTenant);
    }
    
    [Fact]
    public void ReturnFalseInHasResolvedTenantIfTenantInfoIsNull_NonGeneric()
    {
        IMultiTenantContext context = new MultiTenantContext<TenantInfo>();
        Assert.False(context.HasResolvedTenant);
    }
        
    [Fact]
    public void ReturnTrueInHasResolvedTenantIfTenantInfoIsNotNull_NonGeneric()
    {
        var context = new MultiTenantContext<TenantInfo>
        {
            TenantInfo = new TenantInfo()
        };

        IMultiTenantContext iContext = context;
        Assert.True(iContext.HasResolvedTenant);
    }
}