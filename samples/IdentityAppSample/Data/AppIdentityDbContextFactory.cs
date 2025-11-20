using Finbuckle.MultiTenant.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityAppSample.Data;

public class AppIdentityDbContextFactory : IDbContextFactory<AppIdentityDbContext>
{
    public AppIdentityDbContext CreateDbContext()
    {
        return MultiTenantDbContext.Create<AppIdentityDbContext, AppTenantInfo>(new AppTenantInfo("dummy", "dummy", "dummy"));
    }
}