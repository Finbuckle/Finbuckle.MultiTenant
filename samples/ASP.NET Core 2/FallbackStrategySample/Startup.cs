﻿using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FallbackStrategySample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddMultiTenant<TenantInfo>().
                WithConfigurationStore().
                WithRouteStrategy(ConfigRoutes).
                WithFallbackStrategy("finbuckle");
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseMultiTenant<TenantInfo>();

            // Note this is the same route delegate used in WithFallbackStrategy()
            app.UseMvc(ConfigRoutes);
        }

        private void ConfigRoutes(IRouteBuilder routes)
        {
            routes.MapRoute("Defaut", "{__tenant__=}/{controller=Home}/{action=Index}");
        }
    }
}