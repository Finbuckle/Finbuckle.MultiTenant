// using System;
using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IdentityDataIsolationSample.Data
{
    public class SharedDesignTimeFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // To prep each database uncomment the corresponding line below.
            var tenantInfo = new TenantInfo{ ConnectionString = "Data Source=Data/SharedIdentity.db" };
            // var tenantInfo = new TenantInfo{ ConnectionString = "Data Source=Data/InitechIdentity.db" };
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            return new ApplicationDbContext(tenantInfo, optionsBuilder.Options);
        }
    }
}
