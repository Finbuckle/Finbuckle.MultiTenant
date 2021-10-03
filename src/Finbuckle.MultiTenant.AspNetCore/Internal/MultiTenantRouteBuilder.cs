// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

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