// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System.Security.Claims;
using ClaimStrategySample.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
                    userManager.CreateAsync(alice).Wait();
                    userManager.AddPasswordAsync(alice, "Pass123$").Wait();
                    userManager.AddClaimAsync(alice, new Claim("__tenant__", "initech")).Wait();
                }

                if (userManager.FindByNameAsync("bob@megacorp.com").Result == null)
                {
                    var bob = new IdentityUser{ UserName = "bob@megacorp.com", Email = "bob@megacorp.com", EmailConfirmed = true};
                    userManager.CreateAsync(bob).Wait();
                    userManager.AddPasswordAsync(bob, "Pass123$").Wait();
                    userManager.AddClaimAsync(bob, new Claim("__tenant__", "megacorp")).Wait();
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
