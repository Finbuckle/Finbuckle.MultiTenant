// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Options.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Finbuckle.MultiTenant.Options.Test.Extensions;

public class ServiceCollectionExtensionsShould
{
    public class TestOptions
    {
        public string? Prop1 { get; set; }
    }

    [Fact]
    public void RegisterNamedOptionsPerTenant()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>();
        services.ConfigurePerTenant<TestOptions, TenantInfo>("name1",
            (option, tenant) => option.Prop1 = tenant.Id);
        var sp = services.BuildServiceProvider();

        var configs = sp.GetRequiredService<IEnumerable<IConfigureOptions<TestOptions>>>();
        var config = configs.Where(config =>
            config is ConfigureNamedOptions<TestOptions, IMultiTenantContextAccessor<TenantInfo>> options).ToList();

        Assert.Single(config);
        Assert.Equal("name1",
            config.Select(c => (ConfigureNamedOptions<TestOptions, IMultiTenantContextAccessor<TenantInfo>>)c).Single()
                .Name);
    }

    [Fact]
    public void RegisterUnnamedOptionsPerTenant()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>();
        services.ConfigurePerTenant<TestOptions, TenantInfo>((option, tenant) => option.Prop1 = tenant.Id);
        var sp = services.BuildServiceProvider();

        var configs = sp.GetRequiredService<IEnumerable<IConfigureOptions<TestOptions>>>();
        var config = configs.Where(config =>
            config is ConfigureNamedOptions<TestOptions, IMultiTenantContextAccessor<TenantInfo>> options).ToList();

        Assert.Single(config);
        Assert.Equal(Microsoft.Extensions.Options.Options.DefaultName,
            config.Select(c => (ConfigureNamedOptions<TestOptions, IMultiTenantContextAccessor<TenantInfo>>)c).Single()
                .Name);
    }

    [Fact]
    public void RegisterAllOptionsPerTenant()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>();
        services.ConfigureAllPerTenant<TestOptions, TenantInfo>((option, tenant) => option.Prop1 = tenant.Id);
        var sp = services.BuildServiceProvider();

        var configs = sp.GetRequiredService<IEnumerable<IConfigureOptions<TestOptions>>>();
        var config = configs.Where(config =>
            config is ConfigureNamedOptions<TestOptions, IMultiTenantContextAccessor<TenantInfo>> options).ToList();

        Assert.Single(config);
        Assert.Null(config.Select(c => (ConfigureNamedOptions<TestOptions, IMultiTenantContextAccessor<TenantInfo>>)c)
            .Single().Name);
    }
}