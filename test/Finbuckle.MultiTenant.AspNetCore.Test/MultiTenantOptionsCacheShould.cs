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
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
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
    public void AdjustedOptionsNameOnAdd(string name)
    {
        var tc = new TenantContext("test-id-123", null, null, null, null, null);
        var tca = new TestTenantContextAccessor(tc);
        var cache = new MultiTenantOptionsCache<CookieAuthenticationOptions>(tca);

        var options = new CookieAuthenticationOptions();

        // Add new options.
        var result = cache.TryAdd(name, options);
        Assert.True(result);

        // Fail adding options under same name.
        result = cache.TryAdd(name, options);
        Assert.False(result);

        // Change the TC id and confirm options can be added again.
        tc.GetType().GetProperty("Id").SetValue(tc, "diff_id");
        result = cache.TryAdd(name, options);
        Assert.True(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("name")]
    public void AdjustOptionsNameOnGetOrAdd(string name)
    {
        var tc = new TenantContext("test-id-123", null, null, null, null, null);
        var tca = new TestTenantContextAccessor(tc);
        var cache = new MultiTenantOptionsCache<CookieAuthenticationOptions>(tca);

        var options = new CookieAuthenticationOptions();
        options.Cookie.Name = "a_name";
        var options2 = new CookieAuthenticationOptions();
        options2.Cookie.Name = "diff_name";

        // Add new options.
        var result = cache.GetOrAdd(name, () => options);
        Assert.Equal(options.Cookie.Name, result.Cookie.Name);

        // Get the existing object.
        result = cache.GetOrAdd(name, () => options2);
        Assert.NotEqual(options2.Cookie.Name, result.Cookie.Name);

        // Confirm different tenant on same object is an add.
        tc.GetType().GetProperty("Id").SetValue(tc, "diff_id");
        result = cache.GetOrAdd(name, () => options2);
        Assert.Equal(options2.Cookie.Name, result.Cookie.Name);
    }

    [Fact]
    public void ThrowsIfGetOtAddFactoryIsNull()
    {
        var tc = new TenantContext("test-id-123", null, null, null, null, null);
        var tca = new TestTenantContextAccessor(tc);
        var cache = new MultiTenantOptionsCache<CookieAuthenticationOptions>(tca);

        Assert.Throws<ArgumentNullException>(() => cache.GetOrAdd("", null));
    }

    [Fact]
    public void ThrowIfContructorParamIsNull()
    {
        var tc = new TenantContext("test-id-123", null, null, null, null, null);
        var tca = new TestTenantContextAccessor(tc);

        Assert.Throws<ArgumentNullException>(() => new MultiTenantOptionsCache<CookieAuthenticationOptions>(null));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("name")]
    public void RemoveOptionsForAllTenants(string name)
    {
        var tc = new TenantContext("test-id-123", null, null, null, null, null);
        var tca = new TestTenantContextAccessor(tc);
        var cache = new MultiTenantOptionsCache<CookieAuthenticationOptions>(tca);

        var options = new CookieAuthenticationOptions();

        // Add new options.
        var result = cache.TryAdd(name, options);
        Assert.True(result);

        // Add under a different tenant.
        tc.GetType().GetProperty("Id").SetValue(tc, "diff_id");
        result = cache.TryAdd(name, options);
        Assert.True(result);

        // Remove all options and assert empty.
        result = cache.TryRemove(name);
        Assert.True(result);
        Assert.Empty((IEnumerable)cache.GetType().BaseType.
            GetField("_cache", BindingFlags.NonPublic | BindingFlags.Instance).
            GetValue(cache));
    }
}