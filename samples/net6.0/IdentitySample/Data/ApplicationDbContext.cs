using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentitySample.Data;

public class ApplicationDbContext : MultiTenantIdentityDbContext
{
    public ApplicationDbContext(IMultiTenantContextAccessor multiTenantContextAccessor, DbContextOptions options) : base(multiTenantContextAccessor, options)
    {
    }

    public ApplicationDbContext(IMultiTenantContextAccessor multiTenantContextAccessor) : base(multiTenantContextAccessor)
    {
    }

    public ApplicationDbContext(ITenantInfo tenantInfo) : base(tenantInfo)
    {
        // used for the design-time factory and progammatic migrations in program.cs
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var tenantInfo = TenantInfo as AppTenantInfo;
        optionsBuilder.UseSqlite(tenantInfo?.ConnectionString ?? throw new InvalidOperationException());
        base.OnConfiguring(optionsBuilder);
    }
}