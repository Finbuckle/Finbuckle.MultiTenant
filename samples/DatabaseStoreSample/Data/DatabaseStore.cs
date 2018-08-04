//    Copyright 2018 Andrew White
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
using System.Linq;
using System.Threading.Tasks;
using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;

namespace DatabaseStoreSample.Data
{
    /// <summary>
    /// This is a crude multitenant store using a SQLite database.
    /// This is not tested or suitable for real world use!
    /// </summary>
    public class DatabaseStore : IMultiTenantStore
    {
        private readonly MultiTenantStoreDbContext dbContext;

        public DatabaseStore(MultiTenantStoreDbContext dbContext)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<TenantInfo> GetByIdentifierAsync(string identifier)
        {
            return await dbContext.TenantInfo
                .Where(ti => ti.Identifier == identifier)
                .SingleOrDefaultAsync();
        }

        public async Task<bool> TryAddAsync(TenantInfo tenantInfo)
        {
            int result = 0;
            dbContext.TenantInfo.Add(tenantInfo);

            try
            {
                result = await dbContext.SaveChangesAsync();
            }
            catch
            {                
                // Do something that makes sense here...
            }

            return result > 0;
        }

        public async Task<bool> TryRemoveAsync(string identifier)
        {
            int result = 0;
            var existing = await GetByIdentifierAsync(identifier);

            if (existing != null)
            {
                try
                {
                    dbContext.TenantInfo.Remove(existing);
                    result = await dbContext.SaveChangesAsync();
                }
                catch
                {                    
                    // Do something that makes sense here...
                }
            }

            return result > 0;
        }

        public async Task<bool> TryUpdateAsync(TenantInfo tenantInfo)
        {
            int result = 0;
            dbContext.Entry(tenantInfo).State = EntityState.Modified;

            try
            {
                result = await dbContext.SaveChangesAsync();
            }
            catch
            {                
                // Do something that makes sense here...
            }

            return result > 0;
        }
    }
}