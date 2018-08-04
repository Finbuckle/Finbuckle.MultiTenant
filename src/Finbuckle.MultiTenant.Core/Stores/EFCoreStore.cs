// //    Copyright 2018 Andrew White
// // 
// //    Licensed under the Apache License, Version 2.0 (the "License");
// //    you may not use this file except in compliance with the License.
// //    You may obtain a copy of the License at
// // 
// //        http://www.apache.org/licenses/LICENSE-2.0
// // 
// //    Unless required by applicable law or agreed to in writing, software
// //    distributed under the License is distributed on an "AS IS" BASIS,
// //    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// //    See the License for the specific language governing permissions and
// //    limitations under the License.

// using System;
// using System.Collections.Concurrent;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Finbuckle.MultiTenant.Core;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Logging;

// namespace Finbuckle.MultiTenant.Stores
// {
//     public interface ITenantInfoEntity
//     {
//         string Id { get; }
//         string Identifier { get; }
//         string Name { get; }
//         string ConnectionString { get; }
//         IDictionary<string, object> Items { get; }
//     }

//     /// <summary>
//     /// A multitenant Store that uses EFCore.
//     /// </summary>
//     public class EFCoreStore<TDbContext, TTenantInfoEntity> : IMultiTenantStore
//         where TDbContext : DbContext where TTenantInfoEntity : ITenantInfoEntity
//     {
//         private readonly ILogger<EFCoreStore<TDbContext, TTenantInfoEntity>> logger;
//         private readonly TDbContext dbContext;
        
//         public EFCoreStore(TDbContext dbContext, ILogger<EFCoreStore<TDbContext, TTenantInfoEntity>> logger)
//         {
//             this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
//             this.logger = logger;
//         }

//         public async Task<TenantInfo> GetByIdentifierAsync(string identifier)
//         {
//             if (identifier == null)
//             {
//                 throw new ArgumentNullException(nameof(identifier));
//             }

//             try
//             {

//             }
            
//             Utilities.TryLogInfo(logger, $"Tenant Id \"{result?.Id ?? "<null>"}\" found in store for identifier \"{identifier}\".");

//             return await Task.FromResult(result).ConfigureAwait(false);
//         }

//         public Task<bool> TryAddAsync(TenantInfo tenantInfo)
//         {
//             if (tenantInfo == null)
//             {
//                 throw new ArgumentNullException(nameof(tenantInfo));
//             }

//             var result = tenantMap.TryAdd(tenantInfo.Identifier, tenantInfo);

//             if(result)
//             {
//                 Utilities.TryLogInfo(logger, $"Tenant \"{tenantInfo.Identifier}\" added to EFCoreStore.");
//             }
//             else
//             {
//                 Utilities.TryLogInfo(logger, $"Unable to add tenant \"{tenantInfo.Identifier}\" to EFCoreStore.");
//             }

//             return Task.FromResult(result);
//         }

//         public Task<bool> TryRemoveAsync(string identifier)
//         {
//             if (identifier == null)
//             {
//                 throw new ArgumentNullException(nameof(identifier));
//             }

//             var result = tenantMap.TryRemove(identifier, out var dummy);

//             if(result)
//             {
//                 Utilities.TryLogInfo(logger, $"Tenant \"{identifier}\" removed from EFCoreStore.");
//             }
//             else
//             {
//                 Utilities.TryLogInfo(logger, $"Unable to remove tenant \"{identifier}\" from EFCoreStore.");
//             }

//             return Task.FromResult(result);
//         }

//         public Task<bool> TryUpdateAsync(TenantInfo tenantInfo)
//         {
//             if (tenantInfo == null)
//             {
//                 throw new ArgumentNullException(nameof(tenantInfo));
//             }

//             if (tenantInfo.Id == null)
//             {
//                 throw new ArgumentNullException(nameof(tenantInfo.Id));
//             }

//             var existingTenantInfo = tenantMap.Values.Where(ti => ti.Id == tenantInfo.Id).SingleOrDefault();

//             if(existingTenantInfo == null)
//             {
//                 Utilities.TryLogInfo(logger, $"Tenant Id \"{tenantInfo.Id}\" not found in EFCoreStore.");
//                 return false;
//             }

//             existingTenantInfo = tenantInfo;
//             Utilities.TryLogInfo(logger, $"Tenant Id \"{tenantInfo.Id}\" updated in EFCoreStore.");

//             return await Task.FromResult(true);
//         }
//     }
// }