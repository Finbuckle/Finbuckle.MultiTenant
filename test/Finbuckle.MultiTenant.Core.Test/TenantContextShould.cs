using System;
using System.Collections.Concurrent;
using Finbuckle.MultiTenant.Core;
using Xunit;

public class TenantContextShould
{
    [Fact]
    public void ThrowIfTenantIdSetWithLengthAboveTenantIdMaxLength()
    {
        new TenantContext("".PadRight(1, 'a'), null, null, null, null, null);
        new TenantContext("".PadRight(Constants.TenantIdMaxLength, 'a'), null, null, null, null, null);
        
        Assert.Throws<MultiTenantException>(() => new TenantContext("".PadRight(Constants.TenantIdMaxLength + 1, 'a'), null, null, null, null, null));
        Assert.Throws<MultiTenantException>(() => new TenantContext("".PadRight(999, 'a'), null, null, null, null, null));
    }
}