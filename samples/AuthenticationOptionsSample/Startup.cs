
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AuthenticationOptionsSample
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
            
            services.AddMultiTenant().
                WithInMemoryStore(Configuration.GetSection("Finbuckle:MultiTenant:InMemoryMultiTenantStore")).
                WithRouteStrategy().
                WithPerTenantOptions<CookieAuthenticationOptions>((o, tc) => o.Cookie.Name += tc.Id);

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).
                AddCookie(o =>
                {
                    o.Cookie.Name = "MyAppCookie.";
                });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMultiTenant(ConfigRoutes);

            app.UseAuthentication();

            app.UseMvc(ConfigRoutes);
        }

        private void ConfigRoutes(IRouteBuilder routes)
        {
            routes.MapRoute("Default", "{__tenant__=}/{controller=Home}/{action=Index}");
        }
    }
}