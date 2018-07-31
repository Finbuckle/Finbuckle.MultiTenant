using System;
using System.Collections.Generic;
using System.Text;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityDataIsolationSample.Data
{
    public class ApplicationDbContext : MultiTenantIdentityDbContext
    {
        public ApplicationDbContext(TenantInfo tenantContext, DbContextOptions<ApplicationDbContext> options)
            : base(tenantContext, options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(ConnectionString);
            base.OnConfiguring(optionsBuilder);
        }
    }
}
