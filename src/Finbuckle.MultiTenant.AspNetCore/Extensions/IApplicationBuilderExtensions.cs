using System;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for using <c>Finbuckle.MultiTenant.AspNetCore</c>.
    /// </summary>
    public static class IApplicationBuilderExtensions
    {
        /// <summary>
        /// Use <c>Finbuckle.MultiTenant</c> middleware in processing the request.
        /// </summary>
        /// <param name="builder">The <c>IApplicationBuilder<c/> instance the extension method applies to.</param>
        /// <returns>The same <c>IApplicationBuilder</c> passed into the method.</returns>
        public static IApplicationBuilder UseMultiTenant(this IApplicationBuilder builder) =>
                builder.UseMiddleware<MultiTenantMiddleware>();

        /// <summary>
        /// Use Finbuckle.MultiTenant middleware with routing support in processing the request.
        /// </summary>
        /// <param name="builder">The <c>IApplicationBuilder<c/> instance the extension method applies to.</param>
        /// <returns>The same <c>IApplicationBuilder</c> passed into the method.</returns>
        public static IApplicationBuilder UseMultiTenant(this IApplicationBuilder builder, Action<IRouteBuilder> configRoute)
        {
            var rb = new RouteBuilder(builder, new MultiTenantRouteHandler());
            configRoute(rb);

            // insert attribute based routes 
            rb.Routes.Insert(0, AttributeRouting.CreateAttributeMegaRoute(builder.ApplicationServices));

            var routes = rb.Build();

            return builder.UseMiddleware<MultiTenantMiddleware>(routes);
        }
    }
}