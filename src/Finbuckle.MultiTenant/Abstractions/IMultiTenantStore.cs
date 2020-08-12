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

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Finbuckle.MultiTenant
{
    /// <summary>
    /// Interface definition for tenant stores.
    /// </summary>
    public interface IMultiTenantStore<TTenantInfo> where TTenantInfo : class, ITenantInfo, new()
    {
        /// <summary>
        /// Try to add the TTenantInfo to the store.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task<bool> TryAddAsync(TTenantInfo tenantInfo);

        /// <summary>
        /// Try to update the TTenantInfo in the store.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task<bool> TryUpdateAsync(TTenantInfo tenantInfo);

        /// <summary>
        /// Try to remove the TTenantInfo from the store.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> TryRemoveAsync(string id);

        /// <summary>
        /// Retrieve the TTenantInfo for a given identifier.
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        Task<TTenantInfo> TryGetByIdentifierAsync(string identifier);

        /// <summary>
        /// Retrieve the TTenantInfo for a given tenant Id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<TTenantInfo> TryGetAsync(string id);


        /// <summary>
        /// Retrieve all the TTenantInfo's from the store.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<TTenantInfo>> GetAllAsync();
    }
}