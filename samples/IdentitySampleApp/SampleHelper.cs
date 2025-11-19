using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using IdentitySampleApp.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace IdentitySampleApp;

public abstract class SampleHelper
{
    private static readonly Dictionary<string, List<IdentityUser>> _tenantUsers = new()
    {
        ["initech"] =
        [
            new IdentityUser { UserName = "alice@initech.com", Email = "alice@initech.com" },
            new IdentityUser { UserName = "bob@initech.com", Email = "bob@initech.com" }
        ],
        ["acme"] =
        [
            new IdentityUser { UserName = "harry@acme.com", Email = "harry@acme.com" },
            new IdentityUser { UserName = "larry@acme.com", Email = "larry@acme.com" },
            new IdentityUser { UserName = "moe@acme.com", Email = "moe@acme.com" }
        ]
    };

    /// <summary>
    /// Set up the default tenant users for the sample.
    /// </summary>
    public static void SeedIdentity(WebApplication app)
    {
        var stores = app.Services.GetRequiredService<IMultiTenantStore<AppTenantInfo>>();
        var mtcs = app.Services.GetRequiredService<IMultiTenantContextSetter>();

        foreach (var tenant in stores.GetAllAsync().Result)
        {
            using var scope = app.Services.CreateScope();
            using var db = MultiTenantDbContext.Create<AppIdentityDbContext, AppTenantInfo>(tenant, scope.ServiceProvider);
            db.Database.EnsureCreated();
            var userStore = new UserStore<IdentityUser>(db);
            
            using var userManager = ActivatorUtilities.CreateInstance<UserManager<IdentityUser>>(scope.ServiceProvider, userStore);
            
            foreach(var user in _tenantUsers[tenant.Identifier])
            {
                var existingUser = userManager.FindByEmailAsync(user.Email!).Result;
                if (existingUser == null)
                {
                    user.EmailConfirmed = true;
                    var result = userManager.CreateAsync(user, "P@ssword1").Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception($"Failed to create user {user.Email} for tenant {tenant.Identifier}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }
        }
    }
}