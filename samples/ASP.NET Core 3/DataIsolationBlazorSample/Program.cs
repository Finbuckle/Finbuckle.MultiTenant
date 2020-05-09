using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using DataIsolationBlazorSample.Data;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DataIsolationBlazorSample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            // Set up the databases for the sample if needed.
            var env = host.Services.GetService<IWebHostEnvironment>();
            if (env.EnvironmentName == "Development")
            {
                using (var db = new ToDoDbContext(new TenantInfo { ConnectionString = "Data Source=Data/ToDoList.db" }))
                {
                    db.Database.MigrateAsync().Wait();
                }

                using (var db = new ToDoDbContext(new TenantInfo { ConnectionString = "Data Source=Data/Initech_ToDoList.db" }))
                {
                    db.Database.MigrateAsync().Wait();
                }
                using (var db = new ToDoDbContext(new TenantInfo { ConnectionString = "Data Source=Data/Local_ToDoList.db" }))
                {
                    db.Database.MigrateAsync().Wait();
                }
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
