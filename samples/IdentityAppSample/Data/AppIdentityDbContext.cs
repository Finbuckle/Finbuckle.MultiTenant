using Finbuckle.MultiTenant.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentitySampleApp.Data;

public class AppIdentityDbContext(
    DbContextOptions<AppIdentityDbContext> options) : MultiTenantIdentityDbContext<string>(options)
{
}
