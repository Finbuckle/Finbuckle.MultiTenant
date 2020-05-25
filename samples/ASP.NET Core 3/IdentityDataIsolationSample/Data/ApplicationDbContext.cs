﻿using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;

namespace IdentityDataIsolationSample.Data
{
    public class ApplicationDbContext : MultiTenantIdentityDbContext
    {
        public ApplicationDbContext(TenantInfo tenantInfo) : base(tenantInfo)
        {
        }

        public ApplicationDbContext(TenantInfo tenantInfo, DbContextOptions<ApplicationDbContext> options)
            : base(tenantInfo, options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(TenantInfo.ConnectionString);
            base.OnConfiguring(optionsBuilder);
        }
    }
}
