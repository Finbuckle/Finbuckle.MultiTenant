// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;
using Finbuckle.MultiTenant.Strategies;
using Xunit;

namespace Finbuckle.MultiTenant.Test.Strategies
{
    public class StaticStrategyShould
    {
        [Theory]
        [InlineData("initech")]
        [InlineData("Initech")] // maintain case
        [InlineData("")] // empty string
        [InlineData("    ")] // whitespace
        [InlineData(null)] // null
        public async void ReturnExpectedIdentifier(string staticIdentifier)
        {
            var strategy = new StaticStrategy(staticIdentifier);

            var identifier = await strategy.GetIdentifierAsync(new Object());

            Assert.Equal(staticIdentifier, identifier);
        }

        [Fact]
        public void HavePriorityNeg1000()
        {
            var strategy = new StaticStrategy("");
            Assert.Equal(-1000, strategy.Priority);
        }
    }
}