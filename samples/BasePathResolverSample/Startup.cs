﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BasePathResolverSample
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
            services.AddMvc();
            services.AddMultiTenant().
                WithInMemoryStore(Configuration.GetSection("Finbuckle:MultiTenant:InMemoryMultiTenantStore")).
                WithBasePathStrategy();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMultiTenant();

            app.UseMvc(routes => routes.MapRoute("Default", "{first_segment=}/{controller=Home}/{action=Index}"));
        }
    }
}