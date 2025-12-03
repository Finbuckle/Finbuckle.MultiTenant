using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentitySampleApp.Data;

public class AppIdentityDbContext(
    IMultiTenantContextAccessor<AppTenantInfo> mtca,
    DbContextOptions<AppIdentityDbContext> options) : MultiTenantIdentityDbContext(mtca, options)
{
}