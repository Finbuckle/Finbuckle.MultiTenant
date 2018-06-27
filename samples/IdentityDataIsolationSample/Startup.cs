using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IdentityDataIsolationSample.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace IdentityDataIsolationSample
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
            // Register the db context, but do not specify a provider/connection
            // string since these vary by tenant.
            services.AddDbContext<ApplicationDbContext>();

            services.AddDefaultIdentity<MultiTenantIdentityUser>()
                    .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddRazorPagesOptions(options =>
                {
                    //options.Conventions.
                });

            services.AddMultiTenant()
                .WithRouteStrategy()
                .WithInMemoryStore(Configuration.GetSection("Finbuckle:MultiTenant:InMemoryMultiTenantStore"))
                .WithPerTenantOptions<CookieAuthenticationOptions>( (options, tenantContext) =>
                {
                    // Since we are using the route strategy configure each tenant
                    // to have a different cookie name and path.
                    options.Cookie.Name = $"{tenantContext.Id}:{options.Cookie.Name}";
                    options.LoginPath = $"/{tenantContext.Identifier}/Home/Login";
                    options.Cookie.Path = $"/{tenantContext.Identifier}";
                });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseMultiTenant(ConfigRoutes);
            app.UseAuthentication();
            app.UseMvc(routes =>
            {
                ConfigRoutes(routes);
            });
        }

        private static void ConfigRoutes(Microsoft.AspNetCore.Routing.IRouteBuilder routes)
        {
            routes.MapRoute("Defaut", "{__tenant__=}/{controller=Home}/{action=Index}");
        }
    }
}
