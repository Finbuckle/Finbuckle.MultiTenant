// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

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
            services.AddMultiTenant<TenantInfo>()
                .WithPerTenantOptions<TestOptions>((o, ti) => o.DefaultConnectionString += $"_{ti.Id}_");
            var sp = services.BuildServiceProvider();
            var accessor = sp.GetRequiredService<IMultiTenantContextAccessor<TenantInfo>>();
            accessor.MultiTenantContext = new MultiTenantContext<TenantInfo>
                { TenantInfo = new TenantInfo { Id = "test-id-123" } };

            var options = sp.GetRequiredService<IOptionsSnapshot<TestOptions>>().Get(name);
            Assert.Equal($"{name}_begin_{accessor.MultiTenantContext.TenantInfo!.Id}_end",
                options.DefaultConnectionString);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("name")]
        public void CreateMultipleOptionsWithTenantAction(string name)
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
            accessor.MultiTenantContext = new MultiTenantContext<TenantInfo>
                { TenantInfo = new TenantInfo { Id = "id", Identifier = "identifier" } };

            var options = sp.GetRequiredService<IOptionsSnapshot<TestOptions>>().Get(name);
            Assert.Equal(
                $"{name}_begin_{accessor.MultiTenantContext.TenantInfo!.Id}_{accessor.MultiTenantContext.TenantInfo.Identifier}_end",
                options.DefaultConnectionString);
        }

        [Theory]
        [InlineData("", "name2")]
        [InlineData("name1", "name2")]
        [InlineData("name1", "")]
        public void CreateMultipleNamedOptionsWithTenantAction(string name1, string name2)
        {
            var services = new ServiceCollection();
            services.AddOptions();
            services.Configure<TestOptions>(name1, o => o.DefaultConnectionString = $"{name1}_begin");
            services.Configure<TestOptions>(name2, o => o.DefaultConnectionString = $"{name2}_begin");
            services.PostConfigure<TestOptions>(name1, o => o.DefaultConnectionString += "end");
            services.PostConfigure<TestOptions>(name2, o => o.DefaultConnectionString += "end");
            services.AddMultiTenant<TenantInfo>()
                //configure non-named options
                .WithPerTenantOptions<TestOptions>((o, ti) => o.DefaultConnectionString += "_noname")
                //configure named options
                .WithPerTenantNamedOptions<TestOptions>(name1,
                    (o, ti) => o.DefaultConnectionString += $"_{name1}_")
                .WithPerTenantNamedOptions<TestOptions>(name2,
                    (o, ti) => o.DefaultConnectionString += $"_{name2}_");
            var sp = services.BuildServiceProvider();
            var accessor = sp.GetRequiredService<IMultiTenantContextAccessor<TenantInfo>>();
            accessor.MultiTenantContext = new MultiTenantContext<TenantInfo>
                { TenantInfo = new TenantInfo { Id = "id", Identifier = "identifier" } };

            var options1 = sp.GetRequiredService<IOptionsSnapshot<TestOptions>>().Get(name1);
            var expectedName1 = !string.IsNullOrEmpty(name1) ? name1 : Microsoft.Extensions.Options.Options.DefaultName;
            Assert.Equal(
                $"{expectedName1}_begin_noname_{expectedName1}_end",
                options1.DefaultConnectionString);

            var options2 = sp.GetRequiredService<IOptionsSnapshot<TestOptions>>().Get(name2);
            var expectedName2 = !string.IsNullOrEmpty(name2) ? name2 : Microsoft.Extensions.Options.Options.DefaultName;
            Assert.Equal(
                $"{expectedName2}_begin_noname_{expectedName2}_end",
                options2.DefaultConnectionString);
        }

        [Fact]
        public void IgnoreNullTenantInfo()
        {
            var services = new ServiceCollection();
            services.Configure<TestOptions>(o => o.DefaultConnectionString = "begin");
            services.PostConfigure<TestOptions>(o => o.DefaultConnectionString += "End");
            services.AddMultiTenant<TenantInfo>()
                .WithPerTenantOptions<TestOptions>((o, ti) => o.DefaultConnectionString += $"_{ti.Id}_");
            var sp = services.BuildServiceProvider();
            var accessor = sp.GetRequiredService<IMultiTenantContextAccessor<TenantInfo>>();
            accessor.MultiTenantContext = new MultiTenantContext<TenantInfo>();

            var options = sp.GetRequiredService<IOptionsSnapshot<TestOptions>>().Value;
            Assert.Equal($"beginEnd", options.DefaultConnectionString);
        }

        [Fact]
        public void IgnoreNullMultiTenantContext()
        {
            var services = new ServiceCollection();
            services.Configure<TestOptions>(o => o.DefaultConnectionString = "begin");
            services.PostConfigure<TestOptions>(o => o.DefaultConnectionString += "End");
            services.AddMultiTenant<TenantInfo>()
                .WithPerTenantOptions<TestOptions>((o, ti) => o.DefaultConnectionString += $"_{ti.Id}_");
            var sp = services.BuildServiceProvider();
            var accessor = sp.GetRequiredService<IMultiTenantContextAccessor<TenantInfo>>();
            accessor.MultiTenantContext = null;

            var options = sp.GetRequiredService<IOptionsSnapshot<TestOptions>>().Value;
            Assert.Equal($"beginEnd", options.DefaultConnectionString);
        }

        [Fact]
        public void ValidateOptions()
        {
            var services = new ServiceCollection();
            services.AddOptions<TestOptions>()
                .Configure(o => o.DefaultConnectionString = "begin")
                .ValidateDataAnnotations();
            services.AddMultiTenant<TenantInfo>()
                .WithPerTenantOptions<TestOptions>((o, ti) => o.DefaultConnectionString = null);
            var sp = services.BuildServiceProvider();
            var accessor = sp.GetRequiredService<IMultiTenantContextAccessor<TenantInfo>>();
            accessor.MultiTenantContext = new MultiTenantContext<TenantInfo>
                { TenantInfo = new TenantInfo { Id = "id", Identifier = "identifier" } };

            Assert.Throws<OptionsValidationException>(
                () => sp.GetRequiredService<IOptionsSnapshot<TestOptions>>().Value);
        }

        [Fact]
        public void ValidateNamedOptions()
        {
            var services = new ServiceCollection();
            services.AddOptions<TestOptions>("a name")
                .Configure(o => o.DefaultConnectionString = "begin")
                .ValidateDataAnnotations();
            services.AddMultiTenant<TenantInfo>()
                .WithPerTenantNamedOptions<TestOptions>("a name", (o, ti) => o.DefaultConnectionString = null);
            var sp = services.BuildServiceProvider();
            var accessor = sp.GetRequiredService<IMultiTenantContextAccessor<TenantInfo>>();
            accessor.MultiTenantContext = new MultiTenantContext<TenantInfo>
                { TenantInfo = new TenantInfo { Id = "id", Identifier = "identifier" } };
            
            Assert.Throws<OptionsValidationException>(
                () => sp.GetRequiredService<IOptionsSnapshot<TestOptions>>().Get("a name"));
        }
    }
}