// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Net;
using System.Text;
using System.Text.Json;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Extensions;
using Finbuckle.MultiTenant.StoreCaches;
using Finbuckle.MultiTenant.Stores;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Finbuckle.MultiTenant.Test.Stores;

// The tests in this file prove that every built-in store, cache, the tenant manager, and the resolver work with a
// constructor-only immutable ITenantInfo implementation that does not derive from TenantInfo (see issue #836/#1073).

public class InMemoryStoreImmutableTenantShould : ImmutableTenantStoreTestBase
{
    protected override async Task<IMultiTenantStore<ImmutableTenantInfo, string>> CreateTestStore()
    {
        var store = new InMemoryStore<ImmutableTenantInfo, string>();
        return await PopulateTestStore(store);
    }

    [Fact]
    public override Task GetTenantInfoFromStoreById() => base.GetTenantInfoFromStoreById();

    [Fact]
    public override Task ReturnNullWhenGettingByIdIfTenantInfoNotFound() => base.ReturnNullWhenGettingByIdIfTenantInfoNotFound();

    [Fact]
    public override Task GetTenantInfoFromStoreByIdentifier() => base.GetTenantInfoFromStoreByIdentifier();

    [Fact]
    public override Task ReturnNullWhenGettingByIdentifierIfTenantInfoNotFound() => base.ReturnNullWhenGettingByIdentifierIfTenantInfoNotFound();

    [Fact]
    public override Task AddTenantInfoToStore() => base.AddTenantInfoToStore();

    [Fact]
    public override Task UpdateTenantInfoInStore() => base.UpdateTenantInfoInStore();

    [Fact]
    public override Task RemoveTenantInfoFromStore() => base.RemoveTenantInfoFromStore();

    [Fact]
    public override Task RemoveTenantInfoFromStoreByIdentifier() => base.RemoveTenantInfoFromStoreByIdentifier();

    [Fact]
    public override Task GetAllTenantsFromStoreAsync() => base.GetAllTenantsFromStoreAsync();
}

public class ConfigurationStoreImmutableTenantShould : ImmutableTenantStoreTestBase
{
    protected override Task<IMultiTenantStore<ImmutableTenantInfo, string>> CreateTestStore()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("ConfigurationStoreTestSettings.json")
            .Build();

        // No factory: the configuration binder constructs the type through its single public constructor.
        return Task.FromResult<IMultiTenantStore<ImmutableTenantInfo, string>>(
            new ConfigurationStore<ImmutableTenantInfo, string>(configuration));
    }

    [Fact]
    public async Task BindConstructorParametersFromConfiguration()
    {
        var store = await CreateTestStore();

        var tenant = await store.GetByIdentifierAsync("initech");

        Assert.NotNull(tenant);
        Assert.Equal("initech-id", tenant.Id);
        Assert.Equal("Initech", tenant.Name);
    }

    [Fact]
    public override Task GetTenantInfoFromStoreById() => base.GetTenantInfoFromStoreById();

    [Fact]
    public override Task GetTenantInfoFromStoreByIdentifier() => base.GetTenantInfoFromStoreByIdentifier();

    [Fact]
    public override Task ReturnNullWhenGettingByIdentifierIfTenantInfoNotFound() => base.ReturnNullWhenGettingByIdentifierIfTenantInfoNotFound();

    [Fact]
    public override Task ReturnNullWhenGettingByIdIfTenantInfoNotFound() => base.ReturnNullWhenGettingByIdIfTenantInfoNotFound();

    [Fact]
    public override Task GetAllTenantsFromStoreAsync() => base.GetAllTenantsFromStoreAsync();

    [Fact]
    public override Task GetAllTenantsFromStoreAsyncSkip1Take1() => base.GetAllTenantsFromStoreAsyncSkip1Take1();

    // Not valid for this read-only store.
    public override Task AddTenantInfoToStore() => Task.CompletedTask;
    public override Task RemoveTenantInfoFromStore() => Task.CompletedTask;
    public override Task RemoveTenantInfoFromStoreByIdentifier() => Task.CompletedTask;
    public override Task UpdateTenantInfoInStore() => Task.CompletedTask;
}

public class ConfigurationStoreBindingShould
{
    private const string Prefix = "Finbuckle:MultiTenant:Stores:ConfigurationStore";

    // A tenant type with a property that only exists in Defaults for some tenants.
    public class ConnectionStringTenantInfo : TenantInfo<string>
    {
        public string? ConnectionString { get; init; }
    }

    // A type the binder cannot construct on its own: two public parameterized constructors and no parameterless one.
    public sealed class UnbindableTenantInfo : ITenantInfo<string>
    {
        public UnbindableTenantInfo(string id, string identifier)
        {
            Id = id;
            Identifier = identifier;
        }

