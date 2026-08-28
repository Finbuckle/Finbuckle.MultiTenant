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

/// <summary>
/// A constructor-only <see cref="ITenantInfo"/> implementation (no setters, not derived from <see cref="TenantInfo"/>,
/// not a record) used to prove the ASP.NET Core integration only depends on the interface contract.
/// </summary>
public sealed class ImmutableTenantInfo(string id, string identifier, string? name = null) : ITenantInfo
{
    public string Id { get; } = id;
    public string Identifier { get; } = identifier;
    public string? Name { get; } = name;
}

// Proves the request pipeline works end to end with a constructor-only immutable tenant type (issue #836).
public class ImmutableTenantInfoRequestShould
{
    private static async Task<(ServiceProvider sp, MultiTenantMiddleware mw)> CreateRequestPipeline(
        RequestDelegate next, string identifierToResolve = "initech")
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<ImmutableTenantInfo>()
            .WithStaticStrategy(identifierToResolve)
            .WithInMemoryStore();
        var sp = services.BuildServiceProvider();
        await sp.GetRequiredService<IMultiTenantStore<ImmutableTenantInfo>>()
            .AddAsync(new ImmutableTenantInfo("initech-id", "initech", "Initech"));

        var mw = new MultiTenantMiddleware(next,
            MsOptions.Create(new BypassWhenOptions()),
            MsOptions.Create(new ShortCircuitWhenOptions()));
        return (sp, mw);
    }

    [Fact]
    public async Task ResolveImmutableTenantForRequest()
    {
        ImmutableTenantInfo? observed = null;
        var (sp, mw) = await CreateRequestPipeline(ctx =>
        {
            observed = ctx.GetTenantInfo<ImmutableTenantInfo>();
            return Task.CompletedTask;
        });

        await mw.Invoke(new DefaultHttpContext { RequestServices = sp });

        Assert.NotNull(observed);
        Assert.Equal("initech-id", observed.Id);
        Assert.Equal("Initech", observed.Name);
    }

    [Fact]
    public async Task SetImmutableTenantManually()
    {
        ImmutableTenantInfo? observed = null;
        var (sp, mw) = await CreateRequestPipeline(ctx =>
        {
            Assert.Null(ctx.GetTenantInfo<ImmutableTenantInfo>());
            ctx.SetTenantInfo(new ImmutableTenantInfo("manual-id", "manual"), false);
            observed = ctx.GetTenantInfo<ImmutableTenantInfo>();
            return Task.CompletedTask;
        }, identifierToResolve: "unknown");

        await mw.Invoke(new DefaultHttpContext { RequestServices = sp });

        Assert.Equal("manual-id", observed!.Id);
    }
}
