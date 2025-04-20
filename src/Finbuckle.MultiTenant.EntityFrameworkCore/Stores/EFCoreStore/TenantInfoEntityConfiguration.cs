// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Stores.EFCoreStore;

public class TenantInfoEntityConfiguration<TTenantInfo> : IEntityTypeConfiguration<TTenantInfo>
    where TTenantInfo : class, ITenantInfo, new()
{
    public virtual void Configure(EntityTypeBuilder<TTenantInfo> builder)
    {
        builder.HasKey(ti => ti.Id);
        builder.Property(ti => ti.Id).HasMaxLength(Internal.Constants.TenantIdMaxLength);
        builder.HasIndex(ti => ti.Identifier).IsUnique();
    }
}