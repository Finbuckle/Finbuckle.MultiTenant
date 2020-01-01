using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Finbuckle.MultiTenant;
using IdentityDataIsolationSample.Data;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IdentityDataIsolationSample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();

            // Set up the databases for the sample if needed.
            var env = host.Services.GetService<IHostingEnvironment>();
            if (env.EnvironmentName == "Development")
            {
                using (var db = new ApplicationDbContext(new TenantInfo(null, null, null, "Data Source=Data/SharedIdentity.db", null)))
                {
                    db.Database.MigrateAsync().Wait();
                }

                using (var db = new ApplicationDbContext(new TenantInfo(null, null, null, "Data Source=Data/InitechIdentity.db", null)))
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
