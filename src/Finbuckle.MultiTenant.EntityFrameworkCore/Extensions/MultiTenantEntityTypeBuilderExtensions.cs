// Copyright 2018-2020 Finbuckle LLC, Andrew White, and Contributors
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finbuckle.MultiTenant.EntityFrameworkCore
{
    public static class MultiTenantEntityTypeBuilderExtensions
    {
        /// <summary>
        /// Adds TenantId to all unique indexes.
        /// </summary>
        /// <returns>The MultiTenantEntityTypeBuilder&lt;T&gt; instance.</returns>
        public static MultiTenantEntityTypeBuilder<T> AdjustUniqueIndexes<T>(this MultiTenantEntityTypeBuilder<T> builder) where T : class
        {
            // Update any unique contraints to include TenantId (unless they already do)
            var indexes = builder.Builder.Metadata.GetIndexes()
                                                  .Where(i => i.IsUnique)
                                                  .Where(i => !i.Properties.Select(p => p.Name).Contains("TenantId"))
                                                  .ToList();

            foreach (var index in indexes)
            {
                builder.AdjustIndex(index);
            }

            return builder;
        }
    }
}