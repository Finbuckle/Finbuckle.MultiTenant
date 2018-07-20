using System;
using DataIsolationSample.Data;
using DataIsolationSample.Models;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataIsolationSample
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
                WithRouteStrategy(ConfigRoutes);

            // Register the db context, but do not specify a provider/connection string since
            // these vary by tenant.
            services.AddDbContext<ToDoDbContext>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }

            app.UseStaticFiles();
            app.UseMultiTenant();
            app.UseMvc(ConfigRoutes);

            SetupDb();
        }

        private void SetupDb()
        {
            var tc = new TenantContext("finbuckle", null, null, "Data Source=Data/ToDoList.db", null, null);
            using (var db = new ToDoDbContext(tc))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
                db.ToDoItems.Add(new ToDoItem { Title = "Call Lawyer ", Completed = false });
                db.ToDoItems.Add(new ToDoItem { Title = "File Papers", Completed = false });
                db.ToDoItems.Add(new ToDoItem { Title = "Send Invoices", Completed = true });
                db.SaveChanges();
            }

            tc = new TenantContext("megacorp", null, null, "Data Source=Data/ToDoList.db", null, null);
            using (var db = new ToDoDbContext(tc))
            {
                db.Database.EnsureCreated();
                db.ToDoItems.Add(new ToDoItem { Title = "Send Invoices", Completed = true });
                db.ToDoItems.Add(new ToDoItem { Title = "Construct Additional Pylons", Completed = true });
                db.ToDoItems.Add(new ToDoItem { Title = "Call Insurance Company", Completed = false });
                db.SaveChanges();
            }

            tc = new TenantContext("initech", null, null, "Data Source=Data/Initech_ToDoList.db", null, null);
            using (var db = new ToDoDbContext(tc))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
                db.ToDoItems.Add(new ToDoItem { Title = "Send Invoices", Completed = false });
                db.ToDoItems.Add(new ToDoItem { Title = "Pay Salaries", Completed = true });
                db.ToDoItems.Add(new ToDoItem { Title = "Write Memo", Completed = false });
                db.SaveChanges();
            }
        }

        private void ConfigRoutes(IRouteBuilder routes)
        {
            routes.MapRoute("Defaut", "{__tenant__=}/{controller=Home}/{action=Index}");
        }
    }
}