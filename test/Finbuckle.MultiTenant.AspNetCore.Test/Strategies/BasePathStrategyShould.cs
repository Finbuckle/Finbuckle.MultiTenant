// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;
using Finbuckle.MultiTenant.AspNetCore.Options;
using Finbuckle.MultiTenant.Strategies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Finbuckle.MultiTenant.AspNetCore.Test.Strategies
{
    public class BasePathStrategyShould
    {
        private HttpContext CreateHttpContextMock(string path)
        {
            var mock = new Mock<HttpContext>();
            mock.SetupProperty<PathString>(c => c.Request.Path, path);
            mock.SetupProperty<PathString>(c => c.Request.PathBase, "/");
            mock.SetupProperty(c => c.RequestServices);
            return mock.Object;
        }

        [Fact]
        public async void RebaseAspNetCoreBasePathIfOptionTrue()
        {

            var services = new ServiceCollection();
            services.AddOptions().AddMultiTenant<TenantInfo>().WithBasePathStrategy().WithInMemoryStore(options =>
            {
                options.Tenants.Add(new TenantInfo
                {
                    Id = "base123",
                    Identifier = "base",
                    Name = "base tenant"
                });
            });
            services.Configure<BasePathStrategyOptions>(options => options.RebaseAspNetCorePathBase = true);
            var serviceProvider = services.BuildServiceProvider();
            var httpContext = CreateHttpContextMock("/base/notbase");
            httpContext.RequestServices = serviceProvider;

            Assert.Equal("/", httpContext.Request.PathBase);
            Assert.Equal("/base/notbase", httpContext.Request.Path);
            
            // will trigger OnTenantFound event...
            var resolver = await serviceProvider.GetRequiredService<ITenantResolver>().ResolveAsync(httpContext);

            Assert.Equal("/base", httpContext.Request.PathBase);
            Assert.Equal("/notbase", httpContext.Request.Path);
        }
        
        [Fact]
        public async void NotRebaseAspNetCoreBasePathIfOptionFalse()
        {

            var services = new ServiceCollection();
            services.AddOptions().AddMultiTenant<TenantInfo>().WithBasePathStrategy().WithInMemoryStore(options =>
            {
                options.Tenants.Add(new TenantInfo
                {
                    Id = "base123",
                    Identifier = "base",
                    Name = "base tenant"
                });
            });
            services.Configure<BasePathStrategyOptions>(options => options.RebaseAspNetCorePathBase = false);
            var serviceProvider = services.BuildServiceProvider();
            var httpContext = CreateHttpContextMock("/base/notbase");
            httpContext.RequestServices = serviceProvider;

            Assert.Equal("/", httpContext.Request.PathBase);
            Assert.Equal("/base/notbase", httpContext.Request.Path);
            
            // will trigger OnTenantFound event...
            var resolver = await serviceProvider.GetRequiredService<ITenantResolver>().ResolveAsync(httpContext);

            Assert.Equal("/", httpContext.Request.PathBase);
            Assert.Equal("/base/notbase", httpContext.Request.Path);
        }

        [Theory]
        [InlineData("/test", "test")] // single path
        [InlineData("/Test", "Test")] // maintain case
        [InlineData("", null)] // no path
        [InlineData("/", null)] // just trailing slash
        [InlineData("/initech/ignore/ignore", "initech")] // multiple path segments
        public async void ReturnExpectedIdentifier(string path, string expected)
        {
            var httpContext = CreateHttpContextMock(path);
            var strategy = new BasePathStrategy();

            var identifier = await strategy.GetIdentifierAsync(httpContext);

            Assert.Equal(expected, identifier);
        }

        [Fact]
        public async void ThrowIfContextIsNotHttpContext()
        {
            var context = new Object();
            var strategy = new BasePathStrategy();

            await Assert.ThrowsAsync<MultiTenantException>(() => strategy.GetIdentifierAsync(context));
        }
    }
}