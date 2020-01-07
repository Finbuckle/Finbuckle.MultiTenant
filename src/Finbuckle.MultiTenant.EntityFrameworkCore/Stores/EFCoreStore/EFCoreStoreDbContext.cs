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

using Finbuckle.MultiTenant.Core;
using Microsoft.EntityFrameworkCore;

namespace Finbuckle.MultiTenant.Stores
{
    public class EFCoreStoreDbContext : DbContext
    {
        public EFCoreStoreDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<TenantInfo> TenantInfo { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TenantInfo>().HasKey(ti => ti.Id);
            modelBuilder.Entity<TenantInfo>().Property(ti => ti.Id).HasMaxLength(Constants.TenantIdMaxLength);
            modelBuilder.Entity<TenantInfo>().HasIndex(ti => ti.Identifier).IsUnique();
            modelBuilder.Entity<TenantInfo>().Property(ti => ti.Name).IsRequired();
            modelBuilder.Entity<TenantInfo>().Property(ti => ti.ConnectionString).IsRequired();
            modelBuilder.Entity<TenantInfo>().Ignore(p => p.Items).Ignore(p => p.MultiTenantContext);
        }
    }
}