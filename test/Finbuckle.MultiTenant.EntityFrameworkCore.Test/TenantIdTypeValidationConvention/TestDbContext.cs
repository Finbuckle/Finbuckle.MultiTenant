// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Microsoft.EntityFrameworkCore;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test.TenantIdTypeValidationConvention;

// A MultiTenantDbContext with a configurable tenant id type and model.
public class TenantIdTypeDbContext<TId>(Action<ModelBuilder> config, DbContextOptions options)
    : EntityFrameworkCore.MultiTenantDbContext<TId>(options) where TId : IEquatable<TId>
{
    public DbSet<TenantIdTypeThing> Things => Set<TenantIdTypeThing>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        config(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }
}

// Overrides ConfigureConventions without calling the base implementation, which skips the tenant id type check.
public class NoBaseConventionsDbContext(Action<ModelBuilder> config, DbContextOptions options)
    : TenantIdTypeDbContext<string>(config, options)
{
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
    }
}

public class TenantIdTypeThing
{
    public int Id { get; set; }
}

// The derived type's name sorts before its base type's name, so EF Core visits the derived type first.
public class ZBaseThing
{
    public int Id { get; set; }
}

public class ADerivedThing : ZBaseThing
{
}
