//    Copyright 2018-2020 Andrew White
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
using Finbuckle.MultiTenant.Internal;
using Microsoft.Extensions.Logging;

namespace Finbuckle.MultiTenant.Strategies
{
    public class MultiTenantStrategyWrapper : IMultiTenantStrategy
    {
        public IMultiTenantStrategy Strategy { get; }

        private readonly ILogger logger;

        public MultiTenantStrategyWrapper(IMultiTenantStrategy strategy, ILogger logger)
        {
            this.Strategy = strategy;
            this.logger = logger;
        }

        public async Task<string> GetIdentifierAsync(object context)
        {
            if (context == null)
            {
                throw new System.ArgumentNullException(nameof(context));
            }

            string identifier = null;

            try
            {
                identifier = await Strategy.GetIdentifierAsync(context);
            }
            catch (Exception e)
            {
                var errorMessage = $"Exception in {Strategy.GetType()}.GetIdentifierAsync.";
                Utilities.TryLogError(logger, errorMessage, e);
                throw new MultiTenantException(errorMessage, e);
            }

            if(identifier != null)
            {                
                Utilities.TryLogDebug(logger, $"{Strategy.GetType()}.GetIdentifierAsync: Found identifier: \"{identifier}\".");
            }
            else
            {
                Utilities.TryLogDebug(logger, $"{Strategy.GetType()}.GetIdentifierAsync: No identifier found.");
            }

            return identifier;
        }
    }
}
