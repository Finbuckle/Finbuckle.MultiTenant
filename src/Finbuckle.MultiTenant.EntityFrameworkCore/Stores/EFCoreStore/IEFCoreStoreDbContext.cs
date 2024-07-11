using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Stores.EFCoreStore;

public interface IEFCoreStoreDbContext<TTenantInfo> where TTenantInfo : class, ITenantInfo
{
    public DbSet<TTenantInfo> TenantInfos { get; }
}