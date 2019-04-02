using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using IdentityDataIsolationSample.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Identity.UI;

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

            services.AddIdentity<MultiTenantIdentityUser, MultiTenantIdentityRole>()
                    .AddDefaultTokenProviders()
                    .AddDefaultUI(UIFramework.Bootstrap4)
                    .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddAuthentication()
                .AddGoogle("Google", options =>
                {
                    // These configuration settings should be set via user-secrets or environment variables!
                    options.ClientId = Configuration.GetValue<string>("GoogleClientId");
                    options.ClientSecret = Configuration.GetValue<string>("GoogleClientSecret");
                    options.AuthorizationEndpoint = string.Concat(options.AuthorizationEndpoint, "?prompt=consent");
                });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddMvcOptions(options => options.EnableEndpointRouting = false)
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

            services.AddMultiTenant()
                .WithRouteStrategy(ConfigRoutes)
                .WithInMemoryStore(Configuration.GetSection("Finbuckle:MultiTenant:InMemoryStore"))
                .WithRemoteAuthentication()
                .WithPerTenantOptions<CookieAuthenticationOptions>((options, tenantInfo) =>
                {
                    // Since we are using the route strategy configure each tenant
                    // to have a different cookie name and adjust the paths.
                    options.Cookie.Name = $"{tenantInfo.Id}_{options.Cookie.Name}";
                    // See below for why this is commented out.
                    //options.LoginPath = $"/{tenantInfo.Identifier}/Home/Login";
                    //options.LogoutPath = $"/{tenantInfo.Identifier}";
                    options.Cookie.Path = $"/{tenantInfo.Identifier}";
                });

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
            routes.MapRoute("Defaut", "{__tenant__=}/{controller=Home}/{action=Index}");
        }
    }
}
