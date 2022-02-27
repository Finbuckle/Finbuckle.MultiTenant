// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

// using System;
using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IdentitySample.Data;

public class SharedDesignTimeFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var tenantInfo = new TenantInfo{ ConnectionString = "Data Source=Data/SharedIdentity.db" };
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        return new ApplicationDbContext(tenantInfo, optionsBuilder.Options);
    }
}