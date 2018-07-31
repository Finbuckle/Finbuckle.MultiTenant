//    Copyright 2018 Andrew White
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Finbuckle.MultiTenant.Core;
using Microsoft.Extensions.Options;

namespace Finbuckle.MultiTenant.AspNetCore
{
    /// <summary>
    /// Adds, retrieves, and removes instances of TOptions after adjusting them for the current TenantContext.
    /// </summary>
    internal class MultiTenantOptionsCache<TOptions> : OptionsCache<TOptions> where TOptions : class
    {
        private readonly IMultiTenantContextAccessor multiTenantContextAccessor;

        // The object is just a dummy because there is no ConcurrentSet<T> class.
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, object>> _adjustedOptionsNames =
            new ConcurrentDictionary<string, ConcurrentDictionary<string, object>>();

        public MultiTenantOptionsCache(IMultiTenantContextAccessor tenantContextAccessor)
        {
            this.multiTenantContextAccessor = tenantContextAccessor ?? throw new ArgumentNullException(nameof(tenantContextAccessor));
        }

        /// <summary>
        /// Gets a named options instance, or adds a new instance created with createOptions.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="createOptions"></param>
        /// <returns></returns>
        public override TOptions GetOrAdd(string name, Func<TOptions> createOptions)
        {
            if (createOptions == null)
            {
                throw new ArgumentNullException(nameof(createOptions));
            }

            name = name ?? Options.DefaultName;
            var adjustedOptionsName = AdjustOptionsName(multiTenantContextAccessor.MultiTenantContext?.TenantInfo.Id, name);
            return base.GetOrAdd(adjustedOptionsName, () => MultiTenantFactoryWrapper(name, adjustedOptionsName, createOptions));
        }

        /// <summary>
        /// Tries to adds a new option to the cache, will return false if the name already exists.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override bool TryAdd(string name, TOptions options)
        {
            name = name ?? Options.DefaultName;

            var adjustedOptionsName = AdjustOptionsName(multiTenantContextAccessor?.MultiTenantContext.TenantInfo.Id, name);

            if (base.TryAdd(adjustedOptionsName, options))
            {
                CacheAdjustedOptionsName(name, adjustedOptionsName);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Try to remove an options instance. Removes for all tenants.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override bool TryRemove(string name)
        {
            name = name ?? Options.DefaultName;
            var result = false;

            if (!_adjustedOptionsNames.TryGetValue(name, out var adjustedOptionsNames))
                return false;

            List<string> removedNames = new List<string>();
            foreach (var adjustedOptionsName in adjustedOptionsNames)
            {
                if (base.TryRemove(adjustedOptionsName.Key))
                {
                    removedNames.Add(adjustedOptionsName.Key);
                    result = true;
                }
            }

            foreach (var removedName in removedNames)
            {
                adjustedOptionsNames.TryRemove(removedName, out var dummy);
            }

            return result;
        }

        /// <summary>
        /// Concatenates a prefix string to the options name string.
        /// </summary>
        /// <remarks>
        /// If the prefix is null, an empty string is used. If name is null, the Options.DefaultName is used.
        /// </remarks>
        /// <param name="prefix"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private string AdjustOptionsName(string prefix, string name)
        {
            if (name == null)
            {
                throw new MultiTenantException("", new ArgumentNullException(nameof(name)));
            }

            // Hash so that prefix + option name can't cause a collision. 
            byte[] buffer = Encoding.UTF8.GetBytes(prefix ?? "");
            var sha1 = System.Security.Cryptography.SHA1.Create();
            var hash = sha1.ComputeHash(buffer);
            prefix = Convert.ToBase64String(hash);

            return (prefix) + (name);
        }

        /// <summary>
        /// Creates an options instance, adjusted them according to the TenantContext, and caches the adjusted name.
        /// </summary>
        /// <param name="optionsName"></param>
        /// <param name="adjustedOptionsName"></param>
        /// <param name="createOptions"></param>
        /// <returns></returns>
        private TOptions MultiTenantFactoryWrapper(string optionsName, string adjustedOptionsName, Func<TOptions> createOptions)
        {
            if (createOptions == null)
            {
                throw new ArgumentNullException(nameof(createOptions));
            }

            var options = createOptions();
            CacheAdjustedOptionsName(optionsName, adjustedOptionsName);

            return options;
        }

        /// <summary>
        /// Caches an object's adjusted name indexed by the original name.
        /// </summary>
        /// <param name="optionsName"></param>
        /// <param name="adjustedOptionsName"></param>
        private void CacheAdjustedOptionsName(string optionsName, string adjustedOptionsName)
        {
            _adjustedOptionsNames.GetOrAdd(optionsName, new ConcurrentDictionary<string, object>()).TryAdd(adjustedOptionsName, null);
        }
    }
}