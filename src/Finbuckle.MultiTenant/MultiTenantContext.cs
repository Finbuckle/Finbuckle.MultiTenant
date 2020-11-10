//    Copyright 2018-2020 Finbuckle LLC, Andrew White, and Contributors
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

namespace Finbuckle.MultiTenant
{
    /// <summary>
    /// Contains information for the multitenant tenant, store, and strategy.
    /// </summary>
    public class MultiTenantContext<T> : IMultiTenantContext<T>
        where T : class, ITenantInfo, new()
    {
        public T TenantInfo { get; set; }
        public StrategyInfo StrategyInfo { get; set; }
        public StoreInfo<T> StoreInfo { get; set; }
    }
}