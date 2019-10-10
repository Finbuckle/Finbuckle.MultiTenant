using System;
using EFCoreStoreSample.Data;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EFCoreStoreSample
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddMultiTenant()
                .WithEFCoreStore<AppDbContext, AppTenantInfo>()
                .WithRouteStrategy(ConfigRoutes);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseMultiTenant();
            app.UseMvc(ConfigRoutes);

            // Seed the database the multitenant store will need.
            SetupStore(app.ApplicationServices);
        }

        private void SetupStore(IServiceProvider sp)
        {
            var scopeServices = sp.CreateScope().ServiceProvider;
            var store = scopeServices.GetRequiredService<IMultiTenantStore>();

            store.TryAddAsync(new TenantInfo("tenant-finbuckle-d043favoiaw", "finbuckle", "Finbuckle", "finbuckle_conn_string", null)).Wait();
            store.TryAddAsync(new TenantInfo("tenant-initech-341ojadsfa", "initech", "Initech LLC", "initech_conn_string", null)).Wait();
        }

        private void ConfigRoutes(IRouteBuilder routes)
        {
            routes.MapRoute("Defaut", "{__tenant__=}/{controller=Home}/{action=Index}");
        }
    }
}