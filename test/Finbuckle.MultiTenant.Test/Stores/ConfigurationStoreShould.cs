// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Stores;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Finbuckle.MultiTenant.Test.Stores;

public class ConfigurationStoreShould : MultiTenantStoreTestBase
{
    [Fact]
    public void NotThrowIfNoDefaultSection()
    {
        // See https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/426
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("ConfigurationStoreTestSettings_NoDefaults.json");
        IConfiguration configuration = configBuilder.Build();

        // ReSharper disable once ObjectCreationAsStatement
        // Will throw if fail
        new ConfigurationStore<TenantInfo, string>(configuration);
    }

    [Fact]
    public void ThrowIfNullConfiguration()
    {
        Assert.Throws<ArgumentNullException>(() => new ConfigurationStore<TenantInfo, string>(null!));
    }

    [Fact]
    public void ThrowIfEmptyOrNullSection()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
        IConfiguration configuration = configBuilder.Build();

        Assert.Throws<ArgumentException>(() => new ConfigurationStore<TenantInfo,string>(configuration, ""));
        Assert.Throws<ArgumentException>(() => new ConfigurationStore<TenantInfo,string>(configuration, null!));
    }

    [Fact]
    public void ThrowIfInvalidSection()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
        IConfiguration configuration = configBuilder.Build();

        Assert.Throws<MultiTenantException>(() => new ConfigurationStore<TenantInfo,string>(configuration, "invalid"));
    }

    [Fact]
    public async Task IgnoreCaseWhenGettingTenantInfoFromStoreByIdentifier()
    {
        var store = await CreateTestStore();

        var tenant = await store.GetByIdentifierAsync("INITECH");

        Assert.NotNull(tenant);
        Assert.Equal("initech", tenant.Identifier);
    }

    [Fact]
    public async Task ThrowWhenTryingToGetIdentifierGivenNullIdentifier()
    {
        var store = await CreateTestStore();

        await Assert.ThrowsAsync<ArgumentNullException>(async () => await store.GetByIdentifierAsync(null!));
    }

    [Fact]
    public async Task OverlayTenantCollectionEntriesOnDefaultsByIndex()
    {
        // The tenant section is layered over Defaults into one configuration before binding, so collection entries
        // present in both are overlaid by index (standard configuration layering) rather than appended.
        const string prefix = "Finbuckle:MultiTenant:Stores:ConfigurationStore";
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{prefix}:Defaults:Tags:0"] = "default-0",
                [$"{prefix}:Defaults:Tags:1"] = "default-1",
                [$"{prefix}:Tenants:0:Id"] = "initech-id",
                [$"{prefix}:Tenants:0:Identifier"] = "initech",
                [$"{prefix}:Tenants:0:Tags:0"] = "initech-0",
                [$"{prefix}:Tenants:1:Id"] = "lol-id",
                [$"{prefix}:Tenants:1:Identifier"] = "lol"
            })
            .Build();

        var store = new ConfigurationStore<TaggedTenantInfo, string>(configuration);

        var initech = await store.GetByIdentifierAsync("initech");
        var lol = await store.GetByIdentifierAsync("lol");

        Assert.NotNull(initech);
        Assert.Equal(new[] { "initech-0", "default-1" }, initech.Tags);
        Assert.NotNull(lol);
        Assert.Equal(new[] { "default-0", "default-1" }, lol.Tags);
    }

    private const string BindingPrefix = "Finbuckle:MultiTenant:Stores:ConfigurationStore";

    private static IConfiguration BuildBindingConfiguration(Dictionary<string, string?> values)
    {
        var data = new Dictionary<string, string?>
        {
            [$"{BindingPrefix}:Tenants:0:Id"] = "initech-id",
            [$"{BindingPrefix}:Tenants:0:Identifier"] = "initech",
            [$"{BindingPrefix}:Tenants:1:Id"] = "lol-id",
            [$"{BindingPrefix}:Tenants:1:Identifier"] = "lol"
        };

        foreach (var (key, value) in values)
            data[$"{BindingPrefix}:{key}"] = value;

        return new ConfigurationBuilder().AddInMemoryCollection(data).Build();
    }

    [Fact]
    public async Task OverlayAndExtendDefaultsWhenTenantCollectionIsLonger()
    {
        var configuration = BuildBindingConfiguration(new Dictionary<string, string?>
        {
            ["Defaults:Tags:0"] = "default-0",
            ["Defaults:Tags:1"] = "default-1",
            ["Tenants:0:Tags:0"] = "initech-0",
            ["Tenants:0:Tags:1"] = "initech-1",
            ["Tenants:0:Tags:2"] = "initech-2"
        });

        var store = new ConfigurationStore<TaggedTenantInfo, string>(configuration);

        var initech = await store.GetByIdentifierAsync("initech");

        Assert.NotNull(initech);
        Assert.Equal(new[] { "initech-0", "initech-1", "initech-2" }, initech.Tags);
    }

    [Fact]
    public async Task OverlayTenantArrayEntriesOnDefaultsByIndex()
    {
        var configuration = BuildBindingConfiguration(new Dictionary<string, string?>
        {
            ["Defaults:Codes:0"] = "default-0",
            ["Defaults:Codes:1"] = "default-1",
            ["Tenants:0:Codes:0"] = "initech-0"
        });

        var store = new ConfigurationStore<TaggedTenantInfo, string>(configuration);

        var initech = await store.GetByIdentifierAsync("initech");
        var lol = await store.GetByIdentifierAsync("lol");

        Assert.NotNull(initech);
        Assert.Equal(new[] { "initech-0", "default-1" }, initech.Codes);
        Assert.NotNull(lol);
        Assert.Equal(new[] { "default-0", "default-1" }, lol.Codes);
    }

    [Fact]
    public async Task MergeDictionaryKeysFromDefaultsAndTenant()
    {
        var configuration = BuildBindingConfiguration(new Dictionary<string, string?>
        {
            ["Defaults:Settings:Theme"] = "light",
            ["Defaults:Settings:Region"] = "us",
            ["Tenants:0:Settings:Theme"] = "dark",
            ["Tenants:0:Settings:Plan"] = "gold"
        });

        var store = new ConfigurationStore<TaggedTenantInfo, string>(configuration);

        var initech = await store.GetByIdentifierAsync("initech");
        var lol = await store.GetByIdentifierAsync("lol");

        Assert.NotNull(initech);
        Assert.Equal(
            new Dictionary<string, string> { ["Theme"] = "dark", ["Region"] = "us", ["Plan"] = "gold" },
            initech.Settings);
        Assert.NotNull(lol);
        Assert.Equal(new Dictionary<string, string> { ["Theme"] = "light", ["Region"] = "us" }, lol.Settings);
    }

    [Fact]
    public async Task KeepScalarInitializerValueWhenNotConfigured()
    {
        // The binder constructs the instance, so property initializers run and act as fallbacks.
        var configuration = BuildBindingConfiguration(new Dictionary<string, string?>
        {
            ["Tenants:0:Plan"] = "gold"
        });

        var store = new ConfigurationStore<InitializedTenantInfo, string>(configuration);

        var initech = await store.GetByIdentifierAsync("initech");
        var lol = await store.GetByIdentifierAsync("lol");

        Assert.NotNull(initech);
        Assert.Equal("gold", initech.Plan);
        Assert.NotNull(lol);
        Assert.Equal("free", lol.Plan);
    }

    [Fact]
    public async Task AppendConfiguredEntriesToListInitializer()
    {
        // The binder adds configured entries to a list that an initializer already populated rather than replacing it.
        var configuration = BuildBindingConfiguration(new Dictionary<string, string?>
        {
            ["Defaults:Tags:0"] = "default-0",
            ["Tenants:0:Tags:0"] = "initech-0"
        });

        var store = new ConfigurationStore<InitializedTenantInfo, string>(configuration);

        var initech = await store.GetByIdentifierAsync("initech");
        var lol = await store.GetByIdentifierAsync("lol");

        Assert.NotNull(initech);
        Assert.Equal(new[] { "seed", "initech-0" }, initech.Tags);
        Assert.NotNull(lol);
        Assert.Equal(new[] { "seed", "default-0" }, lol.Tags);
    }

    [Fact]
    public async Task OverlayTenantCollectionEntriesOnDefaultsByIndexForFactory()
    {
        // The factory receives the same layered configuration as the standard binder.
        var configuration = BuildBindingConfiguration(new Dictionary<string, string?>
        {
            ["Defaults:Tags:0"] = "default-0",
            ["Defaults:Tags:1"] = "default-1",
            ["Tenants:0:Tags:0"] = "initech-0"
        });

        var store = new ConfigurationStore<TaggedTenantInfo, string>(configuration, BindingPrefix, config =>
            new TaggedTenantInfo
            {
                Id = config["Id"]!,
                Identifier = config["Identifier"]!,
                Tags = config.GetSection("Tags").Get<List<string>>() ?? new()
            });

        var initech = await store.GetByIdentifierAsync("initech");
        var lol = await store.GetByIdentifierAsync("lol");

        Assert.NotNull(initech);
        Assert.Equal(new[] { "initech-0", "default-1" }, initech.Tags);
        Assert.NotNull(lol);
        Assert.Equal(new[] { "default-0", "default-1" }, lol.Tags);
    }

    // Basic store functionality tested in MultiTenantStoresShould.cs

    protected override Task<IMultiTenantStore<TenantInfo, string>> CreateTestStore()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
        var configuration = configBuilder.Build();

        return Task.FromResult<IMultiTenantStore<TenantInfo, string>>(new ConfigurationStore<TenantInfo, string >(configuration));
    }

    protected override Task<IMultiTenantStore<TenantInfo, string>> PopulateTestStore(IMultiTenantStore<TenantInfo, string> store)
    {
        throw new NotImplementedException();
    }

    [Fact]
    public override async Task GetTenantInfoFromStoreById()
    {
        await base.GetTenantInfoFromStoreById();
    }

    [Fact]
    public override async Task GetTenantInfoFromStoreByIdentifier()
    {
        await base.GetTenantInfoFromStoreByIdentifier();
    }

    [Fact]
    public override async Task ReturnNullWhenGettingByIdentifierIfTenantInfoNotFound()
    {
        await base.ReturnNullWhenGettingByIdentifierIfTenantInfoNotFound();
    }

    [Fact]
    public override async Task ReturnNullWhenGettingByIdIfTenantInfoNotFound()
    {
        await base.ReturnNullWhenGettingByIdIfTenantInfoNotFound();
    }

    // [Fact(Skip = "Not valid for this store.")]
    public override Task AddTenantInfoToStore()
    {
        return Task.CompletedTask;
    }

    // [Fact(Skip = "Not valid for this store.")]
    public override Task RemoveTenantInfoFromStore()
    {
        return Task.CompletedTask;
    }

    // [Fact(Skip = "Not valid for this store.")]
    public override Task UpdateTenantInfoInStore()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public override async Task GetAllTenantsFromStoreAsync()
    {
        await base.GetAllTenantsFromStoreAsync();
    }
    
    [Fact]
    public override async Task GetAllTenantsFromStoreAsyncSkip1Take1()
    {
        await base.GetAllTenantsFromStoreAsyncSkip1Take1();
    }

    public class TaggedTenantInfo : TenantInfo<string>
    {
        public List<string> Tags { get; set; } = new();
        public string[] Codes { get; set; } = [];
        public Dictionary<string, string> Settings { get; set; } = new();
    }

    public class InitializedTenantInfo : TenantInfo<string>
    {
        public string Plan { get; set; } = "free";
        public List<string> Tags { get; set; } = ["seed"];
    }
}