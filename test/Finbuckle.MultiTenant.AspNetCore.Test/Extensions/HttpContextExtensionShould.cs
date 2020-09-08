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
using Finbuckle.MultiTenant.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class HttpContextExtensionShould
{
    [Fact]
    public void GetTenantContextIfExists()
    {
        var ti = new TenantInfo { Id = "test" };
        var tc = new MultiTenantContext<TenantInfo>();
        tc.TenantInfo = ti;

        var services = new ServiceCollection();
        services.AddScoped<IMultiTenantContextAccessor<TenantInfo>>(_ => new MultiTenantContextAccessor<TenantInfo>{ MultiTenantContext = tc });
        var sp = services.BuildServiceProvider();

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.RequestServices).Returns(sp);

        var mtc = httpContextMock.Object.GetMultiTenantContext<TenantInfo>();

        Assert.Same(tc, mtc);
    }

    [Fact]
    public void ReturnNullIfNoMultiTenantContext()
    {
        var services = new ServiceCollection();
        services.AddScoped<IMultiTenantContextAccessor<TenantInfo>>(_ => new MultiTenantContextAccessor<TenantInfo>());
        var sp = services.BuildServiceProvider();

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.RequestServices).Returns(sp);

        var mtc = httpContextMock.Object.GetMultiTenantContext<TenantInfo>();

        Assert.Null(mtc);
    }

    [Fact]
    public void SetTenantInfo()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>();
        var sp = services.BuildServiceProvider();

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.RequestServices).Returns(sp);
        var context = httpContextMock.Object;

        var ti2 = new TenantInfo { Id = "tenant2" };
        var res = context.TrySetTenantInfo(ti2, false);
        var mtc = context.GetMultiTenantContext<TenantInfo>();
        Assert.True(res);
        Assert.Same(ti2, mtc.TenantInfo);
    }

    [Fact]
    public void SetMultiTenantContextAcccessor()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>();
        var sp = services.BuildServiceProvider();

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.RequestServices).Returns(sp);
        var context = httpContextMock.Object;

        var ti2 = new TenantInfo { Id = "tenant2" };
        var res = context.TrySetTenantInfo(ti2, false);
        var mtc = context.GetMultiTenantContext<TenantInfo>();
        var accessor = context.RequestServices.GetRequiredService<IMultiTenantContextAccessor<TenantInfo>>();
        Assert.True(res);
        Assert.Same(mtc, accessor.MultiTenantContext);
    }

    [Fact]
    public void SetStoreInfoAndStrategyInfoNull()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>();
        var sp = services.BuildServiceProvider();

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.RequestServices).Returns(sp);
        var context = httpContextMock.Object;

        var ti2 = new TenantInfo { Id = "tenant2" };
        var res = context.TrySetTenantInfo(ti2, false);
        var mtc = context.GetMultiTenantContext<TenantInfo>();

        Assert.Null(mtc.StoreInfo);
        Assert.Null(mtc.StrategyInfo);
    }

    [Fact]
    public void ResetScopeIfApplicable()
    {
        var httpContextMock = new Mock<HttpContext>();

        httpContextMock.SetupProperty(c => c.RequestServices);

        var services = new ServiceCollection();
        services.AddScoped<object>(_sp => DateTime.Now);
        services.AddMultiTenant<TenantInfo>();
        var sp = services.BuildServiceProvider();
        httpContextMock.Object.RequestServices = sp;

        var ti2 = new TenantInfo { Id = "tenant2" };
        var res = httpContextMock.Object.TrySetTenantInfo(ti2, true);

        Assert.NotSame(sp, httpContextMock.Object.RequestServices);
        Assert.NotStrictEqual<DateTime>((DateTime)sp.GetService<object>(),
            (DateTime)httpContextMock.Object.RequestServices.GetService<object>());
    }

    [Fact]
    public void NotResetScopeIfNotApplicable()
    {
        var httpContextMock = new Mock<HttpContext>();

        httpContextMock.SetupProperty(c => c.RequestServices);

        var services = new ServiceCollection();
        services.AddScoped<object>(_sp => DateTime.Now);
        services.AddMultiTenant<TenantInfo>();
        var sp = services.BuildServiceProvider();
        httpContextMock.Object.RequestServices = sp;

        var ti2 = new TenantInfo { Id = "tenant2" };
        var res = httpContextMock.Object.TrySetTenantInfo(ti2, false);

        Assert.Same(sp, httpContextMock.Object.RequestServices);
        Assert.StrictEqual<DateTime>((DateTime)sp.GetService<object>(),
            (DateTime)httpContextMock.Object.RequestServices.GetService<object>());
    }
}