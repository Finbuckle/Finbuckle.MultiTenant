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
                // Register our custom IMultiTenantStore.
                // Specify a scoped lifetime because the DbContext used is also scoped (added by AddDbContext below).
                // See the DatabaseStore class for details.
                .WithStore<DatabaseStore>(ServiceLifetime.Scoped)
                .WithRouteStrategy(ConfigRoutes);

            // Register the DbContext as usual (note the service is registered with a scope lifetime)...
            services.AddDbContext<MultiTenantStoreDbContext>(options =>
            {
                options.UseSqlite("Data Source=Data/MultiTenantData.db");
            });
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