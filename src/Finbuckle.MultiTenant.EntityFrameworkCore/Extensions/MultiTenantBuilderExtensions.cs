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

using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Stores;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provides builder methods for Finbuckle.MultiTenant services and configuration.
    /// </summary>
    public static class FinbuckleMultiTenantBuilderExtensions
    {
        /// <summary>
        /// Adds an EFCore based multitenant store to the application. Will also add the database context service unless it is already added.
        /// </summary>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithEFCoreStore<TEFCoreStoreDbContext, TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder)
            where TEFCoreStoreDbContext : EFCoreStoreDbContext<TTenantInfo>
            where TTenantInfo : class, ITenantInfo, new()
        {
            builder.Services.AddDbContext<TEFCoreStoreDbContext>(); // Note, will not override existing context if already added.
            return builder.WithStore<EFCoreStore<TEFCoreStoreDbContext, TTenantInfo>>(ServiceLifetime.Scoped);
        }
    }
}