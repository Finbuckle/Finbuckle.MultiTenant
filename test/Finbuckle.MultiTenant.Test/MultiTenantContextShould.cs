using Xunit;

namespace Finbuckle.MultiTenant.Test;

public class MultiTenantContextShould
{
    [Fact]
    public void ReturnFalseForIsResolvedIfTenantInfoIsNull()
    {
        IMultiTenantContext<TenantInfo> context = new MultiTenantContext<TenantInfo>();
        Assert.False(context.IsResolved);
    }
        
    [Fact]
    public void ReturnTrueIsResolvedIfTenantInfoIsNotNull()
    {
        var context = new MultiTenantContext<TenantInfo>
        {
            TenantInfo = new TenantInfo()
        };

        Assert.True(context.IsResolved);
    }
    
    [Fact]
    public void ReturnFalseForIsResolvedIfTenantInfoIsNull_NonGeneric()
    {
        IMultiTenantContext context = new MultiTenantContext<TenantInfo>();
        Assert.False(context.IsResolved);
    }
        
    [Fact]
    public void ReturnTrueIsResolvedIfTenantInfoIsNotNull_NonGeneric()
    {
        var context = new MultiTenantContext<TenantInfo>
        {
            TenantInfo = new TenantInfo()
        };

        IMultiTenantContext iContext = context;
        Assert.True(iContext.IsResolved);
    }
}