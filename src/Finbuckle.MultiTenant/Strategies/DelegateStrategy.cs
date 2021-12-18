// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;
using System.Threading.Tasks;

namespace Finbuckle.MultiTenant.Strategies
{
    public class DelegateStrategy : IMultiTenantStrategy
    {
        private readonly Func<object, Task<string>> doStrategy;

        public DelegateStrategy(Func<object, Task<string>> doStrategy)
        {
            this.doStrategy = doStrategy ?? throw new ArgumentNullException(nameof(doStrategy));
        }

        public async Task<string?> GetIdentifierAsync(object context)
        {
            var identifier = await doStrategy(context);
            return await Task.FromResult(identifier);
        }
    }
}