//    Copyright 2018-2020 Finbuckle LLC, Andrew White, and Contributors
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
    [Fact]
    async void UseResolver()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>().
            WithStaticStrategy("initech").
            WithInMemoryStore();
        var sp = services.BuildServiceProvider();
        var store = sp.GetService<IMultiTenantStore<TenantInfo>>();
        store.TryAddAsync(new TenantInfo { Id = "initech", Identifier = "initech" }).Wait();

        var context = new Mock<HttpContext>();
        context.Setup(c => c.RequestServices).Returns(sp);

        var mw = new MultiTenantMiddleware(c => {
            Assert.Equal("initech", context.Object.RequestServices.GetService<ITenantInfo>().Id);
            return Task.CompletedTask;
        });

        await mw.Invoke(context.Object);
    }

    [Fact]
    async void SetMultiTenantContextAccessor()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>().
            WithStaticStrategy("initech").
            WithInMemoryStore();
        var sp = services.BuildServiceProvider();
        var store = sp.GetService<IMultiTenantStore<TenantInfo>>();
        store.TryAddAsync(new TenantInfo { Id = "initech", Identifier = "initech" }).Wait();

        var context = new Mock<HttpContext>();
        context.Setup(c => c.RequestServices).Returns(sp);

        var mw = new MultiTenantMiddleware(c => {
            var accessor = c.RequestServices.GetRequiredService<IMultiTenantContextAccessor<TenantInfo>>();
            var resolver = c.RequestServices.GetRequiredService<ITenantResolver<TenantInfo>>();
            Assert.NotNull(accessor.MultiTenantContext);
            return Task.CompletedTask;
        });

        await mw.Invoke(context.Object);        
    }
}