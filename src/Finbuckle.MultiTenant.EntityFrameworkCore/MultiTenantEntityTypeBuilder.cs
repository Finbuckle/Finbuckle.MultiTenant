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
    public class MultiTenantEntityTypeBuilder<T> where T : class
    {
        public EntityTypeBuilder<T> Builder { get; }

        public MultiTenantEntityTypeBuilder(EntityTypeBuilder<T> builder)
        {
            Builder = builder;
        }

        /// <summary>
        /// Adds TenantId to all unique indexes.
        /// </summary>
        /// <returns>The MultiTenantEntityTypeBuilder&lt;T&gt; instance.</returns>
        public MultiTenantEntityTypeBuilder<T> AdjustUniqueIndexes()
        {
            // Update any unique contraints to include TenantId (unless they already do)
            var indexes = Builder.Metadata.GetIndexes()
                                          .Where(i => i.IsUnique)
                                          .Where(i => !i.Properties.Select(p => p.Name).Contains("TenantId"))
                                          .ToList();

            foreach (var index in indexes)
            {
                AdjustIndex(index);
            }

            return this;
        }

        /// <summary>
        /// Adds TenantId to the index.
        /// </summary>
        /// <returns>The MultiTenantEntityTypeBuilder&lt;T&gt; instance.</returns>
        public MultiTenantEntityTypeBuilder<T> AdjustIndex(IMutableIndex index)
        {
            // Set the new unique index with TenantId preserving name and database name
            IndexBuilder indexBuilder = null;
#if NET
            Builder.Metadata.RemoveIndex(index);
            if (index.Name != null)
                indexBuilder = Builder.HasIndex(index.Properties.Select(p => p.Name).Append("TenantId").ToArray(), index.Name).HasDatabaseName(index.GetDatabaseName());
            else
                indexBuilder = Builder.HasIndex(index.Properties.Select(p => p.Name).Append("TenantId").ToArray()).HasDatabaseName(index.GetDatabaseName());
#elif NETSTANDARD2_1
            Builder.Metadata.RemoveIndex(index.Properties);
            indexBuilder = Builder.HasIndex(index.Properties.Select(p => p.Name).Append("TenantId").ToArray()).HasName(index.GetName());
#elif NETSTANDARD2_0
            Builder.Metadata.RemoveIndex(index.Properties);
            indexBuilder = Builder.HasIndex(index.Properties.Select(p => p.Name).Append("TenantId").ToArray()).HasName(index.Relational().Name);
#endif

            if(index.IsUnique)
                indexBuilder.IsUnique();

            return this;
        }
    }
}