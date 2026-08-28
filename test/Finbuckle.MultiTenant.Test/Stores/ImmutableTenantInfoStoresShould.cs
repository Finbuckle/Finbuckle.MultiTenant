// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Net;
using System.Text.Json;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Extensions;
using Finbuckle.MultiTenant.Stores;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using MsOptions = Microsoft.Extensions.Options.Options;

namespace Finbuckle.MultiTenant.Test.Stores;

// The tests in this file prove that every built-in store, the resolver, the multi-tenant context, and per-tenant
// options work with a constructor-only immutable ITenantInfo implementation that does not derive from TenantInfo
// (see issue #836/#1073).

public class InMemoryStoreImmutableTenantShould : ImmutableTenantStoreTestBase
{
    protected override async Task<IMultiTenantStore<ImmutableTenantInfo>> CreateTestStore()
    {
        var store = new InMemoryStore<ImmutableTenantInfo>(
            MsOptions.Create(new InMemoryStoreOptions<ImmutableTenantInfo>()));
        return await PopulateTestStore(store);
    }

    [Fact]
    public async Task MoveIdentifierWhenUpdatingTenant()
    {
        var store = await CreateTestStore();

        Assert.True(await store.UpdateAsync(new ImmutableTenantInfo("initech-id", "initech2")));
        Assert.Null(await store.GetByIdentifierAsync("initech"));
        Assert.Equal("initech2", (await store.GetAsync("initech-id"))?.Identifier);
    }

    [Fact]
    public void AcceptImmutableTenantsFromOptions()
    {
        var options = new InMemoryStoreOptions<ImmutableTenantInfo>();
        options.Tenants.Add(new ImmutableTenantInfo("initech-id", "initech"));
        var store = new InMemoryStore<ImmutableTenantInfo>(MsOptions.Create(options));

        Assert.Equal("initech-id", store.GetByIdentifierAsync("initech").Result?.Id);
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
    public override Task GetAllTenantsFromStoreAsync() => base.GetAllTenantsFromStoreAsync();
}

public class ConfigurationStoreImmutableTenantShould : ImmutableTenantStoreTestBase
{
    private const string Prefix = "Finbuckle:MultiTenant:Stores:ConfigurationStore";

    protected override Task<IMultiTenantStore<ImmutableTenantInfo>> CreateTestStore()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("ConfigurationStoreTestSettings.json")
            .Build();

        // Constructor-only types need the factory overload on 10.x; the store passes the tenant's own keys overlaid
        // on the Defaults section.
        return Task.FromResult<IMultiTenantStore<ImmutableTenantInfo>>(
            new ConfigurationStore<ImmutableTenantInfo>(configuration, Prefix,
                config => new ImmutableTenantInfo(config["Id"]!, config["Identifier"]!, config["Name"])));
    }

    [Fact]
    public async Task PassTenantValuesToFactory()
    {
        var store = await CreateTestStore();

        var tenant = await store.GetByIdentifierAsync("initech");

        Assert.NotNull(tenant);
        Assert.Equal("initech-id", tenant.Id);
        Assert.Equal("Initech", tenant.Name);
    }

    [Fact]
    public void PassMergedDefaultsToFactory()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("ConfigurationStoreTestSettings.json")
            .Build();

        var seen = new Dictionary<string, string?>();
        _ = new ConfigurationStore<ImmutableTenantInfo>(configuration, Prefix, config =>
        {
            seen[config["Identifier"]!] = config["ConnectionString"];
            return new ImmutableTenantInfo(config["Id"]!, config["Identifier"]!);
        });

