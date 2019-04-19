using System.Threading.Tasks;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace DelegateStrategySample
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
            services.AddMultiTenant().
                WithInMemoryStore(Configuration.GetSection("Finbuckle:MultiTenant:InMemoryStore")).
                WithDelegateStrategy(context =>
                {
                    ((HttpContext)context).Request.Query.TryGetValue("tenant", out StringValues tenantId);
                    return Task.FromResult(tenantId.ToString());
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
            app.UseMvcWithDefaultRoute();
        }
    }
}