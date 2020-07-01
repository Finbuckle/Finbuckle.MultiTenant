using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using SharedLoginSample.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Identity.UI;

namespace SharedLoginSample
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

            services.AddIdentity<IdentityUser, IdentityRole>()
                    .AddDefaultTokenProviders()
                    .AddDefaultUI()
                    .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddRazorPagesOptions(options =>
                {
                    // Since we are using the route multitenant strategy we must add the
                    // route parameter to the Pages conventions used by Identity.
                    options.Conventions.AddAreaFolderRouteModelConvention("Identity", "/Account", model =>
                    {
                        foreach (var selector in model.Selectors)
                        {
                            selector.AttributeRouteModel.Template =
                                AttributeRouteModel.CombineTemplates("{__tenant__}", selector.AttributeRouteModel.Template);
                        }
                    });
                });

            services.AddMultiTenant<TenantInfo>()
                .WithRouteStrategy(ConfigRoutes)
                .WithConfigurationStore()
                .WithPerTenantAuthentication();

            // Required due to a bug in ASP.NET Core Identity (https://github.com/aspnet/Identity/issues/2019)
            services.PostConfigure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme, options =>
            {
                // This will result in a path of /_tenant_/Identity/Account/Login
                options.LoginPath = $"{options.Cookie.Path}{options.LoginPath}";
                options.LogoutPath = $"{options.Cookie.Path}{options.LogoutPath}";
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
            app.UseAuthentication();
            app.UseMvc(ConfigRoutes);
        }

        private static void ConfigRoutes(Microsoft.AspNetCore.Routing.IRouteBuilder routes)
        {
            routes.MapRoute("SharedLogin", "SharedLogin", new { controller = "Home", action = "SharedLogin" });
            routes.MapRoute("Defaut", "{__tenant__=}/{controller=Home}/{action=Index}");
        }
    }
}
