//    Copyright 2018 Andrew White
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using Finbuckle.MultiTenant.Core;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

public class MultiTenantOptionsIntegrationShould
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
    public void UsePerTenantWhenUsingIOptionsMonitor()
    {
        var tc = new TenantContext("test-id-123", null, null, null, null, null);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IHttpContextAccessor>(sp => CreateHttpContextAccessorMock(tc));
        serviceCollection.AddMultiTenant().WithPerTenantOptions<CookieAuthenticationOptions>((o, _tc) => o.Cookie.Name = _tc.Id);
        serviceCollection.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();

        var services = serviceCollection.BuildServiceProvider();

        var options = services.GetService<IOptionsMonitor<CookieAuthenticationOptions>>();
        var optionsValue = options.Get(CookieAuthenticationDefaults.AuthenticationScheme);

        Assert.Equal(tc.Id, optionsValue.Cookie.Name);
    }

    [Fact]
    public void UsePerTenantWhenUsingIOptions()
    {
        var tc = new TenantContext("test-id-123", null, null, null, null, null);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IHttpContextAccessor>(sp => CreateHttpContextAccessorMock(tc));
        serviceCollection.AddMultiTenant().WithPerTenantOptions<CookieAuthenticationOptions>((o, _tc) => o.Cookie.Name = _tc.Id);
        serviceCollection.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();

        var services = serviceCollection.BuildServiceProvider();

        var options = services.GetService<IOptions<CookieAuthenticationOptions>>();
        var optionsValue = options.Value;

        Assert.Equal(tc.Id, optionsValue.Cookie.Name);
    }

    [Fact]
    public void UsePerTenantWhenUsingIOptionsSnapshot()
    {
        var tc = new TenantContext("test-id-123", null, null, null, null, null);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IHttpContextAccessor>(sp => CreateHttpContextAccessorMock(tc));
        serviceCollection.AddMultiTenant().WithPerTenantOptions<CookieAuthenticationOptions>((o, _tc) => o.Cookie.Name = _tc.Id);
        serviceCollection.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();

        var services = serviceCollection.BuildServiceProvider();

        var options = services.GetService<IOptionsSnapshot<CookieAuthenticationOptions>>();
        var optionsValue = options.Get(CookieAuthenticationDefaults.AuthenticationScheme);

        Assert.Equal(tc.Id, optionsValue.Cookie.Name);
    }
}