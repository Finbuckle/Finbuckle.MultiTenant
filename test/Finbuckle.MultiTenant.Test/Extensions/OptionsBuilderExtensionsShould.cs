// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Finbuckle.MultiTenant.Test.Extensions;

public class OptionsBuilderExtensionsShould
{
    public class TestOptions
    {
        public string? Prop1;
        public string? Prop2;
    }

    public class TestDependency
    {
        public string Value;

        public TestDependency(string value) => Value = value;
    }

    [Fact]
    public void ConfigurePerTenant_OnOptionsBuilder_UsesTenantContext()
    {
        var services = new ServiceCollection();
        var tenantHolder = new TenantInfoHolder();
        services.AddSingleton(tenantHolder);
        services.AddScoped<ITenantContext<TenantInfo,string>>(sp => new TestTenantContext { TenantInfo = sp.GetRequiredService<TenantInfoHolder>().Current });
        services.AddScoped<ITenantContext<string>>(sp => sp.GetRequiredService<ITenantContext<TenantInfo,string>>());
        services.AddOptions<TestOptions>().ConfigurePerTenant<TestOptions, TenantInfo,string>((options, tenantInfo) => options.Prop1 = tenantInfo.Id);
        var provider = services.BuildServiceProvider();

        tenantHolder.Current = new TenantInfo { Id = "tenant-1", Identifier = "identifier-1" };

        // IOptions values are now shared across scopes via keyed singleton manager cache.
        using var scope1 = provider.CreateScope();
        var options1 = scope1.ServiceProvider.GetRequiredService<IOptions<TestOptions>>().Value;

        using var scope2 = provider.CreateScope();
        var options2 = scope2.ServiceProvider.GetRequiredService<IOptions<TestOptions>>().Value;

        Assert.Same(options1, options2);
        Assert.Equal(tenantHolder.Current!.Id, options1.Prop1);
        Assert.Equal(tenantHolder.Current!.Id, options2.Prop1);
    }

    [Fact]
    public void ConfigurePerTenant_OnOptionsBuilder_WithDependency_UsesDependencyAndTenantContext()
    {
        var services = new ServiceCollection();
        var tenantHolder = new TenantInfoHolder();
        services.AddSingleton(tenantHolder);
        services.AddScoped<ITenantContext<TenantInfo,string>>(sp => new TestTenantContext { TenantInfo = sp.GetRequiredService<TenantInfoHolder>().Current });
        services.AddScoped<ITenantContext<string>>(sp => sp.GetRequiredService<ITenantContext<TenantInfo,string>>());
        services.AddSingleton(new TestDependency("prefix-"));
        services.AddOptions<TestOptions>().ConfigurePerTenant<TestOptions, TestDependency, TenantInfo,string>((options, dep, tenantInfo) =>
            options.Prop1 = dep.Value + tenantInfo.Id);
        var provider = services.BuildServiceProvider();

        tenantHolder.Current = new TenantInfo { Id = "tenant-2", Identifier = "identifier-2" };

        // Dependency-backed configuration still resolves once per tenant/value and is reused across scopes.
        using var scope1 = provider.CreateScope();
        var options1 = scope1.ServiceProvider.GetRequiredService<IOptions<TestOptions>>().Value;

        using var scope2 = provider.CreateScope();
        var options2 = scope2.ServiceProvider.GetRequiredService<IOptions<TestOptions>>().Value;

        Assert.Same(options1, options2);
        Assert.Equal("prefix-tenant-2", options1.Prop1);
        Assert.Equal("prefix-tenant-2", options2.Prop1);
    }

    [Fact]
    public void PostConfigurePerTenant_OnOptionsBuilder_UsesTenantContext()
    {
        var services = new ServiceCollection();
        var tenantHolder = new TenantInfoHolder();
        services.AddSingleton(tenantHolder);
        services.AddScoped<ITenantContext<TenantInfo,string>>(sp => new TestTenantContext { TenantInfo = sp.GetRequiredService<TenantInfoHolder>().Current });
        services.AddScoped<ITenantContext<string>>(sp => sp.GetRequiredService<ITenantContext<TenantInfo,string>>());
        services.AddOptions<TestOptions>()
            .Configure(options => options.Prop1 = "base")
            .PostConfigurePerTenant<TestOptions, TenantInfo,string>((options, tenantInfo) => options.Prop2 = tenantInfo.Identifier);
        var provider = services.BuildServiceProvider();

        tenantHolder.Current = new TenantInfo { Id = "tenant-3", Identifier = "identifier-3" };

        // Post-configuration remains tenant-aware while the final IOptions value is shared per tenant.
        using var scope1 = provider.CreateScope();
        var options1 = scope1.ServiceProvider.GetRequiredService<IOptions<TestOptions>>().Value;

        using var scope2 = provider.CreateScope();
        var options2 = scope2.ServiceProvider.GetRequiredService<IOptions<TestOptions>>().Value;

        Assert.Same(options1, options2);
        Assert.Equal("base", options1.Prop1);
        Assert.Equal("base", options2.Prop1);
        Assert.Equal(tenantHolder.Current!.Identifier, options1.Prop2);
        Assert.Equal(tenantHolder.Current!.Identifier, options2.Prop2);
    }

    private sealed class TestTenantContext : ITenantContext<TenantInfo,string>
    {
        public TenantInfo? TenantInfo { get; set; }

        ITenantInfo<string>? ITenantContext<string>.TenantInfo
        {
            get => TenantInfo;
            set => TenantInfo = (TenantInfo?)value;
        }
    }

    private sealed class TenantInfoHolder
    {
        public TenantInfo? Current { get; set; }
    }
}
