using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;

namespace IdentitySample.Data;

public class ApplicationDbContext : MultiTenantIdentityDbContext
{
    public ApplicationDbContext(ITenantInfo tenantInfo) : base(tenantInfo)
    {
    }

    public ApplicationDbContext(ITenantInfo tenantInfo, DbContextOptions options) : base(tenantInfo, options)
    {
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(TenantInfo.ConnectionString ?? throw new InvalidOperationException());
        base.OnConfiguring(optionsBuilder);
    }
}