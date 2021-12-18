// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Finbuckle.MultiTenant.AspNetCore
{
    internal class MultiTenantRouteBuilder : IRouteBuilder
    {
        private readonly IServiceProvider serviceProvider;
        private IRouter defaultHandler = new RouteHandler(_ => Task.CompletedTask);

        public MultiTenantRouteBuilder(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public IApplicationBuilder ApplicationBuilder => throw new NotImplementedException();

        public IRouter? DefaultHandler { get => defaultHandler; set => throw new NotImplementedException(); }

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