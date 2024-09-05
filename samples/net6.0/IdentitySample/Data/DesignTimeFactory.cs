// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

// using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IdentitySample.Data;

public class SharedDesignTimeFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var tenantInfo = new AppTenantInfo{ ConnectionString = "Data Source=Data/SharedIdentity.db" };
        return new ApplicationDbContext(tenantInfo);
    }
}