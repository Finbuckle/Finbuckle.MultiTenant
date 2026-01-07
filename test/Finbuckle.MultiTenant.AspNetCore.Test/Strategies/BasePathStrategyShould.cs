// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.AspNetCore.Extensions;
using Finbuckle.MultiTenant.AspNetCore.Options;
using Finbuckle.MultiTenant.AspNetCore.Strategies;
using Finbuckle.MultiTenant.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Finbuckle.MultiTenant.AspNetCore.Test.Strategies;

public class BasePathStrategyShould
{
    private HttpContext CreateHttpContextMock(string path, string pathBase = "/")
    {
        var mock = new Mock<HttpContext>();
        mock.SetupProperty<PathString>(c => c.Request.Path, path);
        mock.SetupProperty<PathString>(c => c.Request.PathBase, pathBase);
        mock.SetupProperty(c => c.RequestServices);
        return mock.Object;
    }

    [Fact]
    public async Task RebaseAspNetCoreBasePathIfOptionTrue()
    {
        var services = new ServiceCollection();
        services.AddOptions().AddMultiTenant<TenantInfo>().WithBasePathStrategy().WithInMemoryStore(options =>
        {
            options.Tenants.Add(new TenantInfo { Id = "base123", Identifier = "base", Name = "base tenant" });
        });
        services.Configure<BasePathStrategyOptions>(options => options.RebaseAspNetCorePathBase = true);
        var serviceProvider = services.BuildServiceProvider();
        var httpContext = CreateHttpContextMock("/base/notBase");
        httpContext.RequestServices = serviceProvider;

        Assert.Equal("/", httpContext.Request.PathBase);
        Assert.Equal("/base/notBase", httpContext.Request.Path);

        // will trigger OnTenantFound event...
        await serviceProvider.GetRequiredService<ITenantResolver>().ResolveAsync(httpContext);

        Assert.Equal("/base", httpContext.Request.PathBase);
        Assert.Equal("/notBase", httpContext.Request.Path);
    }

    [Fact]
    public async Task NotRebaseAspNetCoreBasePathIfOptionFalse()
    {
        var services = new ServiceCollection();
        services.AddOptions().AddMultiTenant<TenantInfo>().WithBasePathStrategy().WithInMemoryStore(options =>
        {
            options.Tenants.Add(new TenantInfo { Id = "base123", Identifier = "base", Name = "base tenant" });
        });
        services.Configure<BasePathStrategyOptions>(options => options.RebaseAspNetCorePathBase = false);
        var serviceProvider = services.BuildServiceProvider();
        var httpContext = CreateHttpContextMock("/base/notBase");
        httpContext.RequestServices = serviceProvider;

        Assert.Equal("/", httpContext.Request.PathBase);
        Assert.Equal("/base/notBase", httpContext.Request.Path);

        // will trigger OnTenantFound event...
        await serviceProvider.GetRequiredService<ITenantResolver>().ResolveAsync(httpContext);

        Assert.Equal("/", httpContext.Request.PathBase);
        Assert.Equal("/base/notBase", httpContext.Request.Path);
    }

    [Theory]
    [InlineData("/test", "test")] // single path
    [InlineData("/Test", "Test")] // maintain case
    [InlineData("", null)] // no path
    [InlineData("/", null)] // just trailing slash
    [InlineData("/initech/ignore/ignore", "initech")] // multiple path segments
    public async Task ReturnExpectedIdentifier(string path, string? expected)
    {
        var httpContext = CreateHttpContextMock(path);
        var strategy = new BasePathStrategy();

        var identifier = await strategy.GetIdentifierAsync(httpContext);

        Assert.Equal(expected, identifier);
    }

    [Fact]
    public async Task ReturnNullIfContextIsNotHttpContext()
    {
        var context = new object();
        var strategy = new BasePathStrategy();

        Assert.Null(await strategy.GetIdentifierAsync(context));
    }

    [Fact]
    public async Task AppendTenantToExistingBase()
    {
        var services = new ServiceCollection();
        services.AddOptions().AddMultiTenant<TenantInfo>().WithBasePathStrategy().WithInMemoryStore(options =>
        {
            options.Tenants.Add(new TenantInfo { Id = "tenant", Identifier = "tenant", Name = "tenant" });
        });
        services.Configure<BasePathStrategyOptions>(options => options.RebaseAspNetCorePathBase = true);
        var serviceProvider = services.BuildServiceProvider();
        var httpContext = CreateHttpContextMock("/tenant/path", "/base");
        httpContext.RequestServices = serviceProvider;

        Assert.Equal("/base", httpContext.Request.PathBase);
        Assert.Equal("/tenant/path", httpContext.Request.Path);

        // will trigger OnTenantFound event...
        await serviceProvider.GetRequiredService<ITenantResolver>().ResolveAsync(httpContext);

        Assert.Equal("/base/tenant", httpContext.Request.PathBase);
        Assert.Equal("/path", httpContext.Request.Path);
    }
}