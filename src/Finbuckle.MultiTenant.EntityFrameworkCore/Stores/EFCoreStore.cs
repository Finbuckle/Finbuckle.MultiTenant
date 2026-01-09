// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Stores;

/// <summary>
/// A multi-tenant store that uses Entity Framework Core for tenant storage.
/// </summary>
/// <typeparam name="TEFCoreStoreDbContext">The <see cref="EFCoreStoreDbContext{TTenantInfo}"/> implementation type.</typeparam>
/// <typeparam name="TTenantInfo">The <see cref="ITenantInfo"/> implementation type.</typeparam>
public class EFCoreStore<TEFCoreStoreDbContext, TTenantInfo> : IMultiTenantStore<TTenantInfo>
    where TEFCoreStoreDbContext : EFCoreStoreDbContext<TTenantInfo>
    where TTenantInfo : class, ITenantInfo
{
    // internal for testing
    internal readonly TEFCoreStoreDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of EFCoreStore.
    /// </summary>
    /// <param name="dbContext">The <see cref="EFCoreStoreDbContext{TTenantInfo}"/> instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dbContext"/> is null.</exception>
    public EFCoreStore(TEFCoreStoreDbContext dbContext)
    {
        this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <inheritdoc />
    public virtual async Task<TTenantInfo?> GetAsync(string id)
    {
        return await dbContext.TenantInfo.AsNoTracking()
            .Where(ti => ti.Id == id)
            .SingleOrDefaultAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<TTenantInfo>> GetAllAsync()
    {
        return await dbContext.TenantInfo.AsNoTracking().ToListAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<TTenantInfo>> GetAllAsync(int take, int skip)
    {
        return await dbContext.TenantInfo.Skip(skip).Take(take).AsNoTracking().ToListAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<TTenantInfo?> GetByIdentifierAsync(string identifier)
    {
        return await dbContext.TenantInfo.AsNoTracking()
            .Where(ti => ti.Identifier == identifier)
            .SingleOrDefaultAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<bool> AddAsync(TTenantInfo tenantInfo)
    {
        await dbContext.TenantInfo.AddAsync(tenantInfo).ConfigureAwait(false);
        var result = await dbContext.SaveChangesAsync().ConfigureAwait(false) > 0;
        dbContext.Entry(tenantInfo).State = EntityState.Detached;

        return result;
    }

    /// <inheritdoc />
    public virtual async Task<bool> RemoveAsync(string identifier)
    {
        var existing = await dbContext.TenantInfo
            .Where(ti => ti.Identifier == identifier)
            .SingleOrDefaultAsync().ConfigureAwait(false);

        if (existing is null)
        {
            return false;
        }

        dbContext.TenantInfo.Remove(existing);
        return await dbContext.SaveChangesAsync().ConfigureAwait(false) > 0;
    }

    /// <inheritdoc />
    public virtual async Task<bool> UpdateAsync(TTenantInfo tenantInfo)
    {
        dbContext.TenantInfo.Update(tenantInfo);
        var result = await dbContext.SaveChangesAsync().ConfigureAwait(false) > 0;
        dbContext.Entry(tenantInfo).State = EntityState.Detached;
        return result;
    }
}