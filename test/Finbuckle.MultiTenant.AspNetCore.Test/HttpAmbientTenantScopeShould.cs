// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Reflection;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.AspNetCore.Extensions;
using Finbuckle.MultiTenant.AspNetCore.Options;
using Finbuckle.MultiTenant.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;
using MsOptions = Microsoft.Extensions.Options.Options;

namespace Finbuckle.MultiTenant.AspNetCore.Test;

public class HttpAmbientTenantScopeShould
{
    private const string NoTenant = "none";

    private static async Task<IHost> CreateHostAsync(Action<IApplicationBuilder>? beforeMultiTenant = null,
        params string[] identifiers)
    {
        var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder => webBuilder
                .UseTestServer()
                .ConfigureServices(services =>
                {
                    services.AddMultiTenant<TenantInfo, string>()
                        .WithHeaderStrategy()
                        .WithInMemoryStore();
                })
                .Configure(app =>
                {
                    beforeMultiTenant?.Invoke(app);
                    app.UseMultiTenant<string>();
                    app.Run(async context =>
                    {
                        // Yield so concurrent requests interleave on the thread pool.
                        await Task.Yield();
                        var tenantInfo = context.GetTenantInfo<TenantInfo, string>();
                        await context.Response.WriteAsync(tenantInfo?.Id ?? NoTenant);
                    });
                }))
            .StartAsync();

        using var scope = host.Services.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<TenantManager<TenantInfo, string>>();
        foreach (var identifier in identifiers)
            await manager.AddAsync(new TenantInfo { Id = $"{identifier}-id", Identifier = identifier });

