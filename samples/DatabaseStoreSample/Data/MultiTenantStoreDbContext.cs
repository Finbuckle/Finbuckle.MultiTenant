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

using Finbuckle.MultiTenant;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace DatabaseStoreSample.Data
{

    public class MultiTenantStoreDbContext : DbContext
    {
        public MultiTenantStoreDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<TenantInfo> TenantInfo { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Ignore nontrivial properties on TenantInfo.
            modelBuilder.Entity<TenantInfo>().Ignore(ti => ti.MultiTenantContext).Ignore(ti => ti.Items);
        }
    }
}