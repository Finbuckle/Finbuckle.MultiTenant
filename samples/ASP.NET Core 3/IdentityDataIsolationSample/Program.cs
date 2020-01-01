using System.Linq;
using System.Threading.Tasks;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Options;
using IdentityDataIsolationSample.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace IdentityDataIsolationSample
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            // Set up the databases for the sample if needed.
            var env = host.Services.GetService<IWebHostEnvironment>();
            if (env.EnvironmentName == "Development")
            {
                using (var db = new ApplicationDbContext(new TenantInfo(null, null, null, "Data Source=Data/SharedIdentity.db", null)))
                {
                    await db.Database.MigrateAsync();
                }

                using (var db = new ApplicationDbContext(new TenantInfo(null, null, null, "Data Source=Data/InitechIdentity.db", null)))
                {
                    await db.Database.MigrateAsync();
                }
            }

            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
