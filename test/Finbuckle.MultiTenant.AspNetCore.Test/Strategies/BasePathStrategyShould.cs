// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;
using Finbuckle.MultiTenant.Strategies;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Finbuckle.MultiTenant.AspNetCore.Test.Strategies
{
    public class BasePathStrategyShould
    {
        private HttpContext CreateHttpContextMock(string path)
        {
            var mock = new Mock<HttpContext>();
            mock.Setup(c => c.Request.Path).Returns(path);

            return mock.Object;
        }

        [Theory]
        [InlineData("/test", "test")] // single path
        [InlineData("/Test", "Test")] // maintain case
        [InlineData("", null)] // no path
        [InlineData("/", null)] // just trailing slash
        [InlineData("/initech/ignore/ignore", "initech")] // multiple path segments
        public async void ReturnExpectedIdentifier(string path, string expected)
        {
            var httpContext = CreateHttpContextMock(path);
            var strategy = new BasePathStrategy();

            var identifier = await strategy.GetIdentifierAsync(httpContext);

            Assert.Equal(expected, identifier);
        }

        [Fact]
        public void ThrowIfContextIsNotHttpContext()
        {
            var context = new Object();
            var strategy = new BasePathStrategy();

            Assert.Throws<AggregateException>(() => strategy.GetIdentifierAsync(context).Result);
        }
    }
}