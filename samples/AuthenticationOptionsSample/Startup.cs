
using System.Threading.Tasks;
using Finbuckle.MultiTenant.AspNetCore;
using Finbuckle.MultiTenant.Core;
using Finbuckle.MultiTenant.Core.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

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

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).
                AddCookie(CookieAuthenticationDefaults.AuthenticationScheme).
                AddFacebook("Facebook", options =>
                {
                    options.AppId = "default"; // Required here, but overridden by tenant config.
                    options.AppSecret = "default"; // Required here, but overridden by tenant config.
                    options.Scope.Add("email");
                    options.Fields.Add("name");
                    options.Fields.Add("email");
                }).AddOpenIdConnect("OpenIdConnect", options =>
                {
                    options.ClientId = "default"; // Required here, but overridden by tenant config.
                    options.Authority = "https://default"; // Required here, but overridden by tenant config.
                    options.RequireHttpsMetadata = false; // For testing only.
                });

            services.AddMultiTenant().
                WithInMemoryStore(Configuration.GetSection("Finbuckle:MultiTenant:InMemoryMultiTenantStore")).
                WithRouteStrategy().
                WithRemoteAuthentication(). // Important!
                WithPerTenantOptions<AuthenticationOptions>((options, tenantContext) =>
                {
                    // Allow each tenant to have a different default challenge scheme.
                    if (tenantContext.Items.TryGetValue("ChallengeScheme", out object challengeScheme))
                    {
                        options.DefaultChallengeScheme = (string)challengeScheme;
                    }
                }).
                WithPerTenantOptions<CookieAuthenticationOptions>((options, tenantContext) =>
                {
                    // Set a unique cookie name for this tenant.
                    options.Cookie.Name = tenantContext.Id + "-cookie";

                    // Note the paths set take our routing strategy into account.
                    options.LoginPath = "/" + tenantContext.Identifier + "/Home/Login";
                    options.Cookie.Path = "/" + tenantContext.Identifier;
                }).
                WithPerTenantOptions<OpenIdConnectOptions>((options, tenantContext) =>
                {
                    // Set the OpenIdConnect options if the tenant specifies it.
                    if (tenantContext.Items.TryGetValue("ChallengeScheme", out object challengeScheme))
                    {
                        // You must configure register with the OpenId Connect server and configure
                        // the tenant accordingly!
                        if ((string)challengeScheme == "OpenIdConnect")
                        {
                            options.ClientId = (string)tenantContext.Items["ClientId"];
                            options.Authority = (string)tenantContext.Items["Authority"];
                        }
                    }
                }).
                WithPerTenantOptions<FacebookOptions>((options, tenantContext) =>
                {
                    // Set the Facebook options if the tenant specifies it.
                    if (tenantContext.Items.TryGetValue("ChallengeScheme", out object challengeScheme))
                    {
                        // You must configure register with a Facebook app and configure
                        // the tenant accordingly!
                        if ((string)challengeScheme == "Facebook")
                        {
                            options.AppId = (string)tenantContext.Items["FacebookAppId"];
                            options.AppSecret = (string)tenantContext.Items["FacebookAppSecret"];
                        }
                    }
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