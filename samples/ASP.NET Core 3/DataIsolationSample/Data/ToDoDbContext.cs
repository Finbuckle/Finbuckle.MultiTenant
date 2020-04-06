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

using DataIsolationSample.Models;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DataIsolationSample.Data
{
    public class ToDoDbContext : IdentityDbContext<IdentityUser>, IMultiTenantDbContext
    {
        public ToDoDbContext(TenantInfo tenantInfo)
        {
            this.TenantInfo = tenantInfo;
        }

        public ToDoDbContext(TenantInfo tenantInfo, DbContextOptions<ToDoDbContext> options) : base(options)
        {
            this.TenantInfo = tenantInfo;
        }

        protected string ConnectionString => TenantInfo.ConnectionString;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(ConnectionString);
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ToDoItem>().IsMultiTenant();
            
            base.OnModelCreating(modelBuilder);
        }

        public TenantInfo TenantInfo { get; }

        public TenantMismatchMode TenantMismatchMode { get; set; } = TenantMismatchMode.Throw;

        public TenantNotSetMode TenantNotSetMode { get; set; } = TenantNotSetMode.Throw;
    }
}