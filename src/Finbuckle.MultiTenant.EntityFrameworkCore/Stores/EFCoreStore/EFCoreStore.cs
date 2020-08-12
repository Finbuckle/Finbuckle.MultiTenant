//    Copyright 2018-2020 Andrew White
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Finbuckle.MultiTenant.Stores
{
    public class EFCoreStore<TEFCoreStoreDbContext, TTenantInfo> : IMultiTenantStore<TTenantInfo>
        where TEFCoreStoreDbContext : EFCoreStoreDbContext<TTenantInfo>
        where TTenantInfo :class, ITenantInfo, new()
    {
        internal readonly TEFCoreStoreDbContext dbContext;

        public EFCoreStore(TEFCoreStoreDbContext dbContext)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<TTenantInfo> TryGetAsync(string id)
        {
            return await dbContext.TenantInfo
                            .Where(ti => ti.Id == id)
                            .SingleOrDefaultAsync();
        }

        public async Task<IEnumerable<TTenantInfo>> GetAllAsync()
        {
            return await dbContext.TenantInfo.ToListAsync();
        }

        public async Task<TTenantInfo> TryGetByIdentifierAsync(string identifier)
        {
            return await dbContext.TenantInfo
                            .Where(ti => ti.Identifier == identifier)
                            .SingleOrDefaultAsync();
        }

        public async Task<bool> TryAddAsync(TTenantInfo tenantInfo)
        {
            await dbContext.TenantInfo.AddAsync(tenantInfo);

            return await dbContext.SaveChangesAsync() > 0;
        }

        public async Task<bool> TryRemoveAsync(string identifier)
        {
            var existing = await dbContext.TenantInfo
                .Where(ti => ti.Identifier == identifier)
                .SingleOrDefaultAsync();
            dbContext.TenantInfo.Remove(existing);
            return await dbContext.SaveChangesAsync() > 0;
        }

        public async Task<bool> TryUpdateAsync(TTenantInfo tenantInfo)
        {
            var existingLocal = dbContext.TenantInfo.Local.Where(ti => ti.Id == tenantInfo.Id).SingleOrDefault();
            if(existingLocal != null)
            {
                dbContext.Entry(existingLocal).State = EntityState.Detached;
            }

            dbContext.TenantInfo.Update(tenantInfo);
            return await dbContext.SaveChangesAsync() > 0;
        }
    }
}