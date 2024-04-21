// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Stores.EFCoreStore;

public class EFCoreStore<TEFCoreStoreDbContext, TTenantInfo> : IMultiTenantStore<TTenantInfo>
    where TEFCoreStoreDbContext : EFCoreStoreDbContext<TTenantInfo>
    where TTenantInfo : class, ITenantInfo, new()
{
    internal readonly TEFCoreStoreDbContext dbContext;

    public EFCoreStore(TEFCoreStoreDbContext dbContext)
    {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

    public virtual async Task<TTenantInfo?> TryGetAsync(string id)
    {
            return await dbContext.TenantInfo.AsNoTracking()
                .Where(ti => ti.Id == id)
                .SingleOrDefaultAsync();
        }

    public virtual async Task<IEnumerable<TTenantInfo>> GetAllAsync()
    {
            return await dbContext.TenantInfo.AsNoTracking().ToListAsync();
        }

    public virtual async Task<TTenantInfo?> TryGetByIdentifierAsync(string identifier)
    {
            return await dbContext.TenantInfo.AsNoTracking()
                .Where(ti => ti.Identifier == identifier)
                .SingleOrDefaultAsync();
        }

    public virtual async Task<bool> TryAddAsync(TTenantInfo tenantInfo)
    {
            await dbContext.TenantInfo.AddAsync(tenantInfo);
            var result = await dbContext.SaveChangesAsync() > 0;
            dbContext.Entry(tenantInfo).State = EntityState.Detached;
            
            return result;
        }

    public virtual async Task<bool> TryRemoveAsync(string identifier)
    {
            var existing = await dbContext.TenantInfo
                .Where(ti => ti.Identifier == identifier)
                .SingleOrDefaultAsync();

            if (existing is null)
            {
                return false;
            }

            dbContext.TenantInfo.Remove(existing);
            return await dbContext.SaveChangesAsync() > 0;
        }

    public virtual async Task<bool> TryUpdateAsync(TTenantInfo tenantInfo)
    {
            dbContext.TenantInfo.Update(tenantInfo);
            var result = await dbContext.SaveChangesAsync() > 0;
            dbContext.Entry(tenantInfo).State = EntityState.Detached;
            return result;
        }
}