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
using System.Collections.Generic;
using System.Threading.Tasks;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.AspNetCore;
using Finbuckle.MultiTenant.Stores;
using Finbuckle.MultiTenant.Strategies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class MultiTenantMiddlewareShould
{
    private Mock<HttpContext> CreateHttpContextMock(IServiceProvider serviceProvider)
    {
        var items = new Dictionary<object, object>();

        var mock = new Mock<HttpContext>();
        mock.Setup(c => c.RequestServices).Returns(serviceProvider);
        mock.Setup(c => c.Items).Returns(items);

        return mock;
    }

    [Fact]
    public void ResolveTenant()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant().WithInMemoryStore().WithStaticStrategy("initech");
        var sp = services.BuildServiceProvider();
        var ti = new TenantInfo("initech", "initech", null, null, null);
        sp.GetService<IMultiTenantStore>().TryAddAsync(ti).Wait();

        var context = CreateHttpContextMock(sp).Object;

        var mw = new MultiTenantMiddleware(null);
        mw.Invoke(context).Wait();

        var resolvedTenantContext = (MultiTenantContext)context.Items[Finbuckle.MultiTenant.AspNetCore.Constants.HttpContextMultiTenantContext];
        Assert.NotNull(resolvedTenantContext.TenantInfo);
        Assert.Equal("initech", resolvedTenantContext.TenantInfo.Id);
        Assert.Equal("initech", resolvedTenantContext.TenantInfo.Identifier);
        Assert.NotNull(resolvedTenantContext.TenantInfo.MultiTenantContext);
    }

    [Fact]
    public void SetStrategyInfo()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant().WithInMemoryStore().WithStaticStrategy("initech");
        var sp = services.BuildServiceProvider();
        var ti = new TenantInfo("initech", "initech", null, null, null);
        sp.GetService<IMultiTenantStore>().TryAddAsync(ti).Wait();

        var context = CreateHttpContextMock(sp).Object;

        var mw = new MultiTenantMiddleware(null);
        mw.Invoke(context).Wait();

        var resolvedTenantContext = (MultiTenantContext)context.Items[Finbuckle.MultiTenant.AspNetCore.Constants.HttpContextMultiTenantContext];
        
        Assert.NotNull(resolvedTenantContext.StrategyInfo);
        Assert.NotNull(resolvedTenantContext.StrategyInfo.Strategy);
        Assert.Equal(typeof(StaticStrategy), resolvedTenantContext.StrategyInfo.StrategyType);
        Assert.NotNull(resolvedTenantContext.StrategyInfo.MultiTenantContext);
    }

    [Fact]
    public void SetStoreInfo()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant().WithInMemoryStore().WithStaticStrategy("initech");
        var sp = services.BuildServiceProvider();
        var ti = new TenantInfo("initech", "initech", null, null, null);
        sp.GetService<IMultiTenantStore>().TryAddAsync(ti).Wait();

        var context = CreateHttpContextMock(sp).Object;

        var mw = new MultiTenantMiddleware(null);
        mw.Invoke(context).Wait();

        var resolvedTenantContext = (MultiTenantContext)context.Items[Finbuckle.MultiTenant.AspNetCore.Constants.HttpContextMultiTenantContext];
        
        Assert.NotNull(resolvedTenantContext.StoreInfo);
        Assert.NotNull(resolvedTenantContext.StoreInfo.Store);
        Assert.Equal(typeof(MultiTenantStoreWrapper<InMemoryStore>), resolvedTenantContext.StoreInfo.StoreType);
        Assert.NotNull(resolvedTenantContext.StoreInfo.MultiTenantContext);
    }

    [Fact]
    public void SetContextItemToNullIfNoTenant()
    {
        var services = new ServiceCollection();
        services.AddAuthentication();
        services.AddMultiTenant().WithInMemoryStore().WithStaticStrategy("initech");
        var sp = services.BuildServiceProvider();

        var mock = CreateHttpContextMock(sp);
        var context = mock.Object;

        var mw = new MultiTenantMiddleware(null);
        mw.Invoke(context).Wait();

        var resolveTenantContext = (MultiTenantContext)context.Items[Finbuckle.MultiTenant.AspNetCore.Constants.HttpContextMultiTenantContext];
        Assert.Null(resolveTenantContext);
    }

    internal class NullStrategy : IMultiTenantStrategy
    {
        public async Task<string> GetIdentifierAsync(object context)
        {
           return await Task.FromResult<string>(null);
        }
    }

    [Fact]
    public void HandleRemoteAuthenticationResolutionIfUsingWithRemoteAuthentication()
    {
        var services = new ServiceCollection();
        services.AddAuthentication();
        services.AddMultiTenant().WithInMemoryStore().WithStrategy<NullStrategy>(ServiceLifetime.Singleton).WithRemoteAuthentication();
        
        // Substitute in the mock...
        services.Remove(ServiceDescriptor.Singleton<IRemoteAuthenticationStrategy, RemoteAuthenticationStrategy>());
        var remoteResolverMock = new Mock<RemoteAuthenticationStrategy>();
        services.AddSingleton<IRemoteAuthenticationStrategy>(_sp => remoteResolverMock.Object);
        var sp = services.BuildServiceProvider();

        var mock = CreateHttpContextMock(sp);
        var context = mock.Object;
        
        var mw = new MultiTenantMiddleware(null);
        mw.Invoke(context).Wait();
        remoteResolverMock.Verify(r => r.GetIdentifierAsync(context));
    }

    [Fact]
    public void SkipRemoteAuthenticationResolutionIfNotUsingWithRemoteAuthentication()
    {
        var services = new ServiceCollection();
        services.AddAuthentication();
        services.AddMultiTenant().WithInMemoryStore().WithStrategy<NullStrategy>(ServiceLifetime.Singleton);
        
        // Add in the mock...
        var remoteResolverMock = new Mock<RemoteAuthenticationStrategy>();
        services.AddSingleton<IRemoteAuthenticationStrategy>(_sp => remoteResolverMock.Object);
        var sp = services.BuildServiceProvider();

        var mock = CreateHttpContextMock(sp);
        var context = mock.Object;
        
        var mw = new MultiTenantMiddleware(null);
        mw.Invoke(context).Wait();
        remoteResolverMock.Verify(r => r.GetIdentifierAsync(context), Times.Never);
    }
}