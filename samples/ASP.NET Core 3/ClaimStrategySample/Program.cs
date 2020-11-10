using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ClaimStrategySample.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ClaimStrategySample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

                if (userManager.FindByNameAsync("alice@initech.com").Result == null)
                {
                    var alice = new IdentityUser{ UserName = "alice@initech.com", Email = "alice@initech.com", EmailConfirmed = true};
                    var result = userManager.CreateAsync(alice).Result;
                    result = userManager.AddPasswordAsync(alice, "Pass123$").Result;
                    result = userManager.AddClaimAsync(alice, new Claim("__tenant__", "initech")).Result;
                }

                if (userManager.FindByNameAsync("bob@megacorp.com").Result == null)
                {
                    var bob = new IdentityUser{ UserName = "bob@megacorp.com", Email = "bob@megacorp.com", EmailConfirmed = true};
                    var result = userManager.CreateAsync(bob).Result;
                    result = userManager.AddPasswordAsync(bob, "Pass123$").Result;
                    result = userManager.AddClaimAsync(bob, new Claim("__tenant__", "megacorp")).Result;
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
