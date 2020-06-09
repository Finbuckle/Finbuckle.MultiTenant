//    Copyright 2018-2020 Andrew White
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
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Strategies;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

public class HostStrategyShould
{
    private HttpContext CreateHttpContextMock(string host)
    {
        var mock = new Mock<HttpContext>();
        mock.Setup(c => c.Request.Host).Returns(new HostString(host));

        return mock.Object;
    }

    [Theory]
    [InlineData("", "__tenant__", null)] // no host
    [InlineData("initech", "__tenant__", "initech")] // basic match
    [InlineData("Initech", "__tenant__", "Initech")] // maintain case
    [InlineData("abc.com.test.", "__tenant__.", null)] // invalid pattern
    [InlineData("abc", "__tenant__.", null)] // invalid pattern
    [InlineData("abc", ".__tenant__", null)] // invalid pattern
    [InlineData("abc", ".__tenant__.", null)] // invalid pattern
    [InlineData("abc-cool.org", "__tenant__-cool.org", "abc")] // mixed segment
    [InlineData("abc.com.test", "__tenant__.*", "abc")] // first segment
    [InlineData("abc", "__tenant__.*", "abc")] // first and only segment
    [InlineData("www.example.test", "?.__tenant__.?", "example")] // domain
    [InlineData("www.example.test", "?.__tenant__.*", "example")] // 2nd segment
    [InlineData("www.example", "?.__tenant__.*", "example")] // 2nd segment
    [InlineData("www.example.r", "?.__tenant__.?.*", "example")] // 2nd segment of 3+
    [InlineData("www.example.r.f", "?.__tenant__.?.*", "example")] // 2nd segment of 3+
    [InlineData("example.ok.test", "*.__tenant__.?.?", "example")] // 3rd last segment
    [InlineData("w.example.ok.test", "*.?.__tenant__.?.?", "example")] // 3rd last of 4+ segments
    [InlineData("example.com", "__tenant__", "example.com")] // match entire domain (2.1)

    public async void ReturnExpectedIdentifier(string host, string template, string expected)
    {
        var httpContext = CreateHttpContextMock(host);
        var strategy = new HostStrategy(template);

        var identifier = await strategy.GetIdentifierAsync(httpContext);

        Assert.Equal(expected, identifier);
    }

    [Theory]
    [InlineData("*.__tenant__.*")]
    [InlineData("*a.__tenant__")]
    [InlineData("a*a.__tenant__")]
    [InlineData("a*.__tenant__")]
    [InlineData("*-.__tenant__")]
    [InlineData("-*-.__tenant__")]
    [InlineData("-*.__tenant__")]
    [InlineData("__tenant__.-?")]
    [InlineData("__tenant__.-?-")]
    [InlineData("__tenant__.?-")]
    [InlineData("")]
    [InlineData("     ")]
    [InlineData(null)]

    public void ThrowIfInvalidTemplate(string template)
    {
        Assert.Throws<MultiTenantException>(() => new HostStrategy(template));
    }

    [Fact]
    public void ThrowIfContextIsNotHttpContext()
    {
        var context = new Object();
        var strategy = new HostStrategy("__tenant__.*");

        Assert.Throws<AggregateException>(() => strategy.GetIdentifierAsync(context).Result);
    }
}