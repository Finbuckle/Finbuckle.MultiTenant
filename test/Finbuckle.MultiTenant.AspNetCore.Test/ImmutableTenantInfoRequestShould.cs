// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.AspNetCore.Extensions;
using Finbuckle.MultiTenant.AspNetCore.Options;
using Finbuckle.MultiTenant.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using MsOptions = Microsoft.Extensions.Options.Options;

namespace Finbuckle.MultiTenant.AspNetCore.Test;

// Proves the request pipeline works end to end with a constructor-only immutable tenant type and that the active
// tenant of a request cannot be replaced once the middleware has resolved it (issue #836).
public class ImmutableTenantInfoRequestShould
{
    private static async Task<(ServiceProvider sp, MultiTenantMiddleware<string> mw)> CreateRequestPipeline(
        RequestDelegate next, string identifierToResolve = "initech")
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<ImmutableTenantInfo, string>()
            .WithStaticStrategy(identifierToResolve)
            .WithInMemoryStore();
        var sp = services.BuildServiceProvider();
        await sp.GetRequiredService<IMultiTenantStore<ImmutableTenantInfo, string>>()
            .AddAsync(new ImmutableTenantInfo("initech-id", "initech", "Initech"));

        var mw = new MultiTenantMiddleware<string>(next,
            MsOptions.Create(new BypassWhenOptions()),
            MsOptions.Create(new ShortCircuitWhenOptions<string>()));
        return (sp, mw);
    }

    private static Task Invoke(MultiTenantMiddleware<string> mw, HttpContext context, IServiceProvider sp) =>
        mw.Invoke(context,
            sp.GetRequiredService<ITenantContext<string>>(),
            sp.GetRequiredService<ITenantResolver<string>>(),
            sp.GetRequiredService<ITenantScopeProvider>());

    [Fact]
    public async Task ResolveImmutableTenantForRequest()
    {
        ImmutableTenantInfo? observed = null;
        var (sp, mw) = await CreateRequestPipeline(ctx =>
        {
            observed = ctx.GetTenantInfo<ImmutableTenantInfo, string>();
            return Task.CompletedTask;
        });

        await Invoke(mw, new DefaultHttpContext { RequestServices = sp }, sp);

        Assert.NotNull(observed);
        Assert.Equal("initech-id", observed.Id);
        Assert.Equal("Initech", observed.Name);
    }

    [Fact]
    public async Task NotAllowReplacingResolvedTenantDownstream()
    {
        MultiTenantException? thrown = null;
        ImmutableTenantInfo? observedAfter = null;
        var (sp, mw) = await CreateRequestPipeline(ctx =>
        {
            thrown = Assert.Throws<MultiTenantException>(() =>
                ctx.SetTenantInfo<ImmutableTenantInfo, string>(new ImmutableTenantInfo("other-id", "other")));
            observedAfter = ctx.GetTenantInfo<ImmutableTenantInfo, string>();
            return Task.CompletedTask;
        });

        await Invoke(mw, new DefaultHttpContext { RequestServices = sp }, sp);

        Assert.NotNull(thrown);
        Assert.Equal("initech-id", observedAfter!.Id);
    }

    [Fact]
    public async Task IgnoreTrySetTenantInfoWhenTenantAlreadyResolved()
    {
        ImmutableTenantInfo? observedAfter = null;
        var (sp, mw) = await CreateRequestPipeline(ctx =>
        {
            ctx.TrySetTenantInfo<ImmutableTenantInfo, string>(new ImmutableTenantInfo("other-id", "other"));
            observedAfter = ctx.GetTenantInfo<ImmutableTenantInfo, string>();
            return Task.CompletedTask;
        });

        await Invoke(mw, new DefaultHttpContext { RequestServices = sp }, sp);

        Assert.Equal("initech-id", observedAfter!.Id);
    }

    [Fact]
    public async Task AllowSettingTenantDownstreamWhenNotResolved()
    {
        ImmutableTenantInfo? observedAfter = null;
        var (sp, mw) = await CreateRequestPipeline(ctx =>
        {
            Assert.Null(ctx.GetTenantInfo<ImmutableTenantInfo, string>());
            ctx.SetTenantInfo<ImmutableTenantInfo, string>(new ImmutableTenantInfo("manual-id", "manual"));
            observedAfter = ctx.GetTenantInfo<ImmutableTenantInfo, string>();
            return Task.CompletedTask;
        }, identifierToResolve: "unknown");

        await Invoke(mw, new DefaultHttpContext { RequestServices = sp }, sp);

        Assert.Equal("manual-id", observedAfter!.Id);
    }
}