        Assert.Equal("Datasource=sample.db", seen["initech"]); // from Defaults
        Assert.Equal("Datasource=lol.db", seen["lol"]); // tenant value overrides Defaults
    }

    [Fact]
    public void ThrowClearErrorWithoutFactory()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("ConfigurationStoreTestSettings.json")
            .Build();

        // Without a factory the default binder cannot set constructor-only properties; previously this surfaced as
        // an ArgumentNullException from the dictionary, now it is a MultiTenantException pointing at the fix.
        var e = Assert.Throws<MultiTenantException>(() => new ConfigurationStore<ImmutableTenantInfo>(configuration));
        Assert.Contains("tenantInfoFactory", e.Message);
    }

    [Fact]
    public void ThrowIfFactoryIsNull()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("ConfigurationStoreTestSettings.json")
            .Build();

        Assert.Throws<ArgumentNullException>(() =>
            new ConfigurationStore<ImmutableTenantInfo>(configuration, Prefix, null!));
    }

    [Fact]
    public void ThrowIfFactoryReturnsNull()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("ConfigurationStoreTestSettings.json")
            .Build();

        Assert.Throws<MultiTenantException>(() =>
            new ConfigurationStore<ImmutableTenantInfo>(configuration, Prefix, _ => null!));
    }

    [Fact]
    public async Task RegisterFactoryOverloadThroughBuilder()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("ConfigurationStoreTestSettings.json")
            .Build();

        var services = new ServiceCollection();
        services.AddMultiTenant<ImmutableTenantInfo>()
            .WithConfigurationStore(configuration, Prefix,
                config => new ImmutableTenantInfo(config["Id"]!, config["Identifier"]!));
        var sp = services.BuildServiceProvider();

        var store = sp.GetRequiredService<IMultiTenantStore<ImmutableTenantInfo>>();
        Assert.IsType<ConfigurationStore<ImmutableTenantInfo>>(store);
        Assert.Equal("initech-id", (await store.GetByIdentifierAsync("initech"))!.Id);
    }

    [Fact]
    public async Task KeepDefaultBindingForSettableTypes()
    {
        // The default (no factory) path is unchanged for settable types: init-only TenantInfo and a set-based type.
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("ConfigurationStoreTestSettings.json")
            .Build();

        var initStore = new ConfigurationStore<TenantInfo>(configuration);
        Assert.Equal("Initech", (await initStore.GetByIdentifierAsync("initech"))!.Name);

        var setStore = new ConfigurationStore<MutableTenantInfo>(configuration);
        Assert.Equal("lol-id", (await setStore.GetByIdentifierAsync("lol"))!.Id);
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
    public override Task UpdateTenantInfoInStore() => Task.CompletedTask;
}

public class EchoStoreImmutableTenantShould
{
    [Fact]
    public async Task ThrowClearErrorWithoutFactory()
    {
        var store = new EchoStore<ImmutableTenantInfo>();

        var e = await Assert.ThrowsAsync<MultiTenantException>(() => store.GetByIdentifierAsync("initech"));
        Assert.Contains("tenantInfoFactory", e.Message);

        e = await Assert.ThrowsAsync<MultiTenantException>(() => store.GetAsync("initech"));
        Assert.Contains("tenantInfoFactory", e.Message);
    }

    [Fact]
    public async Task EchoWithFactory()
    {
        var store = new EchoStore<ImmutableTenantInfo>(identifier => new ImmutableTenantInfo(identifier, identifier));

        var byIdentifier = await store.GetByIdentifierAsync("initech");
        Assert.Equal("initech", byIdentifier!.Id);
        Assert.Equal("initech", byIdentifier.Identifier);

        var byId = await store.GetAsync("initech");
        Assert.Equal("initech", byId!.Id);
        Assert.Equal("initech", byId.Identifier);
    }

    [Fact]
    public void ThrowIfFactoryIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new EchoStore<ImmutableTenantInfo>(null!));
    }

    [Fact]
    public async Task StillUseReflectionForSettableTypes()
    {
        // MutableTenantInfo has public setters; TenantInfo has init setters. Both work without a factory.
        var mutable = new EchoStore<MutableTenantInfo>();
        Assert.Equal("initech", (await mutable.GetByIdentifierAsync("initech"))!.Id);

        var init = new EchoStore<TenantInfo>();
        Assert.Equal("initech", (await init.GetByIdentifierAsync("initech"))!.Id);
    }

    [Fact]
    public async Task RegisterFactoryOverloadThroughBuilder()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<ImmutableTenantInfo>()
            .WithEchoStore(identifier => new ImmutableTenantInfo(identifier, identifier));
        var sp = services.BuildServiceProvider();

        var store = sp.GetRequiredService<IMultiTenantStore<ImmutableTenantInfo>>();
        Assert.Equal("initech", (await store.GetByIdentifierAsync("initech"))!.Id);
    }

    [Fact]
    public async Task RegisterParameterlessOverloadThroughBuilderForSettableTypes()
    {
        // Ensures adding the factory constructor did not break DI activation of the parameterless one.
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>().WithEchoStore();
        var sp = services.BuildServiceProvider();

        var store = sp.GetRequiredService<IMultiTenantStore<TenantInfo>>();
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
            .Returns(new HttpClient(new TestHandler("""{"Id":"initech-id","Identifier":"initech","Name":"Initech"}""")));
        var client = new HttpRemoteStoreClient<ImmutableTenantInfo>(factory.Object);
        var store = new HttpRemoteStore<ImmutableTenantInfo>(client, "https://example.com/{__tenant__}");

        var tenant = await store.GetByIdentifierAsync("initech");

        Assert.NotNull(tenant);
        Assert.Equal("initech-id", tenant.Id);
        Assert.Equal("initech", tenant.Identifier);
        Assert.Equal("Initech", tenant.Name);
    }
}

