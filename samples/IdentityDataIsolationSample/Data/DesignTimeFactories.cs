using System;
using System.Collections.Generic;
using System.Text;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IdentityDataIsolationSample.Data
{
    public class SharedDesignTimeFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var tenantInfo = new TenantInfo(null, null, null, "Data Source=Data/SharedIdentity.db", null);
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            return new ApplicationDbContext(tenantInfo, optionsBuilder.Options);
        }
    }

}
