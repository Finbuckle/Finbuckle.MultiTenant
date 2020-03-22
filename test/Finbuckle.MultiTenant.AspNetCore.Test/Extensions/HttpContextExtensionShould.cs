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
using Finbuckle.MultiTenant.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class HttpContextExtensionShould
{
    private Mock<HttpContext> CreateHttpContextMock(IServiceProvider serviceProvider)
    {
        var items = new Dictionary<object, object>();

        var mock = new Mock<HttpContext>();
        mock.SetupProperty(c => c.RequestServices);
        mock.Object.RequestServices = serviceProvider;

        return mock;
    }
    
    [Fact]
    public void GetTenantContextIfExists()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant().WithInMemoryStore().WithStaticStrategy("initech");
        var sp = services.BuildServiceProvider();
        var ti = new TenantInfo("initech", "initech", null, null, null);
        sp.GetService<IMultiTenantStore>().TryAddAsync(ti).Wait();

        var context = CreateHttpContextMock(sp).Object;

        var mw = new MultiTenantMiddleware(null);
        mw.Invoke(context).Wait();

        var resolvedContext = context.GetMultiTenantContext();

        Assert.NotNull(resolvedContext);
        Assert.Same(ti, resolvedContext.TenantInfo);
    }

    [Fact]
    public void ReturnNullIfNoMultiTenantContext()
    {
        var services = new ServiceCollection();
        var sp = services.BuildServiceProvider();

        var context = CreateHttpContextMock(sp).Object;
        var resolvedContext = context.GetMultiTenantContext();

        Assert.Null(resolvedContext);
    }

    [Fact]
    public void SetTenantInfo()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant().WithInMemoryStore().WithStaticStrategy("initech");
        var sp = services.BuildServiceProvider();
        var ti = new TenantInfo("initech", "initech", null, null, null);
        sp.GetService<IMultiTenantStore>().TryAddAsync(ti).Wait();

        var context = CreateHttpContextMock(sp).Object;

        var mw = new MultiTenantMiddleware(null);
        mw.Invoke(context).Wait();

        var ti2 = new TenantInfo("tenant2", null, null, null, null);

        var result = context.TrySetTenantInfo(ti2, false);

        Assert.True(result);
        Assert.Same(context.RequestServices.GetService<IMultiTenantContext>(), ti2.MultiTenantContext);
        Assert.Same(ti2, context.GetMultiTenantContext().TenantInfo);
    }

    [Fact]
    public void NotSetTenantInfoIfNoMultiTenantContext()
    {
        var services = new ServiceCollection();
        var sp = services.BuildServiceProvider();

        var context = CreateHttpContextMock(sp).Object;

        var ti2 = new TenantInfo("tenant2", null, null, null, null);

        var result = context.TrySetTenantInfo(ti2, false);

        Assert.False(result);
    }

    [Fact]
    public void SetStoreInfoAndStrategyInfoNullWhenSettingTenantInfo()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant().WithInMemoryStore().WithStaticStrategy("initech");
        var sp = services.BuildServiceProvider();
        var ti = new TenantInfo("initech", "initech", null, null, null);
        sp.GetService<IMultiTenantStore>().TryAddAsync(ti).Wait();

        var context = CreateHttpContextMock(sp).Object;

        var mw = new MultiTenantMiddleware(null);
        mw.Invoke(context).Wait();

        Assert.NotNull(context.GetMultiTenantContext().StoreInfo);
        Assert.NotNull(context.GetMultiTenantContext().StrategyInfo);

        var ti2 = new TenantInfo("tenant2", null, null, null, null);
        context.TrySetTenantInfo(ti2, false);

        Assert.Null(context.GetMultiTenantContext().StoreInfo);
        Assert.Null(context.GetMultiTenantContext().StrategyInfo);
    }

    [Fact]
    public void ResetScopeIfApplicable()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant().WithInMemoryStore().WithStaticStrategy("initech");
        var sp = services.BuildServiceProvider();
        var ti = new TenantInfo("initech", "initech", null, null, null);
        sp.GetService<IMultiTenantStore>().TryAddAsync(ti).Wait();

        var context = CreateHttpContextMock(sp).Object;

        var mw = new MultiTenantMiddleware(null);
        mw.Invoke(context).Wait();

        var originalSp = context.RequestServices;

        var ti2 = new TenantInfo("tenant2", null, null, null, null);
        var result = context.TrySetTenantInfo(ti2, true);

        Assert.NotSame(originalSp, context.RequestServices);
    }

    [Fact]
    public void NotResetScopeIfNotApplicable()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant().WithInMemoryStore().WithStaticStrategy("initech");
        var sp = services.BuildServiceProvider();
        var ti = new TenantInfo("initech", "initech", null, null, null);
        sp.GetService<IMultiTenantStore>().TryAddAsync(ti).Wait();

        var context = CreateHttpContextMock(sp).Object;

        var mw = new MultiTenantMiddleware(null);
        mw.Invoke(context).Wait();

        var originalSp = context.RequestServices;

        var ti2 = new TenantInfo("tenant2", null, null, null, null);
        var result = context.TrySetTenantInfo(ti2, false);

        Assert.Same(originalSp, context.RequestServices);
    }
}