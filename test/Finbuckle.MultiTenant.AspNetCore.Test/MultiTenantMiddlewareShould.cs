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
        var tc = new TenantContext("initech", "initech", null, null, null, null);
        sp.GetService<IMultiTenantStore>().TryAdd(tc);

        var context = CreateHttpContextMock(sp).Object;

        var mw = new MultiTenantMiddleware(null);
        mw.Invoke(context).Wait();

        var resolveTenantContext = (TenantContext)context.Items[Finbuckle.MultiTenant.AspNetCore.Constants.HttpContextTenantContext];
        Assert.Equal("initech", resolveTenantContext.Id);
    }

    [Fact]
    public void SetStrategyType()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant().WithInMemoryStore().WithStaticStrategy("initech");
        var sp = services.BuildServiceProvider();
        var tc = new TenantContext("initech", "initech", null, null, null, null);
        sp.GetService<IMultiTenantStore>().TryAdd(tc);

        var context = CreateHttpContextMock(sp).Object;

        var mw = new MultiTenantMiddleware(null);
        mw.Invoke(context).Wait();

        var resolveTenantContext = (TenantContext)context.Items[Finbuckle.MultiTenant.AspNetCore.Constants.HttpContextTenantContext];
        Assert.Equal("initech", resolveTenantContext.Id);
        Assert.Equal(typeof(StaticMultiTenantStrategy), resolveTenantContext.MultiTenantStrategyType);
    }

    [Fact]
    public void SetStoreType()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant().WithInMemoryStore().WithStaticStrategy("initech");
        var sp = services.BuildServiceProvider();
        var tc = new TenantContext("initech", "initech", null, null, null, null);
        sp.GetService<IMultiTenantStore>().TryAdd(tc);

        var context = CreateHttpContextMock(sp).Object;

        var mw = new MultiTenantMiddleware(null);
        mw.Invoke(context).Wait();

        var resolveTenantContext = (TenantContext)context.Items[Finbuckle.MultiTenant.AspNetCore.Constants.HttpContextTenantContext];
        Assert.Equal("initech", resolveTenantContext.Id);
        Assert.Equal(typeof(InMemoryMultiTenantStore), resolveTenantContext.MultiTenantStoreType);
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

        var resolveTenantContext = (TenantContext)context.Items[Finbuckle.MultiTenant.AspNetCore.Constants.HttpContextTenantContext];
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
        services.AddMultiTenant().WithInMemoryStore().WithStrategy<NullStrategy>().WithRemoteAuthentication();
        
        // Substitute in the mock...
        services.Remove(ServiceDescriptor.Singleton<IRemoteAuthenticationMultiTenantStrategy, RemoteAuthenticationMultiTenantStrategy>());
        var remoteResolverMock = new Mock<RemoteAuthenticationMultiTenantStrategy>();
        services.AddSingleton<IRemoteAuthenticationMultiTenantStrategy>(_sp => remoteResolverMock.Object);
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
        services.AddMultiTenant().WithInMemoryStore().WithStrategy<NullStrategy>();
        
        // Add in the mock...
        var remoteResolverMock = new Mock<RemoteAuthenticationMultiTenantStrategy>();
        services.AddSingleton<IRemoteAuthenticationMultiTenantStrategy>(_sp => remoteResolverMock.Object);
        var sp = services.BuildServiceProvider();

        var mock = CreateHttpContextMock(sp);
        var context = mock.Object;
        
        var mw = new MultiTenantMiddleware(null);
        mw.Invoke(context).Wait();
        remoteResolverMock.Verify(r => r.GetIdentifierAsync(context), Times.Never);
    }
}