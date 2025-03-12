// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant.Strategies;

public class StaticStrategy : IMultiTenantStrategy
{
    // internal for testing
    // ReSharper disable once MemberCanBePrivate.Global
    internal readonly string Identifier;

    public int Priority => -1000;

    public StaticStrategy(string identifier)
    {
        this.Identifier = identifier;
    }

    public async Task<string?> GetIdentifierAsync(object context)
    {
        return await Task.FromResult(Identifier).ConfigureAwait(false);
    }
}