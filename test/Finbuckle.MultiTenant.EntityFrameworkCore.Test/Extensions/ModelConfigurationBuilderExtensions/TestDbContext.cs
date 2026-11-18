// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test.Extensions.ModelConfigurationBuilderExtensions;

// Implements IMultiTenantDbContext directly rather than deriving from MultiTenantDbContext, so it only validates the
// tenant id type if it opts in from ConfigureConventions.
public class DirectMultiTenantDbContext : DbContext, IMultiTenantDbContext<string>
{
    private readonly Action<ModelConfigurationBuilder>? _conventions;
    private readonly Action<ModelBuilder> _model;

    public DirectMultiTenantDbContext(Action<ModelConfigurationBuilder>? conventions, Action<ModelBuilder> model,
        DbContextOptions options) : base(options)
    {
        _conventions = conventions;
        _model = model;
    }

    public ITenantInfo<string>? TenantInfo { get; set; }
    public TenantMismatchMode TenantMismatchMode => TenantMismatchMode.Throw;
    public TenantNotSetMode TenantNotSetMode => TenantNotSetMode.Throw;

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        _conventions?.Invoke(configurationBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) => _model(modelBuilder);
}

public class DirectThing
{
    public int Id { get; set; }
}

public class DirectThingWithIntTenantId
{
    public int Id { get; set; }
    public int TenantId { get; set; }
}
