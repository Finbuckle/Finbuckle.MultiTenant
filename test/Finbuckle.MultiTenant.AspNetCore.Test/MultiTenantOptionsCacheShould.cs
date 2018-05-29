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
using System.Threading.Tasks;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.AspNetCore;
using Finbuckle.MultiTenant.Core;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

public class MultiTenantOptionsCacheShould
{
    private IHttpContextAccessor CreateHttpContextAccessorMock(TenantContext tenantContext)
    {
        var httpContextMock = new Mock<HttpContext>();
        object tc = tenantContext;
        httpContextMock.Setup(c => c.Items.TryGetValue(Finbuckle.MultiTenant.AspNetCore.Constants.HttpContextTenantContext, out tc)).Returns(true);

        var mock = new Mock<IHttpContextAccessor>();
        mock.SetupGet(c => c.HttpContext).Returns(httpContextMock.Object);

        return mock.Object;
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("name")]
    public void AddAdjustedTenantOptionsForOptionsName(string name)
    {
        var tc = new TenantContext("test-id-123", null, null, null, null, null);
        var cache = new MultiTenantOptionsCache<CookieAuthenticationOptions>(CreateHttpContextAccessorMock(tc), (o, context) =>
        {
            o.Cookie.Name = context.Id;
        });

        var options = new CookieAuthenticationOptions();

        // Add new options.
        var result = cache.TryAdd(name, options);
        Assert.True(result);

        // Fail adding options under same name.
        result = cache.TryAdd(name, options);
        Assert.False(result);

        // Check the option was adjusted.
        var adjustedOption = cache.GetOrAdd(name, () => options);
        Assert.Equal(tc.Id, adjustedOption.Cookie.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("name")]
    public void GetOrAddAdjustedTenantOptionsForOptionsName(string name)
    {
        var tc = new TenantContext("test-id-123", null, null, null, null, null);
        var cache = new MultiTenantOptionsCache<CookieAuthenticationOptions>(CreateHttpContextAccessorMock(tc), (o, context) =>
        {
            o.Cookie.Name = context.Id;
        });

        var options = new CookieAuthenticationOptions();

        // Add new options.
        var result = cache.TryAdd(name, options);
        Assert.True(result);

        // Get an existing object.
        var adjustedOption = cache.GetOrAdd(name, () => options);
        Assert.Equal(tc.Id, adjustedOption.Cookie.Name);

        // Add a nonexisting object.
        adjustedOption = cache.GetOrAdd("not_here", () => new CookieAuthenticationOptions());
        Assert.Equal(tc.Id, adjustedOption.Cookie.Name);
    }

    [Fact]
    public void ThrowsIfGetOtAddFactoryIsNull()
    {
        var tc = new TenantContext("test-id-123", null, null, null, null, null);
        var cache = new MultiTenantOptionsCache<CookieAuthenticationOptions>(CreateHttpContextAccessorMock(tc), (o, context) =>
        {
            o.Cookie.Name = context.Id;
        });

        Assert.Throws<MultiTenantException>(() => cache.GetOrAdd("", null));
    }

    [Fact]
    public void ThrowIfContructorParamIsNull()
    {
        var tc = new TenantContext("test-id-123", null, null, null, null, null);
        var cache = new MultiTenantOptionsCache<CookieAuthenticationOptions>(CreateHttpContextAccessorMock(tc), (o, context) =>
        {
            o.Cookie.Name = context.Id;
        });

        Assert.Throws<MultiTenantException>(() => new MultiTenantOptionsCache<CookieAuthenticationOptions>(CreateHttpContextAccessorMock(tc), null));

        Assert.Throws<MultiTenantException>(() => new MultiTenantOptionsCache<CookieAuthenticationOptions>(null, (o, context) => o.Cookie.Name = ""));

        Assert.Throws<MultiTenantException>(() => new MultiTenantOptionsCache<CookieAuthenticationOptions>(null, null));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("name")]
    public void RemoveTenantOptionsForOptionsName(string name)
    {
        var tc = new TenantContext("test-id-123", null, null, null, null, null);

        var cache = new MultiTenantOptionsCache<CookieAuthenticationOptions>(CreateHttpContextAccessorMock(tc), (o, context) =>
        {
            o.Cookie.Name = context.Id;
        });

        var options = new CookieAuthenticationOptions();

        // Add new options.
        var result = cache.TryAdd(name, options);
        Assert.True(result);

        // Get an existing object.
        var adjustedOption = cache.GetOrAdd(name, () => options);
        Assert.Equal(tc.Id, adjustedOption.Cookie.Name);

        // Remove the existing object.
        result = cache.TryRemove(name);
        Assert.True(result);

        // Get the nonexisting object.
        adjustedOption = cache.GetOrAdd(name, () => options);
        Assert.Same(adjustedOption, options);
    }

}