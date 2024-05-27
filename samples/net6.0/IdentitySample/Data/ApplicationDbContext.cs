using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentitySample.Data;

public class ApplicationDbContext : MultiTenantIdentityDbContext
{
    public DbSet<Client> Clients => Set<Client>();
    
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<Client>().IsMultiTenant().AdjustUniqueIndexes().AdjustIndexes();
        AdjustKeys(builder, modelBuilder);
        base.OnModelCreating(modelBuilder);
    }
    
    private static MultiTenantEntityTypeBuilder AdjustKeys(MultiTenantEntityTypeBuilder builder, ModelBuilder modelBuilder)
    {
        var keys = builder.Builder.Metadata.GetKeys();
        foreach (var key in keys.ToArray())
        {
            builder.AdjustKey(key, modelBuilder);
        }

        return builder;
    }
}