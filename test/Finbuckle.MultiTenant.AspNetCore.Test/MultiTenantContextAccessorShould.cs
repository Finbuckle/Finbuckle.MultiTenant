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

using System.Collections.Generic;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.AspNetCore;
using Finbuckle.MultiTenant.Core;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

public class MultiTenantContextAccessorShould
{
    [Fact]
    public void GetMultiTenantContextFromIHttpAccessor()
    {
        var items = new Dictionary<object, object>();
        var ti = new TenantInfo("test", null, null, null, null);
        var tc = new MultiTenantContext();
        tc.TenantInfo = ti;
        items.Add(Finbuckle.MultiTenant.AspNetCore.Constants.HttpContextMultiTenantContext, tc);

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.Items).Returns(items);

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContextMock.Object);

        var accessor = new MultiTenantContextAccessor(httpContextAccessorMock.Object);

        Assert.Equal(ti.Id, accessor.MultiTenantContext.TenantInfo.Id);
    }

    [Fact]
    public void ReturnNullIfNoHttpContext()
    {
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(a => a.HttpContext).Returns((HttpContext)null);

        var accessor = new MultiTenantContextAccessor(httpContextAccessorMock.Object);

        Assert.Null(accessor.MultiTenantContext);
    }
}