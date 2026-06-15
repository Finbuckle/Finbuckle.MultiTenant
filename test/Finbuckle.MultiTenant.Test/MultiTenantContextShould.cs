using Finbuckle.MultiTenant.Abstractions;
using Xunit;

namespace Finbuckle.MultiTenant.Test;

public class MultiTenantContextShould
{
    [Fact]
    public void ReturnFalseForIsResolvedIfTenantInfoIsNull()
    {
        ITenantContext<TenantInfo> context = new TenantContext<TenantInfo>(null);
        Assert.False(context.IsResolved);
    }

    [Fact]
    public void ReturnTrueIsResolvedIfTenantInfoIsNotNull()
    {
        var context = new TenantContext<TenantInfo>(tenantInfo: new TenantInfo { Id = "", Identifier = "" });

        Assert.True(context.IsResolved);
    }

    [Fact]
    public void ReturnFalseForIsResolvedIfTenantInfoIsNull_NonGeneric()
    {
        ITenantContext context = new TenantContext<TenantInfo>(null);
        Assert.False(context.IsResolved);
    }

    [Fact]
    public void ReturnTrueIsResolvedIfTenantInfoIsNotNull_NonGeneric()
    {
        var context = new TenantContext<TenantInfo>(tenantInfo: new TenantInfo { Id = "", Identifier = "" });

        ITenantContext iContext = context;
        Assert.True(iContext.IsResolved);
    }
}