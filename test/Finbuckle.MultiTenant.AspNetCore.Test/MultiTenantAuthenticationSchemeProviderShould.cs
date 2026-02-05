// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.AspNetCore.Extensions;
using Finbuckle.MultiTenant.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Finbuckle.MultiTenant.AspNetCore.Test;

public class MultiTenantAuthenticationSchemeProviderShould
{
    [Fact]
    public async Task ReturnPerTenantAuthenticationOptions()
    {
        var services = new ServiceCollection();
        services.AddAuthentication()
            .AddCookie("tenant1Scheme")
            .AddCookie("tenant2Scheme");

        services.AddMultiTenant<TenantInfo>()
            .WithPerTenantAuthentication();

        services.ConfigureAllPerTenant<AuthenticationOptions, TenantInfo>((ao, ti) =>
        {
            ao.DefaultChallengeScheme = ti.Identifier + "Scheme";
        });

        var sp = services.BuildServiceProvider();

        var tenant1 = new TenantInfo { Id = "tenant1", Identifier = "tenant1" };

        var tenant2 = new TenantInfo { Id = "tenant2", Identifier = "tenant2" };

        var mtc = new MultiTenantContext<TenantInfo>(tenant1);
        var setter = sp.GetRequiredService<IMultiTenantContextSetter>();
        setter.MultiTenantContext = mtc;

        var schemeProvider = sp.GetRequiredService<IAuthenticationSchemeProvider>();

        var option = await schemeProvider.GetDefaultChallengeSchemeAsync();

        Assert.NotNull(option);
        Assert.Equal("tenant1Scheme", option.Name);

        mtc = new MultiTenantContext<TenantInfo>(tenant2);
        setter.MultiTenantContext = mtc;
        option = await schemeProvider.GetDefaultChallengeSchemeAsync();

        Assert.NotNull(option);
        Assert.Equal("tenant2Scheme", option.Name);
    }
}