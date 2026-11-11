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
        new ConfigurationStore<TenantInfo>(configuration);
    }

    [Fact]
    public void ThrowIfNullConfiguration()
    {
        Assert.Throws<ArgumentNullException>(() => new ConfigurationStore<TenantInfo>(null!));
    }

    [Fact]
    public void ThrowIfEmptyOrNullSection()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
        IConfiguration configuration = configBuilder.Build();

        Assert.Throws<ArgumentException>(() => new ConfigurationStore<TenantInfo>(configuration, ""));
        Assert.Throws<ArgumentException>(() => new ConfigurationStore<TenantInfo>(configuration, null!));
    }

    [Fact]
    public void ThrowIfInvalidSection()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
        IConfiguration configuration = configBuilder.Build();

        Assert.Throws<MultiTenantException>(() => new ConfigurationStore<TenantInfo>(configuration, "invalid"));
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

    private const string CollectionPrefix = "Finbuckle:MultiTenant:Stores:ConfigurationStore";

    private static IConfiguration BuildCollectionConfiguration(Dictionary<string, string?> values)
    {
        var data = new Dictionary<string, string?>
        {
            [$"{CollectionPrefix}:Tenants:0:Id"] = "initech-id",
            [$"{CollectionPrefix}:Tenants:0:Identifier"] = "initech",
            [$"{CollectionPrefix}:Tenants:1:Id"] = "lol-id",
            [$"{CollectionPrefix}:Tenants:1:Identifier"] = "lol"
        };

        foreach (var (key, value) in values)
            data[$"{CollectionPrefix}:{key}"] = value;

        return new ConfigurationBuilder().AddInMemoryCollection(data).Build();
    }

    [Fact]
    public async Task AppendTenantListEntriesToDefaults()
    {
        // Defaults and the tenant section are bound one after the other, so list entries present in both are
        // appended (the behavior since before 10.2.0).
        var configuration = BuildCollectionConfiguration(new Dictionary<string, string?>
        {
            ["Defaults:Tags:0"] = "default-0",
            ["Defaults:Tags:1"] = "default-1",
            ["Tenants:0:Tags:0"] = "initech-0"
        });

        var store = new ConfigurationStore<TaggedTenantInfo>(configuration);

        var initech = await store.GetByIdentifierAsync("initech");
        var lol = await store.GetByIdentifierAsync("lol");

        Assert.NotNull(initech);
        Assert.Equal(new[] { "default-0", "default-1", "initech-0" }, initech.Tags);
        Assert.NotNull(lol);
        Assert.Equal(new[] { "default-0", "default-1" }, lol.Tags);
    }

    [Fact]
    public async Task AppendTenantListEntriesWhenTenantHasMoreEntriesThanDefaults()
    {
        var configuration = BuildCollectionConfiguration(new Dictionary<string, string?>
        {
            ["Defaults:Tags:0"] = "default-0",
            ["Tenants:0:Tags:0"] = "initech-0",
            ["Tenants:0:Tags:1"] = "initech-1",
            ["Tenants:0:Tags:2"] = "initech-2"
        });

        var store = new ConfigurationStore<TaggedTenantInfo>(configuration);

        var initech = await store.GetByIdentifierAsync("initech");

        Assert.NotNull(initech);
        Assert.Equal(new[] { "default-0", "initech-0", "initech-1", "initech-2" }, initech.Tags);
    }

    [Fact]
    public async Task AppendTenantArrayEntriesToDefaults()
    {
        var configuration = BuildCollectionConfiguration(new Dictionary<string, string?>
        {
            ["Defaults:Codes:0"] = "default-0",
            ["Defaults:Codes:1"] = "default-1",
            ["Tenants:0:Codes:0"] = "initech-0"
        });

        var store = new ConfigurationStore<TaggedTenantInfo>(configuration);

        var initech = await store.GetByIdentifierAsync("initech");
        var lol = await store.GetByIdentifierAsync("lol");

        Assert.NotNull(initech);
        Assert.Equal(new[] { "default-0", "default-1", "initech-0" }, initech.Codes);
        Assert.NotNull(lol);
        Assert.Equal(new[] { "default-0", "default-1" }, lol.Codes);
    }

    [Fact]
    public async Task MergeDictionaryKeysFromDefaultsAndTenant()
    {
        // Dictionaries merge by key: tenant keys replace matching default keys and other default keys are kept.
        var configuration = BuildCollectionConfiguration(new Dictionary<string, string?>
        {
            ["Defaults:Settings:Theme"] = "light",
            ["Defaults:Settings:Region"] = "us",
            ["Tenants:0:Settings:Theme"] = "dark",
            ["Tenants:0:Settings:Plan"] = "gold"
        });

        var store = new ConfigurationStore<TaggedTenantInfo>(configuration);

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
    public async Task OverlayTenantCollectionEntriesOnDefaultsByIndexForFactory()
    {
        // The factory receives one configuration with the tenant section layered over Defaults, so collection
        // entries present in both are overlaid by index (standard configuration layering) rather than appended.
        var configuration = BuildCollectionConfiguration(new Dictionary<string, string?>
        {
            ["Defaults:Tags:0"] = "default-0",
            ["Defaults:Tags:1"] = "default-1",
            ["Tenants:0:Tags:0"] = "initech-0"
        });

        var store = new ConfigurationStore<TaggedTenantInfo>(configuration, CollectionPrefix, config =>
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

    protected override Task<IMultiTenantStore<TenantInfo>> CreateTestStore()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
        var configuration = configBuilder.Build();

        return Task.FromResult<IMultiTenantStore<TenantInfo>>(new ConfigurationStore<TenantInfo>(configuration));
    }

    protected override Task<IMultiTenantStore<TenantInfo>> PopulateTestStore(IMultiTenantStore<TenantInfo> store)
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

    public class TaggedTenantInfo : TenantInfo
    {
        public List<string> Tags { get; set; } = new();
        public string[] Codes { get; set; } = [];
        public Dictionary<string, string> Settings { get; set; } = new();
    }
}