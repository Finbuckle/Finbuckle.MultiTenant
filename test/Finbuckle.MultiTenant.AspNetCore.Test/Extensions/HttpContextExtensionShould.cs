// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;
using Finbuckle.MultiTenant.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Finbuckle.MultiTenant.AspNetCore.Test.Extensions
{
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
            context.TrySetTenantInfo(ti2, false);
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
            services.AddScoped<object>(_ => DateTime.Now);
            services.AddMultiTenant<TenantInfo>();
            var sp = services.BuildServiceProvider();
            httpContextMock.Object.RequestServices = sp;

            var ti2 = new TenantInfo { Id = "tenant2" };
            httpContextMock.Object.TrySetTenantInfo(ti2, true);

            Assert.NotSame(sp, httpContextMock.Object.RequestServices);
            Assert.NotStrictEqual((DateTime)sp.GetService<object>(),
                (DateTime)httpContextMock.Object.RequestServices.GetService<object>());
        }

        [Fact]
        public void NotResetScopeIfNotApplicable()
        {
            var httpContextMock = new Mock<HttpContext>();

            httpContextMock.SetupProperty(c => c.RequestServices);

            var services = new ServiceCollection();
            services.AddScoped<object>(_ => DateTime.Now);
            services.AddMultiTenant<TenantInfo>();
            var sp = services.BuildServiceProvider();
            httpContextMock.Object.RequestServices = sp;

            var ti2 = new TenantInfo { Id = "tenant2" };
            httpContextMock.Object.TrySetTenantInfo(ti2, false);

            Assert.Same(sp, httpContextMock.Object.RequestServices);
            Assert.StrictEqual((DateTime)sp.GetService<object>(),
                (DateTime)httpContextMock.Object.RequestServices.GetService<object>());
        }
    }
}