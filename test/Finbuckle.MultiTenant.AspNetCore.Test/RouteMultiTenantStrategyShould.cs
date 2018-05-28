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
using System.Collections.Concurrent;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.AspNetCore;
using Finbuckle.MultiTenant.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Moq;
using Xunit;

public class RouteMultiTenantStrategyShould
{
    private HttpContext CreateHttpContextMock(string tenantParam, string routeValue)
    {
        var routeData = new RouteData();
        routeData.Values.Add(tenantParam, routeValue);
        var mockFeature = new Mock<IRoutingFeature>();
        mockFeature.Setup(f => f.RouteData).Returns(routeData);

        var mock = new Mock<HttpContext>();
        mock.Setup(c => c.Features[typeof(IRoutingFeature)]).Returns(mockFeature.Object);

        return mock.Object;
    }

    private HttpContext CreateHttpContextMockWithNoRouteData()
    {
        var mock = new Mock<HttpContext>();
        mock.Setup(c => c.Features[typeof(IRoutingFeature)]).Returns(null);

        return mock.Object;
    }

    [Theory]
    [InlineData("__tenant__", "initech", "initech")] // single path
    [InlineData("__tenant__", "Initech", "Initech")] // maintain case
    public void ReturnExpectedIdentifier(string tenantParam, string routeValue, string expected)
    {
        var httpContext = CreateHttpContextMock(tenantParam, routeValue);
        var strategy = new RouteMultiTenantStrategy(tenantParam);

        var identifier = strategy.GetIdentifier(httpContext);

        Assert.Equal(expected, identifier);
    }

    [Fact]
    public void ThrowIfContextIsNotHttpContext()
    {
        var context = new Object();
        var strategy = new RouteMultiTenantStrategy("__tenant__");

        Assert.Throws<MultiTenantException>(() => strategy.GetIdentifier(context));
    }

    [Fact]
    public void ReturnNullIfNoRouteParamMatch()
    {
        var httpContext = CreateHttpContextMock("__tenant__", "initech");

        var strategy = new RouteMultiTenantStrategy("controller");
        var identifier = strategy.GetIdentifier(httpContext);

        Assert.Null(identifier);
    }

    [Fact]
    public void ReturnNullIfNoRouteData()
    {
        var httpContext = CreateHttpContextMockWithNoRouteData();

        var strategy = new RouteMultiTenantStrategy("__tenant__");
        var identifier = strategy.GetIdentifier(httpContext);

        Assert.Null(identifier);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ThrowIfRouteParamIsNullOrWhitespace(string testString)
    {
        Assert.Throws<MultiTenantException>(() => new RouteMultiTenantStrategy(testString));
    }
}