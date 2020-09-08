using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Finbuckle.MultiTenant;
using Microsoft.Extensions.DependencyInjection;
using SharedLoginSample.Data;
using Microsoft.EntityFrameworkCore;

namespace SharedLoginSample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();

            // Create and migrate the database if necessary.
            var env = host.Services.GetService<IHostingEnvironment>();
            if (env.EnvironmentName == "Development")
            {
                using (var db = new ApplicationDbContext(new TenantInfo { ConnectionString = "Data Source=Data/SharedIdentity.db" }))
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
