// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

// TODO: Implement more tests.

using Finbuckle.MultiTenant.Strategies;
using Xunit;

namespace Finbuckle.MultiTenant.AspNetCore.Test.Strategies;

public class RemoteAuthenticationCallbackStrategyShould
{
    [Fact]
    public void HavePriorityNeg900()
    {
            var strategy = new RemoteAuthenticationCallbackStrategy(null!);
            Assert.Equal(-900, strategy.Priority);
        }
}