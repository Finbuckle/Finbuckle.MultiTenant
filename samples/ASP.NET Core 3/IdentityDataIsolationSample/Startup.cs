﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using IdentityDataIsolationSample.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Routing;
using Finbuckle.Utilities.AspNetCore;
using System.Collections.Generic;

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

            services.AddDefaultIdentity<IdentityUser>()
                    .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.AddRazorPages(options =>
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
            
            services.DecorateService<LinkGenerator, AmbientValueLinkGenerator>(new List<string> { "__tenant__" });

            services.AddMultiTenant<TenantInfo>()
                    .WithRouteStrategy()
                    .WithConfigurationStore()
                    .WithPerTenantOptions<CookieAuthenticationOptions>((options, tenantInfo) =>
                    {
                        // Since we are using the route strategy configure each tenant
                        // to have a different cookie name and adjust the paths.
                        options.Cookie.Path = $"/{tenantInfo.Identifier}";
                        options.Cookie.Name = $"{tenantInfo.Id}_authentication";
                        options.LoginPath = $"{options.Cookie.Path}{options.LoginPath}";
                        options.LogoutPath = $"{options.Cookie.Path}{options.LogoutPath}";
                    });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.EnvironmentName == "Development")
            {
                app.UseDeveloperExceptionPage();
                app.UseStatusCodePages();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseMultiTenant<TenantInfo>();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{__tenant__=}/{controller=Home}/{action=Index}");
                endpoints.MapRazorPages();
            });
        }
    }
}
