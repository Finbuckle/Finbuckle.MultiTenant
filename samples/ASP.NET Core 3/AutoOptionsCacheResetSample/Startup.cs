using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Finbuckle.MultiTenant.AspNetCore.OptionsCacheReset;
using Microsoft.OpenApi.Models;

namespace AutoOptionsCacheResetSample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.Configure<TestOption>(Configuration.GetSection(
                nameof(TestOption)));
            
            services.AddMultiTenant<SampleTenantInfo>()
                .WithInMemoryStore()
                .WithStaticStrategy("finbuckle")
                .WithPerTenantManagedCacheOptions<SampleTenantInfo, TestOption>((options, info) =>
                {
                    options.TenantVersion = info.Version;
                });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "AutoOptionsCacheResetSample", Version = "v1"});
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.EnvironmentName == "Development")
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "AutoOptionsCacheResetSample v1"));
            }

            app.UseStaticFiles();
            app.UseRouting();

            // add SampleTenantInfo to IMultiTenantStore if not exist
            app.Use(async (context, next) =>
            {
                var multiTenantStore =
                    context.RequestServices.GetRequiredService<IMultiTenantStore<SampleTenantInfo>>();
                var tenantInfo = await multiTenantStore.TryGetByIdentifierAsync("finbuckle");
                if (tenantInfo == null)
                {
                    await multiTenantStore.TryAddAsync(new SampleTenantInfo()
                    {
                        Id = "1",
                        Identifier = "finbuckle",
                        Name = "finbuckle",
                        Version = 1,

                    });
                }
                await next.Invoke();
            });
            
            app.UseMultiTenant();
            app.UseMultiTenantOptionsResetManager<SampleTenantInfo>();
            
            app.UseAuthentication();
            app.UseAuthorization();
            
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}