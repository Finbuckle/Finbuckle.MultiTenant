//    Copyright 2019 Andrew White
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
using System.Collections.Generic;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class HttpContextExtensionShould
{
    [Fact]
    public void GetTenantContextIfExists()
    {
        var items = new Dictionary<object, object>();
        var ti = new TenantInfo("test", null, null, null, null);
        var tc = new MultiTenantContext();
        tc.TenantInfo = ti;
        items.Add(Finbuckle.MultiTenant.AspNetCore.Constants.HttpContextMultiTenantContext, tc);

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.Items).Returns(items);

        var mtc = httpContextMock.Object.GetMultiTenantContext();

        Assert.Same(tc, mtc);
    }

    [Fact]
    public void ReturnNullIfNoMultiTenantContext()
    {
        var items = new Dictionary<object, object>();
        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.Items).Returns(items);

        var mtc = httpContextMock.Object.GetMultiTenantContext();

        Assert.Null(mtc);
    }

    [Fact]
    public void SetTenantInfo()
    {
        var items = new Dictionary<object, object>();
        var ti = new TenantInfo("test", null, null, null, null);
        var tc = new MultiTenantContext();
        tc.TenantInfo = ti;
        items.Add(Finbuckle.MultiTenant.AspNetCore.Constants.HttpContextMultiTenantContext, tc);
        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.Items).Returns(items);

        var ti2 = new TenantInfo("tenant2", null, null, null, null);

        var res = httpContextMock.Object.TrySetTenantInfo(ti2, false);

        Assert.True(res);
        Assert.Same(ti2, httpContextMock.Object.GetMultiTenantContext().TenantInfo);
    }

    [Fact]
    public void NotSetTenantInfoIfNoMultiTenantContext()
    {
        var items = new Dictionary<object, object>();
        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.Items).Returns(items);

        var ti2 = new TenantInfo("tenant2", null, null, null, null);

        var res = httpContextMock.Object.TrySetTenantInfo(ti2, false);

        Assert.False(res);
        Assert.Null(httpContextMock.Object.GetMultiTenantContext());
    }

    [Fact]
    public void SetStoreInfoAndStrategyInfoNull()
    {
        var items = new Dictionary<object, object>();
        var ti = new TenantInfo("test", null, null, null, null);
        var tc = new MultiTenantContext();
        tc.TenantInfo = ti;
        tc.StrategyInfo = new StrategyInfo();
        tc.StoreInfo = new StoreInfo();
        items.Add(Finbuckle.MultiTenant.AspNetCore.Constants.HttpContextMultiTenantContext, tc);
        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.Items).Returns(items);

        var ti2 = new TenantInfo("tenant2", null, null, null, null);

        var res = httpContextMock.Object.TrySetTenantInfo(ti2, false);

        Assert.Null(httpContextMock.Object.GetMultiTenantContext().StoreInfo);
        Assert.Null(httpContextMock.Object.GetMultiTenantContext().StrategyInfo);
    }

    [Fact]
    public void ResetScopeIfApplicable()
    {
        var items = new Dictionary<object, object>();
        var ti = new TenantInfo("test", null, null, null, null);
        var tc = new MultiTenantContext();
        tc.TenantInfo = ti;
        items.Add(Finbuckle.MultiTenant.AspNetCore.Constants.HttpContextMultiTenantContext, tc);
        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.Items).Returns(items);

        httpContextMock.SetupProperty(c => c.RequestServices);

        var sc = new ServiceCollection();
        sc.AddScoped<object>(_sp => DateTime.Now);
        var sp = sc.BuildServiceProvider();
        httpContextMock.Object.RequestServices = sp;
        
        var ti2 = new TenantInfo("tenant2", null, null, null, null);
        var res = httpContextMock.Object.TrySetTenantInfo(ti2, true);

        Assert.NotSame(sp, httpContextMock.Object.RequestServices);
        Assert.NotStrictEqual<DateTime>((DateTime)sp.GetService<object>(),
            (DateTime)httpContextMock.Object.RequestServices.GetService<object>());
    }

    [Fact]
    public void NotResetScopeIfNotApplicable()
    {
        var items = new Dictionary<object, object>();
        var ti = new TenantInfo("test", null, null, null, null);
        var tc = new MultiTenantContext();
        tc.TenantInfo = ti;
        items.Add(Finbuckle.MultiTenant.AspNetCore.Constants.HttpContextMultiTenantContext, tc);
        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.Items).Returns(items);

        httpContextMock.SetupProperty(c => c.RequestServices);

        var sc = new ServiceCollection();
        sc.AddScoped<object>(_sp => DateTime.Now);
        var sp = sc.BuildServiceProvider();
        httpContextMock.Object.RequestServices = sp;
        
        var ti2 = new TenantInfo("tenant2", null, null, null, null);
        var res = httpContextMock.Object.TrySetTenantInfo(ti2, false);

        Assert.Same(sp, httpContextMock.Object.RequestServices);
        Assert.StrictEqual<DateTime>((DateTime)sp.GetService<object>(),
            (DateTime)httpContextMock.Object.RequestServices.GetService<object>());
    }
}