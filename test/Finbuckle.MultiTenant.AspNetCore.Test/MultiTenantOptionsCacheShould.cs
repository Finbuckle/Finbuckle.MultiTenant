using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.AspNetCore;
using Finbuckle.MultiTenant.Core;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

public class MultiTenantOptionsCacheShould
{
    private IHttpContextAccessor CreateHttpContextAccessorMock(TenantContext tenantContext)
    {
        var httpContextMock = new Mock<HttpContext>();
        object tc = tenantContext;
        httpContextMock.Setup(c => c.Items.TryGetValue(Finbuckle.MultiTenant.AspNetCore.Constants.HttpContextTenantContext, out tc)).Returns(true);
        
        var mock = new Mock<IHttpContextAccessor>();
        mock.SetupGet(c => c.HttpContext).Returns(httpContextMock.Object);

        return mock.Object;
    }

    [Fact]
    public void AdjustOptionsWithConfiguredAction()
    {
        var tc = new TenantContext("test-id-123", null, null, null, null, null);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IHttpContextAccessor>(sp => CreateHttpContextAccessorMock(tc));
        serviceCollection.AddMultiTenant().WithPerTenantOptionsConfig<CookieAuthenticationOptions>((o, _tc) => o.Cookie.Name = _tc.Id);     
        serviceCollection.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).
                AddCookie(o => { o.Cookie.Name = CookieAuthenticationDefaults.CookiePrefix + "__tenant__"; });

        var services = serviceCollection.BuildServiceProvider();

        var options = services.GetService<IOptionsMonitor<CookieAuthenticationOptions>>();
        var optionsValue = options.Get(CookieAuthenticationDefaults.AuthenticationScheme);

        Assert.Equal(tc.Id, optionsValue.Cookie.Name);
    }
}