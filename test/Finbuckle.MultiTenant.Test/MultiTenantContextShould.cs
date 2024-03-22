using Xunit;

namespace Finbuckle.MultiTenant.Test;

public class MultiTenantContextShould
{
    [Fact]
    public void ReturnFalseInHasResolvedTenantIfTenantInfoIsNull()
    {
        IMultiTenantContext<TenantInfo> context = new MultiTenantContext<TenantInfo>();
        Assert.False(context.IsResolved);
    }
        
    [Fact]
    public void ReturnTrueInHasResolvedTenantIfTenantInfoIsNotNull()
    {
        var context = new MultiTenantContext<TenantInfo>
        {
            TenantInfo = new TenantInfo()
        };

        Assert.True(context.IsResolved);
    }
    
    [Fact]
    public void ReturnFalseInHasResolvedTenantIfTenantInfoIsNull_NonGeneric()
    {
        IMultiTenantContext context = new MultiTenantContext<TenantInfo>();
        Assert.False(context.IsResolved);
    }
        
    [Fact]
    public void ReturnTrueInHasResolvedTenantIfTenantInfoIsNotNull_NonGeneric()
    {
        var context = new MultiTenantContext<TenantInfo>
        {
            TenantInfo = new TenantInfo()
        };

        IMultiTenantContext iContext = context;
        Assert.True(iContext.IsResolved);
    }
}