// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

namespace Finbuckle.MultiTenant.AspNetCore.Options
{
    public class BasePathStrategyOptions
    {
        // TODO make this default to true in next major release
        public bool RebaseAspNetCorePathBase { get; set; } = false;
    }
}