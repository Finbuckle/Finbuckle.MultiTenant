
using Finbuckle.MultiTenant;

using FunctionsDataIsolationSample.Data;
using FunctionsDataIsolationSample.Models;

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
                .WithConfigurationStore()
                .WithRouteStrategy();

            // Register the db context, but do not specify a provider/connection string since
            // these vary by tenant.
            builder.Services.AddDbContext<ToDoDbContext>();

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
