// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Microsoft.Extensions.Options;

namespace Finbuckle.MultiTenant.Abstractions;

// ReSharper disable once TypeParameterCanBeVariant
interface IMultiTenantConfigureNamedOptions<TOptions> : IConfigureNamedOptions<TOptions>
    where TOptions : class
{
}
