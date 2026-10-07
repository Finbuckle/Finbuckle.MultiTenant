// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Stores;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Finbuckle.MultiTenant.Test.Stores;

public class ConfigurationStoreFactoryLifetimeShould
{
    private const string Prefix = "Finbuckle:MultiTenant:Stores:ConfigurationStore";

    [Fact]
    public void ReleaseMergedConfigurationSubscriptionsAfterEachReload()
    {
        using var configuration = CreateConfiguration();
        var tokens = new List<IChangeToken>();
        var store = new ConfigurationStore<TestTenant>(configuration, Prefix, merged =>
        {
            tokens.Add(merged.GetReloadToken());
            return new TestTenant { Id = merged["Id"]!, Identifier = merged["Identifier"]! };
        });

        for (var i = 0; i < 3; i++)
            configuration.Reload();

        Assert.Equal(4, tokens.Count);
        Assert.All(tokens, token => Assert.False(token.HasChanged));
        GC.KeepAlive(store);
    }

    [Theory]
    [InlineData("throw")]
    [InlineData("null")]
    [InlineData("invalid")]
    public void ReleaseMergedConfigurationSubscriptionsWhenConstructionFails(string failure)
    {
        using var configuration = CreateConfiguration();
        IChangeToken? token = null;

        void ConstructStore() => _ = new ConfigurationStore<TestTenant>(configuration, Prefix, merged =>
        {
            token = merged.GetReloadToken();
            return failure switch
            {
                "throw" => throw new InvalidOperationException("Factory failed."),
                "null" => null!,
                _ => new TestTenant { Id = "", Identifier = "tenant" }
            };
        });

        if (failure == "throw")
            Assert.Throws<InvalidOperationException>(ConstructStore);
        else
            Assert.Throws<MultiTenantException>(ConstructStore);

        configuration.Reload();

        Assert.NotNull(token);
        Assert.False(token.HasChanged);
    }

    private static ConfigurationRoot CreateConfiguration() => (ConfigurationRoot)new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            [$"{Prefix}:Tenants:0:Id"] = "id",
            [$"{Prefix}:Tenants:0:Identifier"] = "tenant"
        }).Build();

    public class TestTenant : ITenantInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Identifier { get; set; } = string.Empty;
    }
}
