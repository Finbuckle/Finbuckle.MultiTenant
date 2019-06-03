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
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.AspNetCore;
using Finbuckle.MultiTenant.Core;
using Finbuckle.MultiTenant.Strategies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class ServiceCollectionExtensionsShould
{
    internal void configTestRoute(Microsoft.AspNetCore.Routing.IRouteBuilder routes)
    {
        routes.MapRoute("Defaut", "{__tenant__=}/{controller=Home}/{action=Index}");
    }

    [Fact]
    public void RegisterTenantInfoInDI()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant();
        
        var service = services.Where(s =>   s.Lifetime == ServiceLifetime.Scoped &&
                                            s.ServiceType == typeof(TenantInfo)).SingleOrDefault();

        Assert.NotNull(service);
    }

    [Fact]
    public void RegisterIHttpContextAccessorInDI()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant();
        
        var service = services.Where(s =>   s.Lifetime == ServiceLifetime.Singleton &&
                                            s.ServiceType == typeof(IHttpContextAccessor)).SingleOrDefault();

        Assert.NotNull(service);
    }

    [Fact]
    public void RegisterIMultitenantContextAccessorInDI()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant();
        
        var service = services.Where(s =>   s.Lifetime == ServiceLifetime.Singleton &&
                                            s.ServiceType == typeof(IMultiTenantContextAccessor)).SingleOrDefault();

        Assert.NotNull(service);
    }
}