        return host;
    }

    private static Task<string> GetForTenantAsync(HttpClient client, string? identifier)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/");
        if (identifier is not null)
            request.Headers.Add(Constants.TenantToken, identifier);
        return client.SendAsync(request).ContinueWith(t => t.Result.Content.ReadAsStringAsync()).Unwrap();
    }

    [Fact]
    public async Task EstablishScopeBeforeTheMultiTenantMiddlewareRuns()
    {
        bool? hadScope = null;
        bool? wasResolved = null;
        using var host = await CreateHostAsync(app => app.Use(async (context, next) =>
        {
            hadScope = context.RequestServices.GetRequiredService<ITenantScopeProvider>().HasScope;
            wasResolved = context.GetTenantContext<TenantInfo, string>().IsResolved;
            await next();
        }), "initech");

        var response = await GetForTenantAsync(host.GetTestClient(), "initech");

        Assert.True(hadScope);
        Assert.False(wasResolved);
        Assert.Equal("initech-id", response);
    }

    [Fact]
    public async Task KeepTenantSetEarlierInThePipeline()
    {
        using var host = await CreateHostAsync(app => app.Use(async (context, next) =>
        {
            context.SetTenantInfo<TenantInfo, string>(new TenantInfo { Id = "early-id", Identifier = "early" });
            await next();
        }), "initech");

        var response = await GetForTenantAsync(host.GetTestClient(), "initech");

        Assert.Equal("early-id", response);
    }

    [Fact]
    public async Task BeginAFreshScopeForEachRequest()
    {
        using var host = await CreateHostAsync(null, "initech", "megacorp");
        var client = host.GetTestClient();

        Assert.Equal("initech-id", await GetForTenantAsync(client, "initech"));
        Assert.Equal(NoTenant, await GetForTenantAsync(client, null));
        Assert.Equal("megacorp-id", await GetForTenantAsync(client, "megacorp"));
    }

    [Fact]
    public async Task IsolateConcurrentRequests()
    {
        var identifiers = Enumerable.Range(0, 25).Select(i => $"tenant{i}").ToArray();
        using var host = await CreateHostAsync(null, identifiers);
        var client = host.GetTestClient();

        var responses = await Task.WhenAll(identifiers.Select(id => GetForTenantAsync(client, id)));

        Assert.Equal(identifiers.Select(id => $"{id}-id"), responses);
    }

    [Fact]
    public async Task ThrowAtStartupWhenScopeIsNotConfigured()
    {
        var hostBuilder = new HostBuilder()
            .ConfigureWebHost(webBuilder => webBuilder
                .UseTestServer()
                .ConfigureServices(services =>
                    services.AddMultiTenant<TenantInfo, string>().WithStaticStrategy("initech").WithInMemoryStore())
                .Configure(app => app.UseMultiTenant<string>()));

        var ex = await Assert.ThrowsAsync<MultiTenantException>(() => hostBuilder.StartAsync());
        Assert.Contains("AddHttpAmbientTenantScope()", ex.Message);
    }

    [Fact]
    public async Task ThrowFromMiddlewareWhenNoScopeIsActive()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo, string>().WithStaticStrategy("initech").WithInMemoryStore();
        var sp = services.BuildServiceProvider();
        var mw = new MultiTenantMiddleware<string>(_ => Task.CompletedTask,
            MsOptions.Create(new BypassWhenOptions()),
            MsOptions.Create(new ShortCircuitWhenOptions<string>()));

        // Run on a fresh execution context so no ambient scope from another test can leak in.
        var ex = await Task.Run(() => Assert.ThrowsAsync<MultiTenantException>(() =>
            mw.Invoke(new DefaultHttpContext { RequestServices = sp },
                sp.GetRequiredService<ITenantContext<string>>(),
                sp.GetRequiredService<ITenantResolver<string>>(),
                sp.GetRequiredService<ITenantScopeProvider>())));
        Assert.Contains("AddHttpAmbientTenantScope()", ex.Message);
    }

    [Fact]
    public void RegisterDefaultFactoryWhenNoneIsRegistered()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo, string>().AddHttpAmbientTenantScope();
        var sp = services.BuildServiceProvider();

        var factory = Assert.IsType<TenantScopeHttpContextFactory>(sp.GetRequiredService<IHttpContextFactory>());
        Assert.IsType<DefaultHttpContextFactory>(GetInner(factory));
    }

    [Fact]
    public void DecorateAnExistingFactoryOnlyOnceWhenCalledRepeatedly()
    {
        var services = new ServiceCollection();
        var existing = new Mock<IHttpContextFactory>().Object;
        services.AddSingleton(existing);
        services.AddMultiTenant<TenantInfo, string>()
            .AddHttpAmbientTenantScope()
            .AddHttpAmbientTenantScope()
            .WithHeaderStrategy()
            .WithHostStrategy();
        var sp = services.BuildServiceProvider();

        Assert.Single(services, s => s.ServiceType == typeof(IHttpContextFactory) && !s.IsKeyedService);
        var factory = Assert.IsType<TenantScopeHttpContextFactory>(sp.GetRequiredService<IHttpContextFactory>());
        Assert.Same(existing, GetInner(factory));
    }

    [Fact]
    public async Task BeginScopeOnCreateAndEndItOnDispose()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo, string>().AddHttpAmbientTenantScope();
        var sp = services.BuildServiceProvider();
        var factory = sp.GetRequiredService<IHttpContextFactory>();
        var scopeProvider = sp.GetRequiredService<ITenantScopeProvider>();

        await Task.Run(() =>
        {
            Assert.False(scopeProvider.HasScope);
            var context = factory.Create(new FeatureCollection());
            Assert.True(scopeProvider.HasScope);
            Assert.False(sp.GetRequiredService<ITenantContext<TenantInfo, string>>().IsResolved);

            factory.Dispose(context);
            Assert.False(scopeProvider.HasScope);
        });
    }

    [Fact]
    public async Task EndScopeEvenWhenInnerDisposeThrows()
    {
        var inner = new Mock<IHttpContextFactory>();
        inner.Setup(f => f.Create(It.IsAny<IFeatureCollection>())).Returns(new DefaultHttpContext());
        inner.Setup(f => f.Dispose(It.IsAny<HttpContext>())).Throws<InvalidOperationException>();
        var scopeProvider = new AmbientTenantContext<TenantInfo, string>();
        var factory = new TenantScopeHttpContextFactory(inner.Object, scopeProvider);

        await Task.Run(() =>
        {
            var context = factory.Create(new FeatureCollection());
            Assert.Throws<InvalidOperationException>(() => factory.Dispose(context));
            Assert.False(scopeProvider.HasScope);
        });
    }

    public static TheoryData<string> HelperNames => new(Helpers.Keys);

    private static readonly Dictionary<string, Action<MultiTenantBuilder<TenantInfo, string>>> Helpers = new()
    {
        ["BypassWhen"] = b => b.BypassWhen(_ => { }),
        ["BypassWhenEndpointNotResolved"] = b => b.BypassWhenEndpointNotResolved(),
        ["ShortCircuitWhen"] = b => b.ShortCircuitWhen(_ => { }),
        ["ShortCircuitWhenTenantNotResolved"] = b => b.ShortCircuitWhenTenantNotResolved(),
        ["ShortCircuitWhenTenantNotResolved(Uri)"] = b => b.ShortCircuitWhenTenantNotResolved(new Uri("/", UriKind.Relative)),
        ["WithPerTenantAuthentication"] = b => b.WithPerTenantAuthentication(),
        ["WithPerTenantAuthentication(config)"] = b => b.WithPerTenantAuthentication(_ => { }),
        ["WithPerTenantAuthenticationConventions"] = b => b.WithPerTenantAuthenticationConventions(),
        ["WithPerTenantAuthenticationCore"] = b => b.WithPerTenantAuthenticationCore(),
        ["WithSessionStrategy"] = b => b.WithSessionStrategy(),
        ["WithSessionStrategy(key)"] = b => b.WithSessionStrategy("key"),
        ["WithRemoteAuthenticationCallbackStrategy"] = b => b.WithRemoteAuthenticationCallbackStrategy(),
        ["WithBasePathStrategy"] = b => b.WithBasePathStrategy(),
        ["WithBasePathStrategy(config)"] = b => b.WithBasePathStrategy(_ => { }),
        ["WithRouteStrategy"] = b => b.WithRouteStrategy(),
        ["WithRouteStrategy(param, ambient)"] = b => b.WithRouteStrategy("tenant", false),
        ["WithHostStrategy"] = b => b.WithHostStrategy(),
        ["WithHostStrategy(template)"] = b => b.WithHostStrategy("__tenant__.example.com"),
        ["WithClaimStrategy"] = b => b.WithClaimStrategy(),
        ["WithClaimStrategy(key)"] = b => b.WithClaimStrategy("key"),
        ["WithClaimStrategy(key, scheme)"] = b => b.WithClaimStrategy("key", "scheme"),
        ["WithHttpContextStrategy"] = b => b.WithHttpContextStrategy(_ => Task.FromResult<string?>(null)),
        ["WithHeaderStrategy"] = b => b.WithHeaderStrategy(),
        ["WithHeaderStrategy(key)"] = b => b.WithHeaderStrategy("key"),
    };

    [Theory]
    [MemberData(nameof(HelperNames))]
    public void BeAddedByEveryAspNetCoreHelper(string helper)
    {
        var services = new ServiceCollection();
        services.AddRouting();
        services.AddAuthentication();
        Helpers[helper](services.AddMultiTenant<TenantInfo, string>());
        var sp = services.BuildServiceProvider();

        Assert.IsType<TenantScopeHttpContextFactory>(sp.GetRequiredService<IHttpContextFactory>());
    }

    private static object? GetInner(TenantScopeHttpContextFactory factory) =>
        typeof(TenantScopeHttpContextFactory)
            .GetField("inner", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(factory);
}
