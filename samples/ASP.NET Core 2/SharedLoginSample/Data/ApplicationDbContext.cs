using System;
using System.Collections.Generic;
using System.Text;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SharedLoginSample.Data
{
    public class ApplicationDbContext : MultiTenantIdentityDbContext<IdentityUser>
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