        public UnbindableTenantInfo(string id, string identifier, string? name) : this(id, identifier)
        {
            Name = name;
        }

        public string Id { get; }
        public string Identifier { get; }
        public string? Name { get; }
    }

    [Fact]
    public async Task ApplyDefaultsUnlessTenantOverridesThem()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("ConfigurationStoreTestSettings.json")
            .Build();

        var store = new ConfigurationStore<ConnectionStringTenantInfo, string>(configuration);

        Assert.Equal("Datasource=sample.db", (await store.GetByIdentifierAsync("initech"))!.ConnectionString);
        Assert.Equal("Datasource=lol.db", (await store.GetByIdentifierAsync("lol"))!.ConnectionString);
    }

    [Fact]
    public async Task BindImmutableTenantWithNonStringId()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{Prefix}:Tenants:0:Id"] = "7",
                [$"{Prefix}:Tenants:0:Identifier"] = "initech",
            })
            .Build();

        var store = new ConfigurationStore<ImmutableIntTenantInfo, int>(configuration);

        var tenant = await store.GetAsync(7);
        Assert.NotNull(tenant);
        Assert.Equal("initech", tenant.Identifier);
    }

    [Fact]
    public void ThrowClearErrorWhenTypeCannotBeBound()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("ConfigurationStoreTestSettings.json")
            .Build();

        var e = Assert.Throws<MultiTenantException>(() =>
            new ConfigurationStore<UnbindableTenantInfo, string>(configuration));

        Assert.Contains("tenantInfoFactory", e.Message);
    }

    [Fact]
    public async Task UseTenantInfoFactoryWhenProvided()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("ConfigurationStoreTestSettings.json")
            .Build();

        var store = new ConfigurationStore<UnbindableTenantInfo, string>(configuration, Prefix,
            config => new UnbindableTenantInfo(config["Id"]!, config["Identifier"]!, config["Name"]));

        var tenant = await store.GetByIdentifierAsync("lol");
        Assert.NotNull(tenant);
        Assert.Equal("lol-id", tenant.Id);
        Assert.Equal("LOL", tenant.Name);
        Assert.Equal(2, (await store.GetAllAsync()).Count());
    }

    [Fact]
    public async Task PassMergedDefaultsToTenantInfoFactory()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("ConfigurationStoreTestSettings.json")
            .Build();

        var seen = new Dictionary<string, string?>();
        var store = new ConfigurationStore<ImmutableTenantInfo, string>(configuration, Prefix, config =>
        {
            seen[config["Identifier"]!] = config["ConnectionString"];
            return new ImmutableTenantInfo(config["Id"]!, config["Identifier"]!);
        });

        Assert.Equal(2, (await store.GetAllAsync()).Count());
        Assert.Equal("Datasource=sample.db", seen["initech"]);
        Assert.Equal("Datasource=lol.db", seen["lol"]);
    }

    [Fact]
    public void ThrowIfTenantInfoFactoryIsNull()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("ConfigurationStoreTestSettings.json")
            .Build();

        Assert.Throws<ArgumentNullException>(() =>
            new ConfigurationStore<ImmutableTenantInfo, string>(configuration, Prefix, null!));
    }

    [Fact]
    public void ThrowIfTenantInfoFactoryReturnsNull()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("ConfigurationStoreTestSettings.json")
            .Build();

        Assert.Throws<MultiTenantException>(() =>
            new ConfigurationStore<ImmutableTenantInfo, string>(configuration, Prefix, _ => null!));
    }

    [Fact]
    public async Task RegisterFactoryOverloadThroughBuilder()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("ConfigurationStoreTestSettings.json")
            .Build();

        var services = new ServiceCollection();
        services.AddMultiTenant<UnbindableTenantInfo, string>()
            .WithConfigurationStore(configuration, Prefix,
                config => new UnbindableTenantInfo(config["Id"]!, config["Identifier"]!));
        var sp = services.BuildServiceProvider();

        var store = sp.GetRequiredService<IMultiTenantStore<UnbindableTenantInfo, string>>();
        Assert.IsType<ConfigurationStore<UnbindableTenantInfo, string>>(store);
        Assert.Equal("initech-id", (await store.GetByIdentifierAsync("initech"))!.Id);
    }

    public sealed class ImmutableIntTenantInfo : ITenantInfo<int>
    {
        public ImmutableIntTenantInfo(int id, string identifier)
        {
            Id = id;
            Identifier = identifier;
        }

        public int Id { get; }
        public string Identifier { get; }
    }
}

