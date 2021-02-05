using System;
using System.Collections.Generic;
using System.Text;

using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Stores;

using Microsoft.EntityFrameworkCore;

namespace FunctionsEFCoreStoreSample.Data
{
    public class MultiTenantStoreDbContext : EFCoreStoreDbContext<TenantInfo>
    {
        public MultiTenantStoreDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("EFCoreStoreSampleConnectionString");
            base.OnConfiguring(optionsBuilder);
        }
    }
}
