// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant;

public class StrategyInfo
{
    public Type? StrategyType { get; internal set; }
    public IMultiTenantStrategy? Strategy { get; internal set; }
}