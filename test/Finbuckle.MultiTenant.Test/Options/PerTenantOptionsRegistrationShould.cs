// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Extensions;
using Finbuckle.MultiTenant.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Finbuckle.MultiTenant.Test.Options;

public class PerTenantOptionsRegistrationShould
{
    [Fact]
    public void ReplaceExactClosedOptionsRegistrationsAndKeepOpenGenericDefaults()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>();
        services.AddOptions();
        services.AddSingleton<IOptionsMonitorCache<TestOptions>, OptionsCache<TestOptions>>();
        services.AddSingleton<IOptions<TestOptions>>(Microsoft.Extensions.Options.Options.Create(new TestOptions()));
        services.AddScoped<IOptionsSnapshot<TestOptions>, CustomOptionsSnapshot>();

        services.ConfigurePerTenant<TestOptions, TenantInfo>((_, _) => { });

        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IOptions<>));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IOptionsSnapshot<>));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IOptionsMonitorCache<>));
        Assert.Single(services, descriptor => descriptor.ServiceType == typeof(IOptions<TestOptions>));
        Assert.Single(services, descriptor => descriptor.ServiceType == typeof(IOptionsSnapshot<TestOptions>));
        var cacheDescriptor = Assert.Single(services,
            descriptor => descriptor.ServiceType == typeof(IOptionsMonitorCache<TestOptions>));
        Assert.Equal(typeof(MultiTenantOptionsCache<TestOptions>), cacheDescriptor.ImplementationType);

        using var provider = services.BuildServiceProvider();
        Assert.IsType<MultiTenantOptionsManager<TestOptions>>(
            provider.GetRequiredService<IOptions<TestOptions>>());
        using var scope = provider.CreateScope();
        Assert.IsType<MultiTenantOptionsManager<TestOptions>>(
            scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<TestOptions>>());
    }

    [Fact]
    public void LeaveOneClosedRegistrationAfterRepeatedConfiguration()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>();
        services.ConfigurePerTenant<TestOptions, TenantInfo>((_, _) => { });
        services.AddSingleton<IOptionsMonitorCache<TestOptions>, OptionsCache<TestOptions>>();

        services.PostConfigurePerTenant<TestOptions, TenantInfo>((_, _) => { });

        Assert.Single(services, descriptor => descriptor.ServiceType == typeof(IOptions<TestOptions>));
        Assert.Single(services, descriptor => descriptor.ServiceType == typeof(IOptionsSnapshot<TestOptions>));
        var cacheDescriptor = Assert.Single(services,
            descriptor => descriptor.ServiceType == typeof(IOptionsMonitorCache<TestOptions>));
        Assert.Equal(typeof(MultiTenantOptionsCache<TestOptions>), cacheDescriptor.ImplementationType);
    }

    [Fact]
    public void ValidateNullArgumentsLikeMicrosoftOptionsExtensions()
    {
        var services = new ServiceCollection();
        Action<TestOptions, TenantInfo> configure = (_, _) => { };
        OptionsBuilder<TestOptions>? builder = null;

        Assert.Throws<ArgumentNullException>(() =>
            ServiceCollectionExtensions.ConfigurePerTenant<TestOptions, TenantInfo>(null!, configure));
        Assert.Throws<ArgumentNullException>(() =>
            services.ConfigurePerTenant<TestOptions, TenantInfo>(null!));
        Assert.Throws<ArgumentNullException>(() =>
            ServiceCollectionExtensions.PostConfigurePerTenant<TestOptions, TenantInfo>(null!, configure));
        Assert.Throws<ArgumentNullException>(() =>
            services.PostConfigurePerTenant<TestOptions, TenantInfo>(null!));
        Assert.Throws<ArgumentNullException>(() =>
            Finbuckle.MultiTenant.Extensions.OptionsBuilderExtensions
                .ConfigurePerTenant<TestOptions, TenantInfo>(builder!, configure));
        Assert.Throws<ArgumentNullException>(() =>
            Finbuckle.MultiTenant.Extensions.OptionsBuilderExtensions
                .PostConfigurePerTenant<TestOptions, TenantInfo>(builder!, configure));
    }

    public class TestOptions
    {
        public string? Value { get; set; }
    }

    private sealed class CustomOptionsSnapshot : IOptionsSnapshot<TestOptions>
    {
        public TestOptions Value { get; } = new();

        public TestOptions Get(string? name) => Value;
    }
}
