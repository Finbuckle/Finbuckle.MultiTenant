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
using Microsoft.EntityFrameworkCore;

namespace DataIsolationSample.Data
{
    public class ToDoDbContext : MultiTenantDbContext
    {
        public ToDoDbContext(TenantInfo tenantInfo) : base(tenantInfo)
        {
        }

        public ToDoDbContext(TenantInfo tenantInfo, DbContextOptions<ToDoDbContext> options) : base(tenantInfo, options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(ConnectionString);
            base.OnConfiguring(optionsBuilder);
        }

        public DbSet<ToDoItem> ToDoItems { get; set; }
    }
}