public class EchoStoreImmutableTenantShould
{
    [Fact]
    public async Task ThrowClearErrorWithoutFactory()
    {
        var store = new EchoStore<ImmutableTenantInfo, string>(identifier => identifier);

        var e = await Assert.ThrowsAsync<MultiTenantException>(() => store.GetByIdentifierAsync("initech"));
        Assert.Contains("tenantInfoFactory", e.Message);

        e = await Assert.ThrowsAsync<MultiTenantException>(() => store.GetAsync("initech"));
        Assert.Contains("tenantInfoFactory", e.Message);
    }

    [Fact]
    public async Task EchoWithFactory()
    {
        var store = new EchoStore<ImmutableTenantInfo, string>(identifier => identifier,
            (id, identifier) => new ImmutableTenantInfo(id, identifier));

        var byIdentifier = await store.GetByIdentifierAsync("initech");
        Assert.Equal("initech", byIdentifier!.Id);
        Assert.Equal("initech", byIdentifier.Identifier);

        var byId = await store.GetAsync("initech");
        Assert.Equal("initech", byId!.Id);
        Assert.Equal("initech", byId.Identifier);
    }

    [Fact]
    public async Task EchoWithFactoryForNonStringId()
    {
        var store = new EchoStore<ConfigurationStoreBindingShould.ImmutableIntTenantInfo, int>(int.Parse,
            (id, identifier) => new ConfigurationStoreBindingShould.ImmutableIntTenantInfo(id, identifier));

        Assert.Equal(17, (await store.GetByIdentifierAsync("17"))!.Id);
        Assert.Equal("42", (await store.GetAsync(42))!.Identifier);
    }

    [Fact]
    public void ThrowIfFactoryIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new EchoStore<ImmutableTenantInfo, string>(identifier => identifier, null!));
    }

    [Fact]
    public async Task StillUseReflectionForSettableTypes()
    {
        // MutableTenantInfo has public setters; TenantInfo<string> has init setters. Both work without a factory.
        var mutable = new EchoStore<MutableTenantInfo, string>(identifier => identifier);
        Assert.Equal("initech", (await mutable.GetByIdentifierAsync("initech"))!.Id);

        var init = new EchoStore<TenantInfo<string>, string>(identifier => identifier);
        Assert.Equal("initech", (await init.GetByIdentifierAsync("initech"))!.Id);
    }

    [Fact]
    public async Task RegisterFactoryOverloadThroughBuilder()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<ImmutableTenantInfo, string>()
            .WithEchoStore(identifier => identifier, (id, identifier) => new ImmutableTenantInfo(id, identifier));
        var sp = services.BuildServiceProvider();

        var store = sp.GetRequiredService<IMultiTenantStore<ImmutableTenantInfo, string>>();
        Assert.IsType<EchoStore<ImmutableTenantInfo, string>>(store);
        Assert.Equal("initech", (await store.GetByIdentifierAsync("initech"))!.Id);
    }

    [Fact]
    public async Task RegisterSingleArgOverloadThroughBuilderForSettableTypes()
    {
        // Ensures adding the two-argument constructor did not break DI activation of the one-argument one.
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo, string>().WithEchoStore(identifier => identifier);
        var sp = services.BuildServiceProvider();

        var store = sp.GetRequiredService<IMultiTenantStore<TenantInfo, string>>();
        Assert.Equal("initech", (await store.GetByIdentifierAsync("initech"))!.Id);
    }
}

public class HttpRemoteStoreImmutableTenantShould
{
    private sealed class TestHandler(string json) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) });
    }

    [Fact]
    public async Task DeserializeThroughJsonConstructor()
    {
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(new HttpClient(new TestHandler("""{"id":"initech-id","identifier":"initech","name":"Initech"}""")));
        var client = new HttpRemoteStoreClient<ImmutableTenantInfo, string>(factory.Object);
        var store = new HttpRemoteStore<ImmutableTenantInfo, string>(client, "https://example.com/{__tenant__}");

        var tenant = await store.GetByIdentifierAsync("initech");

        Assert.NotNull(tenant);
        Assert.Equal("initech-id", tenant.Id);
        Assert.Equal("initech", tenant.Identifier);
        Assert.Equal("Initech", tenant.Name);
    }
}

public class StoreCachesImmutableTenantShould
{
    [Fact]
    public async Task RoundTripThroughMemoryCache()
    {
        var cache = new MemoryCacheStoreCache<ImmutableTenantInfo, string>(new MemoryCache(new MemoryCacheOptions()),
            Constants.TenantToken, new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.MaxValue });

        await cache.SetAsync(new ImmutableTenantInfo("initech-id", "initech", "Initech"));

