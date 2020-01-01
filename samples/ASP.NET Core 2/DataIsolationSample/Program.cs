using DataIsolationSample.Data;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DataIsolationSample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();

            var env = host.Services.GetService<IHostingEnvironment>();
            if (env.EnvironmentName == "Development")
            {
                using (var db = new ToDoDbContext(new TenantInfo(null, null, null, "Data Source=Data/ToDoList.db", null)))
                {
                    db.Database.MigrateAsync().Wait();
                }

                using (var db = new ToDoDbContext(new TenantInfo(null, null, null, "Data Source=Data/Initech_ToDoList.db", null)))
                {
                    db.Database.MigrateAsync().Wait();
                }
            }

            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}