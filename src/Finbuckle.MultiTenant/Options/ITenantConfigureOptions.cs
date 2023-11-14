// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Microsoft.Extensions.Options;
using System;

namespace Finbuckle.MultiTenant.Options;

[Obsolete]
public interface ITenantConfigureOptions<TOptions, TTenantInfo>
    where TOptions : class
    where TTenantInfo : class, ITenantInfo, new()
{
    void Configure(TOptions options, TTenantInfo tenantInfo);
}