        Assert.Equal("initech", (await cache.GetAsync("initech-id"))!.Identifier);
        Assert.Equal("initech-id", (await cache.GetByIdentifierAsync("initech"))!.Id);
    }

    [Fact]
    public async Task RoundTripThroughDistributedCache()
    {
        var services = new ServiceCollection();
        services.AddOptions().AddDistributedMemoryCache();
        var sp = services.BuildServiceProvider();

        var cache = new DistributedCacheStoreCache<ImmutableTenantInfo, string>(
            sp.GetRequiredService<IDistributedCache>(), Constants.TenantToken,
            new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(1) });

        await cache.SetAsync(new ImmutableTenantInfo("initech-id", "initech", "Initech"));

        var byId = await cache.GetAsync("initech-id");
        Assert.NotNull(byId);
        Assert.Equal("initech", byId.Identifier);
        Assert.Equal("Initech", byId.Name);
        Assert.Equal("initech-id", (await cache.GetByIdentifierAsync("initech"))!.Id);
    }

    [Fact]
    public void SerializeImmutableTenantAsJson()
    {
        // Sanity check of the serialization contract the distributed cache and remote store rely on.
        var json = JsonSerializer.Serialize(new ImmutableTenantInfo("initech-id", "initech"));
        var roundTripped = JsonSerializer.Deserialize<ImmutableTenantInfo>(Encoding.UTF8.GetBytes(json));

        Assert.Equal("initech-id", roundTripped!.Id);
        Assert.Equal("initech", roundTripped.Identifier);
    }
}

public class TenantManagerImmutableTenantShould
{
    [Fact]
    public async Task AddUpdateRemoveAndInvalidateCaches()
    {
        var store = new InMemoryStore<ImmutableTenantInfo, string>();
        var cache = new MemoryCacheStoreCache<ImmutableTenantInfo, string>(new MemoryCache(new MemoryCacheOptions()),
            Constants.TenantToken, new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.MaxValue });
        var manager = new TenantManager<ImmutableTenantInfo, string>(store, [cache]);

        Assert.True(await manager.AddAsync(new ImmutableTenantInfo("initech-id", "initech")));
        Assert.Equal("initech", (await manager.GetAsync("initech-id"))!.Identifier);
        Assert.NotNull(await cache.GetByIdentifierAsync("initech"));

        // updating requires a new instance because the type is immutable
        Assert.True(await manager.UpdateAsync(new ImmutableTenantInfo("initech-id", "initech2")));
        Assert.Null(await cache.GetByIdentifierAsync("initech"));
        Assert.Equal("initech2", (await manager.GetByIdentifierAsync("initech2"))!.Identifier);

        Assert.True(await manager.RemoveAsync("initech-id"));
        Assert.Null(await manager.GetAsync("initech-id"));
        Assert.Null(await cache.GetByIdentifierAsync("initech2"));
    }
}

public class TenantResolutionImmutableTenantShould
{
    [Fact]
    public async Task ResolveAndFlowThroughAmbientContext()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<ImmutableTenantInfo, string>()
            .WithStaticStrategy("initech")
            .WithMemoryCacheStoreCache()
            .WithInMemoryStore();
        var sp = services.BuildServiceProvider();

        await sp.GetRequiredService<TenantManager<ImmutableTenantInfo, string>>()
            .AddAsync(new ImmutableTenantInfo("initech-id", "initech", "Initech"));

        var resolved = await sp.GetRequiredService<ITenantResolver<ImmutableTenantInfo, string>>().ResolveAsync(new object());
        Assert.NotNull(resolved);
        Assert.Equal("initech-id", resolved.Id);

        sp.BeginTenantScope(resolved);
        var context = sp.GetRequiredService<ITenantContext<ImmutableTenantInfo, string>>();
        Assert.True(context.IsResolved);
        Assert.Same(resolved, context.TenantInfo);

        // the active tenant of a scope is set once; the library exposes no way to replace it
        Assert.Throws<MultiTenantException>(() => context.TenantInfo = new ImmutableTenantInfo("other", "other"));
        Assert.Same(resolved, context.TenantInfo);
    }

    [Fact]
    public void ResolvePerTenantOptions()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<ImmutableTenantInfo, string>();
        services.Configure<TestOptions>(options => options.Value = "base");
        services.ConfigurePerTenant<TestOptions, ImmutableTenantInfo, string>((options, tenant) =>
            options.Value += $":{tenant.Name}");

        using var provider = services.BuildServiceProvider();
        provider.BeginTenantScope(new ImmutableTenantInfo("initech-id", "initech", "Initech"));

        Assert.Equal("base:Initech", provider.GetRequiredService<IOptions<TestOptions>>().Value.Value);
        Assert.Equal("base:Initech", provider.GetRequiredService<IOptionsMonitor<TestOptions>>().CurrentValue.Value);
    }

    public class TestOptions
    {
        public string? Value { get; set; }
    }
}
