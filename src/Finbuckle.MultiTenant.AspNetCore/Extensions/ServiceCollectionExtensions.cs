using System;
using Finbuckle.MultiTenant.AspNetCore;
using Finbuckle.MultiTenant.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Configure <c>Finbuckle.MultiTenant</c> services for the application.
        /// </summary>
        /// <param name="services">The <c>IServiceCollection<c/> instance the extension method applies to.</param>
        /// <returns>An new instance of <c>MultiTenantBuilder</c>.</returns>
        public static MultiTenantBuilder AddMultiTenant(this IServiceCollection services)
        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddSingleton<TenantResolver>();
            services.TryAddScoped<TenantContext>(sp => sp.GetRequiredService<IHttpContextAccessor>().HttpContext.GetTenantContextAsync().Result);
            
            return new MultiTenantBuilder(services);
        }
    }
}