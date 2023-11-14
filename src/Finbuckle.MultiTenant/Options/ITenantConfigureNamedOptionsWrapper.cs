// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System;
using Microsoft.Extensions.Options;

namespace Finbuckle.MultiTenant.Options;

[Obsolete]
interface ITenantConfigureNamedOptionsWrapper<TOptions> : IConfigureNamedOptions<TOptions>
    where TOptions : class
{
}
