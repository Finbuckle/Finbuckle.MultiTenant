using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DataIsolationBlazorSample.Data;
using Finbuckle.MultiTenant;
using DataIsolationBlazorSample.Models;
using Microsoft.AspNetCore.Routing;
using DataIsolationBlazorSample.Classes;

namespace DataIsolationBlazorSample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public ITenantStrategy tenantSettings { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            if (tenantSettings == null)
                tenantSettings = new TenantStrategy();

            Configuration.GetSection("TenantStrategy").Bind(tenantSettings);
            services.AddSingleton<WeatherForecastService>();

            if (tenantSettings.RouteStrategy)
            {
                services.AddMultiTenant<TenantInfo>()
                .WithConfigurationStore()
                .WithRouteStrategy();
            }
            else
            {
                services.AddMultiTenant<TenantInfo>()
                    .WithConfigurationStore()
                    .WithHostStrategy(tenantSettings.HostTemplate);
            }

            //Allows accessing HttpContext in Blazor
            services.AddHttpContextAccessor();

            services.AddSingleton<ITenantStrategy>(Configuration.GetSection("TenantStrategy").Get<TenantStrategy>());
            services.AddTransient<IConfigHelper, ConfigHelper>();
            services.AddScoped<Classes.ContextHelper>();
            services.AddDbContext<ToDoDbContext>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.EnvironmentName == "Development")
            {
                app.UseDeveloperExceptionPage();
            }
            //--!
            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseRouting();
            app.UseMultiTenant<TenantInfo>();

            if (tenantSettings.RouteStrategy)
            {
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapBlazorHub();
                    endpoints.MapControllerRoute("default", tenantSettings.DefaultRoute);
                    endpoints.MapFallbackToPage("/_Host");
                });
            }
            else
            {
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapBlazorHub();
                    endpoints.MapControllers();
                    endpoints.MapFallbackToPage("/_Host");
                });
            }

            SetupDb();
        }
        private void SetupDb()
        {
            var ti = new TenantInfo { Id = "finbuckle", ConnectionString = "Data Source=Data/ToDoList.db" };
            using (var db = new ToDoDbContext(ti))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
                db.ToDoItems.Add(new ToDoItem { Title = "Call Lawyer ", Completed = false });
                db.ToDoItems.Add(new ToDoItem { Title = "File Papers", Completed = false });
                db.ToDoItems.Add(new ToDoItem { Title = "Send Invoices", Completed = true });
                db.SaveChanges();
            }

            ti = new TenantInfo { Id = "megacorp", ConnectionString = "Data Source=Data/ToDoList.db" };
            using (var db = new ToDoDbContext(ti))
            {
                db.Database.EnsureCreated();
                db.ToDoItems.Add(new ToDoItem { Title = "Send Invoices", Completed = true });
                db.ToDoItems.Add(new ToDoItem { Title = "Construct Additional Pylons", Completed = true });
                db.ToDoItems.Add(new ToDoItem { Title = "Call Insurance Company", Completed = false });
                db.SaveChanges();
            }

            ti = new TenantInfo { Id = "initech", ConnectionString = "Data Source=Data/Initech_ToDoList.db" };
            using (var db = new ToDoDbContext(ti))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
                db.ToDoItems.Add(new ToDoItem { Title = "Send Invoices", Completed = false });
                db.ToDoItems.Add(new ToDoItem { Title = "Pay Salaries", Completed = true });
                db.ToDoItems.Add(new ToDoItem { Title = "Write Memo", Completed = false });
                db.SaveChanges();
            }
            ti = new TenantInfo { Id = "localhost", ConnectionString = "Data Source=Data/Local_ToDoList.db" };
            using (var db = new ToDoDbContext(ti))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
                db.ToDoItems.Add(new ToDoItem { Title = "Send Invoices Locally", Completed = true });
                db.ToDoItems.Add(new ToDoItem { Title = "Do not Pay Salaries", Completed = true });
                db.ToDoItems.Add(new ToDoItem { Title = "Do not Write any Memos", Completed = true });
                db.ToDoItems.Add(new ToDoItem { Title = "Forget any Memos", Completed = true });

                db.SaveChanges();
            }
        }

        private void ConfigRoutes(IRouteBuilder routes)
        {
#if routestrategy
           routes.MapRoute("Defaut", "{__tenant__=}/{controller=Home}/{action=Index}");
#endif
        }
    }
}
