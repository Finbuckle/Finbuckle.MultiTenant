using System;
using System.Collections.Concurrent;
using Finbuckle.MultiTenant.Core;
using Xunit;

public class StaticTenantResolverShould
{
    [Theory]
    [InlineData("initech")]
    [InlineData("Initech")] // maintain case
    [InlineData("")] // empty string
    [InlineData("    ")] // whitespace
    [InlineData(null)] // null
    public void ReturnExpectedIdentifier(string staticIdentifier)
    {
        var strategy = new StaticMultiTenantStrategy(staticIdentifier);

        var identifier = strategy.GetIdentifier(new Object());

        Assert.Equal(staticIdentifier, identifier);
    }
}