public class DistributedCacheStoreImmutableTenantShould : ImmutableTenantStoreTestBase
{
    protected override async Task<IMultiTenantStore<ImmutableTenantInfo>> CreateTestStore()
    {
        var services = new ServiceCollection();
        services.AddOptions().AddDistributedMemoryCache();
        var sp = services.BuildServiceProvider();

        var store = new DistributedCacheStore<ImmutableTenantInfo>(sp.GetRequiredService<IDistributedCache>(),
            Constants.TenantToken, TimeSpan.MaxValue);
        return await PopulateTestStore(store);
    }

    [Fact]
    public async Task RoundTripAdditionalProperties()
    {
        var store = await CreateTestStore();

        Assert.Equal("Initech", (await store.GetAsync("initech-id"))!.Name);
        Assert.Equal("Lol, Inc.", (await store.GetByIdentifierAsync("lol"))!.Name);
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

    // GetAllAsync is not implemented by this store.
    public override Task GetAllTenantsFromStoreAsync() => Task.CompletedTask;
    public override Task GetAllTenantsFromStoreAsyncSkip1Take1() => Task.CompletedTask;
}

public class TenantResolutionImmutableTenantShould
{
    [Fact]
    public async Task ResolveThroughDiAndExposeViaContextAccessor()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<ImmutableTenantInfo>()
            .WithStaticStrategy("initech")
            .WithInMemoryStore();
        var sp = services.BuildServiceProvider();

        await sp.GetRequiredService<IMultiTenantStore<ImmutableTenantInfo>>()
            .AddAsync(new ImmutableTenantInfo("initech-id", "initech", "Initech"));

        var context = await sp.GetRequiredService<ITenantResolver<ImmutableTenantInfo>>().ResolveAsync(new object());
        Assert.NotNull(context.TenantInfo);
        Assert.Equal("initech-id", context.TenantInfo.Id);

        sp.GetRequiredService<IMultiTenantContextSetter>().MultiTenantContext = context;
        var accessor = sp.GetRequiredService<IMultiTenantContextAccessor<ImmutableTenantInfo>>();
        Assert.Same(context.TenantInfo, accessor.MultiTenantContext.TenantInfo);
        Assert.True(accessor.MultiTenantContext.IsResolved);
    }

    [Fact]
    public void ResolvePerTenantOptions()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<ImmutableTenantInfo>();
        services.Configure<TestOptions>(options => options.Value = "base");
        services.ConfigurePerTenant<TestOptions, ImmutableTenantInfo>((options, tenant) =>
            options.Value += $":{tenant.Name}");

        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IMultiTenantContextSetter>().MultiTenantContext =
            new MultiTenantContext<ImmutableTenantInfo>(new ImmutableTenantInfo("initech-id", "initech", "Initech"));

        Assert.Equal("base:Initech", provider.GetRequiredService<IOptions<TestOptions>>().Value.Value);
        Assert.Equal("base:Initech", provider.GetRequiredService<IOptionsMonitor<TestOptions>>().CurrentValue.Value);
    }

    [Fact]
    public void SerializeImmutableTenantAsJson()
    {
        // Sanity check of the serialization contract the distributed cache and remote stores rely on.
        var json = JsonSerializer.Serialize(new ImmutableTenantInfo("initech-id", "initech"));
        var roundTripped = JsonSerializer.Deserialize<ImmutableTenantInfo>(json);

        Assert.Equal("initech-id", roundTripped!.Id);
        Assert.Equal("initech", roundTripped.Identifier);
    }

    public class TestOptions
    {
        public string? Value { get; set; }
    }
}
