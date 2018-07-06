using System;
using System.Collections.Generic;
using System.Text;
using Finbuckle.MultiTenant.Core;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityDataIsolationSample.Data
{
    public class ApplicationDbContext : MultiTenantIdentityDbContext
    {
        public ApplicationDbContext(TenantContext tenantContext, DbContextOptions<ApplicationDbContext> options)
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
