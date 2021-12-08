// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Finbuckle.MultiTenant.Test.Options
{
    public class MultiTenantOptionsFactoryShould
    {
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("name")]
        public void CreateOptionsWithTenantAction(string name)
        {
            var services = new ServiceCollection();
            services.AddOptions();
            services.Configure<TestOptions>(name, o => o.DefaultConnectionString = $"{name}_begin");
            services.PostConfigure<TestOptions>(name, o => o.DefaultConnectionString += "end");
            services.AddMultiTenant<TenantInfo>().WithPerTenantOptions<TestOptions>((o, ti) => o.DefaultConnectionString += $"_{ti.Id}_");
            var sp = services.BuildServiceProvider();
            var accessor = sp.GetRequiredService<IMultiTenantContextAccessor<TenantInfo>>();
            accessor.MultiTenantContext = new MultiTenantContext<TenantInfo> { TenantInfo = new TenantInfo { Id = "test-id-123" } };

            var options = sp.GetRequiredService<IOptionsSnapshot<TestOptions>>().Get(name);
            Assert.Equal($"{name}_begin_{accessor.MultiTenantContext.TenantInfo!.Id}_end", options.DefaultConnectionString);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("name")]
        public void CreateMultipelOptionsWithTenantAction(string name)
        {
            var services = new ServiceCollection();
            services.AddOptions();
            services.Configure<TestOptions>(name, o => o.DefaultConnectionString = $"{name}_begin");
            services.PostConfigure<TestOptions>(name, o => o.DefaultConnectionString += "end");
            services.AddMultiTenant<TenantInfo>()
                .WithPerTenantOptions<TestOptions>((o, ti) => o.DefaultConnectionString += $"_{ti.Id}")
                .WithPerTenantOptions<TestOptions>((o, ti) => o.DefaultConnectionString += $"_{ti.Identifier}_");
            var sp = services.BuildServiceProvider();
            var accessor = sp.GetRequiredService<IMultiTenantContextAccessor<TenantInfo>>();
            accessor.MultiTenantContext = new MultiTenantContext<TenantInfo> { TenantInfo = new TenantInfo { Id = "id", Identifier = "identifier" } };

            var options = sp.GetRequiredService<IOptionsSnapshot<TestOptions>>().Get(name);
            Assert.Equal($"{name}_begin_{accessor.MultiTenantContext.TenantInfo!.Id}_{accessor.MultiTenantContext.TenantInfo.Identifier}_end", options.DefaultConnectionString);
        }

        [Fact]
        public void IgnoreNullTenantInfo()
        {
            var services = new ServiceCollection();
            services.Configure<TestOptions>(o => o.DefaultConnectionString = "begin");
            services.PostConfigure<TestOptions>(o => o.DefaultConnectionString += "end");
            services.AddMultiTenant<TenantInfo>().WithPerTenantOptions<TestOptions>((o, ti) => o.DefaultConnectionString += $"_{ti.Id}_");
            var sp = services.BuildServiceProvider();
            var accessor = sp.GetRequiredService<IMultiTenantContextAccessor<TenantInfo>>();
            accessor.MultiTenantContext = new MultiTenantContext<TenantInfo>();

            var options = sp.GetRequiredService<IOptionsSnapshot<TestOptions>>().Value;
            Assert.Equal($"beginend", options.DefaultConnectionString);
        }

        [Fact]
        public void IgnoreNullMultiTenantContext()
        {
            var services = new ServiceCollection();
            services.Configure<TestOptions>(o => o.DefaultConnectionString = "begin");
            services.PostConfigure<TestOptions>(o => o.DefaultConnectionString += "end");
            services.AddMultiTenant<TenantInfo>().WithPerTenantOptions<TestOptions>((o, ti) => o.DefaultConnectionString += $"_{ti.Id}_");
            var sp = services.BuildServiceProvider();
            var accessor = sp.GetRequiredService<IMultiTenantContextAccessor<TenantInfo>>();
            accessor.MultiTenantContext = null;

            var options = sp.GetRequiredService<IOptionsSnapshot<TestOptions>>().Value;
            Assert.Equal($"beginend", options.DefaultConnectionString);
        }

        [Fact]
        public void ValidateOptions()
        {
            var services = new ServiceCollection();
            services.AddOptions<TestOptions>()
                .Configure(o => o.DefaultConnectionString = "begin")
                .ValidateDataAnnotations();
            services.Configure<TestOptions>(o => o.DefaultConnectionString = "begin");
            services.AddMultiTenant<TenantInfo>().WithPerTenantOptions<TestOptions>((o, ti) => o.DefaultConnectionString = null);
            var sp = services.BuildServiceProvider();
            var accessor = sp.GetRequiredService<IMultiTenantContextAccessor<TenantInfo>>();
            accessor.MultiTenantContext = new MultiTenantContext<TenantInfo> { TenantInfo = new TenantInfo { Id = "id", Identifier = "identifier" } };

            Assert.Throws<OptionsValidationException>(() => sp.GetRequiredService<IOptionsSnapshot<TestOptions>>().Value);
        }
    }
}