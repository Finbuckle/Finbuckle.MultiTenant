using Finbuckle.MultiTenant.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentitySample.Data;

public class ApplicationDbContext : MultiTenantIdentityDbContext
{
    private readonly AppTenantInfo _tenantInfo;

    public ApplicationDbContext(AppTenantInfo tenantInfo) : base(tenantInfo)
    {
        _tenantInfo = tenantInfo;
    }

    public ApplicationDbContext(AppTenantInfo tenantInfo, DbContextOptions options) : base(tenantInfo, options)
    {
        _tenantInfo = tenantInfo;
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(_tenantInfo.ConnectionString ?? throw new InvalidOperationException());
        base.OnConfiguring(optionsBuilder);
    }
}