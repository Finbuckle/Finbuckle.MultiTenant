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
using System.Threading;
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

    public class TestStrat2 : DelegateStrategy
    {
        public TestStrat2(Func<object, Task<string>> doStrategy) : base(doStrategy)
        {
        }
    }

    public class TestStrat3 : DelegateStrategy
    {
        public TestStrat3(Func<object, Task<string>> doStrategy) : base(doStrategy)
        {
        }
    }

    public class TestStrat1 : DelegateStrategy
    {
        public TestStrat1(Func<object, Task<string>> doStrategy) : base(doStrategy)
        {
        }
    }

    [Fact]
    public void CycleThroughStrategies()
    {
        var services = new ServiceCollection();
        int count1 = 0;
        int count2 = 0;
        int count3 = 0;
        DateTime time1 = DateTime.Now;
        DateTime time2 = DateTime.Now;
        DateTime time3 = DateTime.Now;

        services.AddMultiTenant().WithInMemoryStore()
            .WithStrategy<TestStrat1>(ServiceLifetime.Singleton, _ =>
            new TestStrat1(c => 
            {
                count1++;
                time1 = DateTime.Now;
                Thread.Sleep(100);
                return Task.FromResult<string>(null);
            }))
            .WithStrategy<TestStrat2>(ServiceLifetime.Singleton, _ =>
            new TestStrat2(c => 
            {
                count2++;
                time2 = DateTime.Now;
                Thread.Sleep(100);
                return Task.FromResult<string>(null);
            }))
            .WithStrategy<TestStrat3>(ServiceLifetime.Singleton, _ =>
            new TestStrat3(c => 
            {
                count3++;
                time3 = DateTime.Now;
                Thread.Sleep(100);
                return Task.FromResult<string>(null);
            }));

        var sp = services.BuildServiceProvider();
        var context = CreateHttpContextMock(sp).Object;

        var mw = new MultiTenantMiddleware(null);
        mw.Invoke(context).Wait();

        Assert.Equal(1, count1);
        Assert.Equal(1, count2);
        Assert.Equal(1, count3);
        Assert.True(time1 < time2 && time2 < time3);
    }

    [Fact]
    public void StopCycleThroughStrategiesWhenOneFound()
    {
        var services = new ServiceCollection();
        int count1 = 0;
        int count2 = 0;
        int count3 = 0;
        DateTime time1 = DateTime.Now;
        DateTime time2 = DateTime.Now;
        DateTime time3 = DateTime.Now;

        services.AddMultiTenant().WithInMemoryStore()
            .WithStrategy<TestStrat1>(ServiceLifetime.Singleton, _ =>
            new TestStrat1(c => 
            {
                count1++;
                time1 = DateTime.Now;
                Thread.Sleep(100);
                return Task.FromResult<string>(null);
            }))
            .WithStrategy<TestStrat2>(ServiceLifetime.Singleton, _ =>
            new TestStrat2(c => 
            {
                count2++;
                time2 = DateTime.Now;
                Thread.Sleep(100);
                return Task.FromResult<string>("found");
            }))
            .WithStrategy<TestStrat3>(ServiceLifetime.Singleton, _ =>
            new TestStrat3(c => 
            {
                count3++;
                time3 = DateTime.Now;
                Thread.Sleep(100);
                return Task.FromResult<string>(null);
            }));

        var sp = services.BuildServiceProvider();
        var context = CreateHttpContextMock(sp).Object;

        var mw = new MultiTenantMiddleware(null);
        mw.Invoke(context).Wait();

        Assert.Equal(1, count1);
        Assert.Equal(1, count2);
        Assert.Equal(0, count3);
        Assert.True(time1 < time2 && time3 < time2);
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

        var resolvedContext = (MultiTenantContext)context.Items[Finbuckle.MultiTenant.AspNetCore.Constants.HttpContextMultiTenantContext];
        
        Assert.NotNull(resolvedContext.StrategyInfo);
        Assert.NotNull(resolvedContext.StrategyInfo.Strategy);

        // Test that the wrapper strategy was "unwrapped"
        Assert.Equal(typeof(StaticStrategy), resolvedContext.StrategyInfo.StrategyType);
        Assert.IsType<StaticStrategy>(resolvedContext.StrategyInfo.Strategy);
        
        Assert.NotNull(resolvedContext.StrategyInfo.MultiTenantContext);
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

        var resolvedContext = (MultiTenantContext)context.Items[Finbuckle.MultiTenant.AspNetCore.Constants.HttpContextMultiTenantContext];
        
        Assert.NotNull(resolvedContext.StoreInfo);
        Assert.NotNull(resolvedContext.StoreInfo.Store);

         // Test that the wrapper store was "unwrapped"
        Assert.Equal(typeof(InMemoryStore), resolvedContext.StoreInfo.StoreType);
        Assert.IsType<InMemoryStore>(resolvedContext.StoreInfo.Store);

        Assert.NotNull(resolvedContext.StoreInfo.MultiTenantContext);
    }

    [Fact]
    public void HandleTenantIdentifierNotFound()
    {
        var services = new ServiceCollection();
        services.AddAuthentication();
        services.AddMultiTenant().WithInMemoryStore().WithDelegateStrategy(o => Task.FromResult<string>(null));
        var sp = services.BuildServiceProvider();

        var mock = CreateHttpContextMock(sp);
        var context = mock.Object;

        var mw = new MultiTenantMiddleware(null);
        mw.Invoke(context).Wait();

        var multiTenantContext = (MultiTenantContext)context.Items[Finbuckle.MultiTenant.AspNetCore.Constants.HttpContextMultiTenantContext];
        Assert.Null(multiTenantContext.TenantInfo);
        Assert.Null(multiTenantContext.StoreInfo);
        Assert.Null(multiTenantContext.StrategyInfo);
    }

    [Fact]
    public void HandleTenantIdentifierNotFoundInStore()
    {
        var services = new ServiceCollection();
        services.AddAuthentication();
        services.AddMultiTenant().WithInMemoryStore().WithStaticStrategy("initech");
        var sp = services.BuildServiceProvider();

        var mock = CreateHttpContextMock(sp);
        var context = mock.Object;

        var mw = new MultiTenantMiddleware(null);
        mw.Invoke(context).Wait();

        var multiTenantContext = (MultiTenantContext)context.Items[Finbuckle.MultiTenant.AspNetCore.Constants.HttpContextMultiTenantContext];
        Assert.Null(multiTenantContext.TenantInfo);
        Assert.Null(multiTenantContext.StoreInfo);
        Assert.NotNull(multiTenantContext.StrategyInfo);
    }

    [Fact]
    public void HandleFallbackStrategy()
    {
        var services = new ServiceCollection();
        services.AddAuthentication();
        services.AddMultiTenant().WithInMemoryStore().WithStaticStrategy("initech").WithFallbackStrategy("default");
        var sp = services.BuildServiceProvider();

        var mock = CreateHttpContextMock(sp);
        var context = mock.Object;

        var mw = new MultiTenantMiddleware(null);
        mw.Invoke(context).Wait();

        var multiTenantContext = (MultiTenantContext)context.Items[Finbuckle.MultiTenant.AspNetCore.Constants.HttpContextMultiTenantContext];
        Assert.Null(multiTenantContext.TenantInfo);
        Assert.Null(multiTenantContext.StoreInfo);
        Assert.NotNull(multiTenantContext.StrategyInfo);
        Assert.IsType<FallbackStrategy>(multiTenantContext.StrategyInfo.Strategy);
    }

    [Fact]
    public void AlwaysHandleFallbackStrategyLast()
    {
        var services = new ServiceCollection();
        services.AddAuthentication();
        services.AddMultiTenant().WithInMemoryStore()
            .WithFallbackStrategy("default")
            .WithDelegateStrategy(async o => await Task.FromResult<string>(null));
        var sp = services.BuildServiceProvider();

        var mock = CreateHttpContextMock(sp);
        var context = mock.Object;

        var mw = new MultiTenantMiddleware(null);
        mw.Invoke(context).Wait();

        var multiTenantContext = (MultiTenantContext)context.Items[Finbuckle.MultiTenant.AspNetCore.Constants.HttpContextMultiTenantContext];
        Assert.Null(multiTenantContext.TenantInfo);
        Assert.Null(multiTenantContext.StoreInfo);
        Assert.NotNull(multiTenantContext.StrategyInfo);
        Assert.IsType<FallbackStrategy>(multiTenantContext.StrategyInfo.Strategy);
    }

    [Fact]
    public void HandleRemoteAuthenticationResolutionIfUsingWithRemoteAuthentication()
    {
        // Create remote strategy mock
        var remoteResolverMock = new Mock<RemoteAuthenticationStrategy>();
        remoteResolverMock.Setup<Task<string>>(o => o.GetIdentifierAsync(It.IsAny<object>())).Returns(Task.FromResult("initech"));

        var services = new ServiceCollection();
        services.AddAuthentication();
        services.AddSingleton<RemoteAuthenticationStrategy>(remoteResolverMock.Object);
        services.AddMultiTenant().WithInMemoryStore().WithDelegateStrategy(o => Task.FromResult<string>(null)).WithRemoteAuthentication();
        
        // Substitute in the mocks...
        var removed = services.Remove(ServiceDescriptor.Singleton<RemoteAuthenticationStrategy, RemoteAuthenticationStrategy>());
        services.AddSingleton<RemoteAuthenticationStrategy>(_sp => remoteResolverMock.Object);
        
        var sp = services.BuildServiceProvider();
        var mock = CreateHttpContextMock(sp);
        var context = mock.Object;

        var mw = new MultiTenantMiddleware(null);
        mw.Invoke(context).Wait();
        remoteResolverMock.Verify(r => r.GetIdentifierAsync(context));

        // Check that the remote strategy was set in the multitenant context.
        Assert.IsAssignableFrom<RemoteAuthenticationStrategy>(context.GetMultiTenantContext().StrategyInfo.Strategy);
    }
}