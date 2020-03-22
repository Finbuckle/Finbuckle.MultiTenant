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
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.AspNetCore;
using Finbuckle.MultiTenant.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class AspNetCoreMultiTenantContextAccessorShould
{
    private Mock<HttpContext> CreateHttpContextMock(IServiceProvider serviceProvider)
    {
        var items = new Dictionary<object, object>();

        var mock = new Mock<HttpContext>();
        mock.Setup(c => c.RequestServices).Returns(serviceProvider);

        return mock;
    }
    
    [Fact]
    public void GetMultiTenantContextFromIHttpContextAccessor()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant().WithInMemoryStore().WithStaticStrategy("initech");
        var sp = services.BuildServiceProvider();
        var ti = new TenantInfo("initech", "initech", null, null, null);
        sp.GetService<IMultiTenantStore>().TryAddAsync(ti).Wait();

        var context = CreateHttpContextMock(sp).Object;
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(a => a.HttpContext).Returns(context);

        var mw = new MultiTenantMiddleware(null);
        mw.Invoke(context).Wait();

        var accessor = new AspNetCoreMultiTenantContextAccessor(httpContextAccessorMock.Object);

        Assert.Equal(ti.Id, accessor.MultiTenantContext.TenantInfo.Id);
    }

    [Fact]
    public void ReturnNullIfNoHttpContext()
    {
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(a => a.HttpContext).Returns((HttpContext)null);

        var accessor = new AspNetCoreMultiTenantContextAccessor(httpContextAccessorMock.Object);

        Assert.Null(accessor.MultiTenantContext);
    }
}
