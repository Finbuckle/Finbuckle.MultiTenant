//    Copyright 2018 Andrew White
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using Finbuckle.MultiTenant.AspNetCore;
using Finbuckle.MultiTenant.Core;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

public class BasePathMultiTenantStrategyShould
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
    public void ReturnExpectedIdentifier(string path, string expected)
    {
        var httpContext = CreateHttpContextMock(path);
        var strategy = new BasePathMultiTenantStrategy();

        var identifier = strategy.GetIdentifier(httpContext);

        Assert.Equal(expected, identifier);
    }

    [Fact]
    public void ThrowIfContextIsNotHttpContext()
    {
        var context = new Object();
        var strategy = new BasePathMultiTenantStrategy();

        Assert.Throws<MultiTenantException>(() => strategy.GetIdentifier(context));
    }
}