// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Microsoft.Extensions.Options;

namespace Finbuckle.MultiTenant.Options;

interface ITenantConfigureNamedOptionsWrapper<TOptions> : IConfigureNamedOptions<TOptions>
    where TOptions : class, new()
{
}
