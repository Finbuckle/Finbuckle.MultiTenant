// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.AspNetCore.Internal;
using Finbuckle.MultiTenant.AspNetCore.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Finbuckle.MultiTenant.AspNetCore.Test;

public class MultiTenantMiddlewareShould
{
    [Fact]
    public async void SetHttpContextItemIfTenantFound()
    {
            var services = new ServiceCollection();
            services.AddMultiTenant<TenantInfo>().
                WithStaticStrategy("initech").
                WithInMemoryStore();
            var sp = services.BuildServiceProvider();
            var store = sp.GetRequiredService<IMultiTenantStore<TenantInfo>>();
            await store.TryAddAsync(new TenantInfo { Id = "initech", Identifier = "initech" });

            var context = new Mock<HttpContext>();
            context.Setup(c => c.RequestServices).Returns(sp);
            context.Setup(c => c.Features).Returns(new FeatureCollection());

            var itemsDict = new Dictionary<object, object?>();
            context.Setup(c => c.Items).Returns(itemsDict);

            var mw = new MultiTenantMiddleware(_ => Task.CompletedTask);

            await mw.Invoke(context.Object);

            var mtc = (IMultiTenantContext<TenantInfo>?)context.Object.Items[typeof(IMultiTenantContext)];

            Assert.NotNull(mtc?.TenantInfo);
            Assert.Equal("initech", mtc.TenantInfo.Id);
        }

    [Fact]
    public async void NotShortCircuitIfTenantFound()
    {
            var services = new ServiceCollection();
            services.AddMultiTenant<TenantInfo>().
                WithStaticStrategy("initech").
                WithInMemoryStore();
            var sp = services.BuildServiceProvider();
            var store = sp.GetRequiredService<IMultiTenantStore<TenantInfo>>();
            await store.TryAddAsync(new TenantInfo { Id = "initech", Identifier = "initech" });

            var context = new Mock<HttpContext>();
            context.Setup(c => c.RequestServices).Returns(sp);
            context.Setup(c => c.Features).Returns(new FeatureCollection());

            var itemsDict = new Dictionary<object, object?>();
            context.Setup(c => c.Items).Returns(itemsDict);

            var options = new ShortCircuitWhenOptions { Predicate = context => !context.IsResolved };
            var optionsMock = new Mock<IOptions<ShortCircuitWhenOptions>>();
            optionsMock.Setup(c => c.Value).Returns(options);

            var calledNext = false;
            var mw = new MultiTenantMiddleware(_ =>
            {
                calledNext = true;

                return Task.CompletedTask;
            }, optionsMock.Object);

            await mw.Invoke(context.Object);

            var mtc = (IMultiTenantContext<TenantInfo>?)context.Object.Items[typeof(IMultiTenantContext)];

            Assert.NotNull(mtc?.TenantInfo);
            Assert.Equal("initech", mtc.TenantInfo.Id);
            Assert.True(calledNext);
        }

    [Fact]
    public async void SetTenantAccessor()
    {
            var services = new ServiceCollection();
            services.AddMultiTenant<TenantInfo>().
                WithStaticStrategy("initech").
                WithInMemoryStore();
            var sp = services.BuildServiceProvider();
            var store = sp.GetRequiredService<IMultiTenantStore<TenantInfo>>();
            await store.TryAddAsync(new TenantInfo { Id = "initech", Identifier = "initech" });

            var context = new Mock<HttpContext>();
            context.Setup(c => c.RequestServices).Returns(sp);
            context.Setup(c => c.Features).Returns(new FeatureCollection());

            var itemsDict = new Dictionary<object, object?>();
            context.Setup(c => c.Items).Returns(itemsDict);
            
            IMultiTenantContext<TenantInfo>? mtc = null;
            
            var mw = new MultiTenantMiddleware(httpContext =>
            {
                // have to check in this Async chain...
                var accessor = context.Object.RequestServices.GetRequiredService<IMultiTenantContextAccessor<TenantInfo>>();
                mtc = accessor.MultiTenantContext;

                return Task.CompletedTask;
            });

            await mw.Invoke(context.Object);
            
            Assert.NotNull(mtc);
            Assert.True(mtc.IsResolved);
            Assert.NotNull(mtc.TenantInfo);
        }
        
    [Fact]
    public async void NotSetTenantAccessorIfNoTenant()
    {
            var services = new ServiceCollection();
            services.AddMultiTenant<TenantInfo>().
                WithStaticStrategy("not_initech").
                WithInMemoryStore();
            var sp = services.BuildServiceProvider();
            var store = sp.GetRequiredService<IMultiTenantStore<TenantInfo>>();
            await store.TryAddAsync(new TenantInfo { Id = "initech", Identifier = "initech" });

            var context = new Mock<HttpContext>();
            context.Setup(c => c.RequestServices).Returns(sp);
            context.Setup(c => c.Features).Returns(new FeatureCollection());

            var itemsDict = new Dictionary<object, object?>();
            context.Setup(c => c.Items).Returns(itemsDict);

            IMultiTenantContext<TenantInfo>? mtc = null;
            
            var mw = new MultiTenantMiddleware(httpContext =>
            {
                // have to check in this Async chain...
                var accessor = context.Object.RequestServices.GetRequiredService<IMultiTenantContextAccessor<TenantInfo>>();
                mtc = accessor.MultiTenantContext;

                return Task.CompletedTask;
            });

            await mw.Invoke(context.Object);

            Assert.NotNull(mtc);
            Assert.False(mtc.IsResolved);
            Assert.Null(mtc.TenantInfo);
        }

    [Fact]
    public async void ShortCircuitIfNoTenant()
    {
            var services = new ServiceCollection();
            services.AddMultiTenant<TenantInfo>().
                WithStaticStrategy("not_initech").
                WithInMemoryStore();
            var sp = services.BuildServiceProvider();
            var store = sp.GetRequiredService<IMultiTenantStore<TenantInfo>>();
            await store.TryAddAsync(new TenantInfo { Id = "initech", Identifier = "initech" });

            var context = new Mock<HttpContext>();
            context.Setup(c => c.RequestServices).Returns(sp);
            context.Setup(c => c.Features).Returns(new FeatureCollection());

            var itemsDict = new Dictionary<object, object?>();
            context.Setup(c => c.Items).Returns(itemsDict);

            var options = new ShortCircuitWhenOptions { Predicate = context => !context.IsResolved };
            var optionsMock = new Mock<IOptions<ShortCircuitWhenOptions>>();
            optionsMock.Setup(c => c.Value).Returns(options);

            var calledNext = false;
            var mw = new MultiTenantMiddleware(_ =>
            {
                calledNext = true;

                return Task.CompletedTask;
            }, optionsMock.Object);

            await mw.Invoke(context.Object);

            var mtc = (IMultiTenantContext<TenantInfo>?)context.Object.Items[typeof(IMultiTenantContext)];

            Assert.NotNull(mtc);
            Assert.False(mtc.IsResolved);
            Assert.Null(mtc.TenantInfo);
            Assert.False(calledNext);
        }
}