// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Finbuckle.MultiTenant.AspNetCore.Test
{
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
            store!.TryAddAsync(new TenantInfo { Id = "initech", Identifier = "initech" }).Wait();

            var context = new Mock<HttpContext>();
            context.Setup(c => c.RequestServices).Returns(sp);

            var mw = new MultiTenantMiddleware(_ => {
                Assert.Equal("initech", context.Object.RequestServices.GetService<ITenantInfo>()!.Id);
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
            store!.TryAddAsync(new TenantInfo { Id = "initech", Identifier = "initech" }).Wait();

            var context = new Mock<HttpContext>();
            context.Setup(c => c.RequestServices).Returns(sp);

            var mw = new MultiTenantMiddleware(c => {
                var accessor = c.RequestServices.GetRequiredService<IMultiTenantContextAccessor<TenantInfo>>();
                Assert.NotNull(accessor.MultiTenantContext);
                return Task.CompletedTask;
            });

            await mw.Invoke(context.Object);
        }
    }
}