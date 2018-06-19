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

using System;
using System.Collections.Generic;
using Finbuckle.MultiTenant.AspNetCore;
using Finbuckle.MultiTenant.Core;
using Finbuckle.MultiTenant.Core.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class MultiTenantMiddlewareShould
{
    private Mock<HttpContext> CreateHttpContextMock(IServiceProvider serviceProvider)
    {
        var items = new Dictionary<object, object>();

        var mock = new Mock<HttpContext>();
        mock.Setup(c => c.RequestServices).Returns(serviceProvider);
        mock.Setup(c => c.Items).Returns(items);

        return mock;
    }

    [Fact]
    public void ResolveTenantRequest()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant().WithInMemoryStore().WithStaticStrategy("initech");
        var sp = services.BuildServiceProvider();
        var tc = new TenantContext("initech", "initech", null, null, null, null);
        sp.GetService<IMultiTenantStore>().TryAdd(tc);

        var context = CreateHttpContextMock(sp).Object;

        var mw = new MultiTenantMiddleware(null);
        mw.Invoke(context).Wait();

        var resolveTenantContext = (TenantContext)context.Items[Finbuckle.MultiTenant.AspNetCore.Constants.HttpContextTenantContext];
        Assert.Equal("initech", resolveTenantContext.Id);
    }

    [Fact]
    public void SetContextItemToNullIfNoTenant()
    {
        var services = new ServiceCollection();
        services.AddAuthentication();
        services.AddMultiTenant().WithInMemoryStore().WithStaticStrategy("initech");
        services.AddTransient<IAuthenticationHandlerProvider, AuthenticationHandlerProvider>();
        services.AddTransient<IAuthenticationSchemeProvider, AuthenticationSchemeProvider>();
        var sp = services.BuildServiceProvider();

        var mock = CreateHttpContextMock(sp);
        var context = mock.Object;

        var mw = new MultiTenantMiddleware(null);
        mw.Invoke(context).Wait();

        var resolveTenantContext = (TenantContext)context.Items[Finbuckle.MultiTenant.AspNetCore.Constants.HttpContextTenantContext];
        Assert.Null(resolveTenantContext);
    }
}