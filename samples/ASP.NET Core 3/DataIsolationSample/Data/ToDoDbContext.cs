// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using DataIsolationSample.Models;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DataIsolationSample.Data
{
    public class ToDoDbContext : MultiTenantDbContext
    {
        public ToDoDbContext(ITenantInfo tenantInfo) : base(tenantInfo)
        {
        }

        public ToDoDbContext(ITenantInfo tenantInfo, DbContextOptions<ToDoDbContext> options) : base(tenantInfo, options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(TenantInfo.ConnectionString);
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ToDoItem>().IsMultiTenant();
            
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<ToDoItem> ToDoItems { get; set; }
    }
}