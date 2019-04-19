//    Copyright 2019 Andrew White
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.Core;
using Microsoft.Extensions.Logging;

namespace Finbuckle.MultiTenant.Strategies
{
    /// <summary>
    /// IMultiTenantStrategy implementation that executes a given delegate.
    /// </summary>
    public class DelegateStrategy : IMultiTenantStrategy
    {
        private readonly Func<object, Task<string>> doStrategy;
        private readonly ILogger<DelegateStrategy> logger;

        public DelegateStrategy(Func<object, Task<string>> implementation) :
            this(implementation, null)
        {
        }

        public DelegateStrategy(Func<object, Task<string>> doStrategy, ILogger<DelegateStrategy> logger)
        {
            this.doStrategy = doStrategy ?? throw new ArgumentNullException(nameof(doStrategy));
            this.logger = logger;
        }

        public async Task<string> GetIdentifierAsync(object context)
        {
            var identifier = await doStrategy(context);

            Utilities.TryLogInfo(logger, $"Found identifier: \"{identifier ?? "<null>"}\"");

            return await Task.FromResult(identifier);
        }
    }
}