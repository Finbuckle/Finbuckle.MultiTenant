//    Copyright 2018-2020 Finbuckle LLC, Andrew White, and Contributors
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

//    Portions of this file are derived from the .NET Foundation source file located at:
//    https://github.com/aspnet/Options/blob/dev/src/Microsoft.Extensions.Options/OptionsManager.cs

using Microsoft.Extensions.Options;

namespace Finbuckle.MultiTenant.Options
{
    /// <summary>
    /// Implementation of IOptions and IOptionsSnapshot that uses dependency injection for its private cache.
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    internal class MultiTenantOptionsManager<TOptions> : IOptions<TOptions>, IOptionsSnapshot<TOptions> where TOptions : class, new()
    {
        private readonly IOptionsFactory<TOptions> _factory;
        private readonly IOptionsMonitorCache<TOptions> _cache; // Note: this is a private cache

        /// <summary>
        /// Initializes a new instance with the specified options configurations.
        /// </summary>
        /// <param name="factory">The factory to use to create options.</param>
        public MultiTenantOptionsManager(IOptionsFactory<TOptions> factory, IOptionsMonitorCache<TOptions> cache)
        {
            _factory = factory;
            _cache = cache;
        }

        public TOptions Value
        {
            get
            {
                return Get(Microsoft.Extensions.Options.Options.DefaultName);
            }
        }

        public virtual TOptions Get(string name)
        {
            name = name ?? Microsoft.Extensions.Options.Options.DefaultName;

            // Store the options in our instance cache.
            return _cache.GetOrAdd(name, () => _factory.Create(name));
        }

        public void Reset()
        {
            _cache.Clear();
        }
    }
}