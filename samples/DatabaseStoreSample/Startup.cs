using System;
using DatabaseStoreSample.Data;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DatabaseStoreSample
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
            services.AddMultiTenant()
                // Register our custom IMultiTenantStore. See the DatabaseStore class for details.
                .WithStore<DatabaseStore>("Data Source=Data/MultiTenantData.db")
                .WithRouteStrategy(ConfigRoutes);

            // Note in this example the DbContext used by the multitenant store is not registered as a service.
            // This is because a store is a singleton and services.AddDbContext adds a scoped service.
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
            SetupDb();
        }

        private void SetupDb()
        {
            var options = (new DbContextOptionsBuilder()).UseSqlite("Data Source=Data/MultiTenantData.db").Options;
            using (var db = new MultiTenantStoreDbContext(options))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
                db.TenantInfo.Add(new TenantInfo("tenant-finbuckle-d043favoiaw", "finbuckle", "Finbuckle", "finbuckle_conn_string", null));
                db.TenantInfo.Add(new TenantInfo("tenant-initech-341ojadsfa", "initech", "Initech LLC", "initech_conn_string", null));
                db.SaveChanges();
            }
        }

        private void ConfigRoutes(IRouteBuilder routes)
        {
            routes.MapRoute("Defaut", "{__tenant__=}/{controller=Home}/{action=Index}");
        }
    }
}