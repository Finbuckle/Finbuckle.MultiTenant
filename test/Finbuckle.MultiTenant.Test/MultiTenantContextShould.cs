using Finbuckle.MultiTenant.Abstractions;
using Xunit;

namespace Finbuckle.MultiTenant.Test;

public class MultiTenantContextShould
{
    [Fact]
    public void ReturnFalseForIsResolvedIfTenantInfoIsNull()
    {
        IMultiTenantContext<TenantInfo> context = new MultiTenantContext<TenantInfo>(null);
        Assert.False(context.IsResolved);
    }

    [Fact]
    public void ReturnTrueIsResolvedIfTenantInfoIsNotNull()
    {
        var context = new MultiTenantContext<TenantInfo>(tenantInfo: new TenantInfo { Id = "", Identifier = "" });

        Assert.True(context.IsResolved);
    }

    [Fact]
    public void ReturnFalseForIsResolvedIfTenantInfoIsNull_NonGeneric()
    {
        IMultiTenantContext context = new MultiTenantContext<TenantInfo>(null);
        Assert.False(context.IsResolved);
    }

    [Fact]
    public void ReturnTrueIsResolvedIfTenantInfoIsNotNull_NonGeneric()
    {
        var context = new MultiTenantContext<TenantInfo>(tenantInfo: new TenantInfo { Id = "", Identifier = "" });

        IMultiTenantContext iContext = context;
        Assert.True(iContext.IsResolved);
    }
}