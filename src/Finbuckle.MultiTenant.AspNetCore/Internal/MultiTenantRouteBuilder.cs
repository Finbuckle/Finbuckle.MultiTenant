//    Copyright 2018-2020 Finbuckle LLC, Andrew White, and Contributors
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
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Finbuckle.MultiTenant.AspNetCore
{
    internal class MultiTenantRouteBuilder : IRouteBuilder
    {
        private readonly IServiceProvider serviceProvider;
        private IRouter defaultHandler = new RouteHandler(context => null);

        public MultiTenantRouteBuilder(IServiceProvider ServiceProvider)
        {
            if (ServiceProvider == null)
            {
                throw new ArgumentNullException(nameof(ServiceProvider));
            }

            serviceProvider = ServiceProvider;
        }

        public IApplicationBuilder ApplicationBuilder => throw new NotImplementedException();

        public IRouter DefaultHandler { get => defaultHandler; set => throw new NotImplementedException(); }

        public IServiceProvider ServiceProvider => serviceProvider;

        public IList<IRouter> Routes { get; } = new List<IRouter>();

        public IRouter Build()
        {
            var routeCollection = new RouteCollection();

            foreach (var route in Routes)
            {
                routeCollection.Add(route);
            }

            return routeCollection;
        }
    }
}