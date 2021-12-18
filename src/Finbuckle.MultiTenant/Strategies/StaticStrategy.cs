// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System.Threading.Tasks;

namespace Finbuckle.MultiTenant.Strategies
{
    public class StaticStrategy : IMultiTenantStrategy
    {
        internal readonly string identifier;

        public int Priority { get => -1000; }

        public StaticStrategy(string identifier)
        {
            this.identifier = identifier;
        }

        public async Task<string?> GetIdentifierAsync(object context)
        {
            return await Task.FromResult(identifier);
        }
    }
}