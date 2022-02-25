// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.Strategies;
using Xunit;

namespace Finbuckle.MultiTenant.Test.Strategies
{
    public class DelegateStrategyShould
    {
        [Fact]
        public void CallDelegate()
        {
            int i = 0;
            var strategy = new DelegateStrategy(_ => Task.FromResult<string?>((i++).ToString()));
            strategy.GetIdentifierAsync(new object()).Wait();

            Assert.Equal(1, i);
        }

        [Theory]
        [InlineData("initech-id")]
        [InlineData("")]
        [InlineData(null)]
        public async Task ReturnDelegateResult(string identifier)
        {
            var strategy = new DelegateStrategy(async _ => await Task.FromResult(identifier));
            var result = await strategy.GetIdentifierAsync(new object());

            Assert.Equal(identifier, result);
        }
        
        [Fact]
        public async Task BeAbleToReturnNull()
        {
            var strategy = new DelegateStrategy(async _ => await Task.FromResult<string?>(null));
            var result = await strategy.GetIdentifierAsync(new object());

            Assert.Null(result);
        }

        [Fact]
        public void ThrowIfNullDelegate()
        {
            Assert.Throws<ArgumentNullException>(() => new DelegateStrategy(null!));
        }
    }
}