using Finbuckle.MultiTenant.EntityFrameworkCore;
using Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;

namespace IdentitySampleApp.Data;

public class AppIdentityDbContextFactory : IDbContextFactory<AppIdentityDbContext>
{
    public AppIdentityDbContext CreateDbContext()
    {
        return MultiTenantDbContextExtensions.Create<AppIdentityDbContext, AppTenantInfo, string>(new AppTenantInfo
        {
            Id = "dummy",
            Identifier = "dummy",
            Name = "dummy"
        });
    }
}