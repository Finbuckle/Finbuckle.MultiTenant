using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            services.AddControllersWithViews();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                    {
                        // Required for Safari 12 issue and OpenID Connect.
                        options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None;
                    })
                    .AddOpenIdConnect(options =>
                    {
                        options.ClientId = "clientId"; // Will be set per-tenant.
                        options.Authority = "https://authorityUrl"; // Will be set per-tenant.
                    });

            services.AddMultiTenant<AuthenticationOptionsSampleTenantInfo>()
                    .WithConfigurationStore()
                    .WithRouteStrategy()
                    .WithPerTenantAuthentication()
                    .WithPerTenantOptions<CookieAuthenticationOptions>((options, tenantInfo) =>
                    {
                        // Note the paths set take our routing strategy into account.
                        options.LoginPath = "/" + tenantInfo.Identifier + "/Home/Login";
                    });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.EnvironmentName == "Development")
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseMultiTenant<AuthenticationOptionsSampleTenantInfo>();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{__tenant__=}/{controller=Home}/{action=Index}");
            });
        }
    }
}