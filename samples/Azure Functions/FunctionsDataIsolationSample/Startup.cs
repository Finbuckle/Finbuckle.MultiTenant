using System;
using System.Collections.Generic;
using System.Text;

using Finbuckle.MultiTenant;

using FunctionsDataIsolationSample.Data;
using FunctionsDataIsolationSample.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(FunctionsDataIsolationSample.Startup))]
namespace FunctionsDataIsolationSample
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddMultiTenant<TenantInfo>()
#if Release
                //.WithConfigurationStore()
#else
                .WithInMemoryStore(config =>
                {
                    config.Tenants.Add(new TenantInfo()
                    {
                        Id = "megacorp",
                        Identifier = "megacorp",
                        Name = "MegaCorp",
                        ConnectionString = "Data Source=Data/ToDoList.db"
                    });
                    config.Tenants.Add(new TenantInfo()
                    {
                        Id = "finbuckle",
                        Identifier = "finbuckle",
                        Name = "Finbuckle",
                        ConnectionString = "Data Source=Data/ToDoList.db"
                    });
                    config.Tenants.Add(new TenantInfo()
                    {
                        Id = "initech",
                        Identifier = "initech",
                        Name = "Initech LLC",
                        ConnectionString = "Data Source=Data/Initech_ToDoList.db"
                    });
                })
#endif
                .WithBasePathStrategy(routePrefix: PathString.FromUriComponent("/api"));

            builder.Services.AddDbContext<ToDoDbContext>();

            builder.UseMultiTenant();

            // Important note: When building a functions app using Sqlite the native DLL's aren't used (doesn't use the DLL's in the runtime folder).
            // You'll have to manually copy the DLL to the appropriate location.
            // Not recommended for production.
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
        }
    }
}
