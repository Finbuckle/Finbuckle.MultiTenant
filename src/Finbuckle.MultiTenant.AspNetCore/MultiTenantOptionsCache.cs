using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Finbuckle.MultiTenant.Core;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Finbuckle.MultiTenant.AspNetCore
{
    /// <summary>
    /// Adds, retrieves, and removes instances of TOptions after adjusting them for the current TenantContext.
    /// </summary>
    public class MultiTenantOptionsCache<TOptions> : OptionsCache<TOptions> where TOptions : class
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Action<TOptions, TenantContext> _tenantConfig;

        // Note: the object is just a dummy because there is no ConcurrentSet<T> class.
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, object>> _adjustedOptionsNames =
            new ConcurrentDictionary<string, ConcurrentDictionary<string, object>>();

        private TenantContext TenantContext { get => _httpContextAccessor.HttpContext?.GetTenantContextAsync().Result; }

        public MultiTenantOptionsCache(IHttpContextAccessor httpContextAccessor, Action<TOptions, TenantContext> tenantConfig)
        {
            _httpContextAccessor = httpContextAccessor;
            _tenantConfig = tenantConfig ?? throw new ArgumentNullException(nameof(tenantConfig));
        }

        /// <summary>
        /// Gets a named options instance, or adds a new instance created with createOptions.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="createOptions"></param>
        /// <returns></returns>
        public override TOptions GetOrAdd(string name, Func<TOptions> createOptions)
        {
            var adjustedOptionsName = AdjustOptionsName(TenantContext?.Id, name);
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
            var adjustedOptionsName = AdjustOptionsName(TenantContext?.Id, name);
            AdjustOptions(options, TenantContext?.Id);

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
        /// Concatenates a perfix string to the options name string.
        /// </summary>
        /// <remarks>
        /// If the prefix is null, an empty string is used. If name is null, the Options.DefaultName is used.
        /// </remarks>
        /// <param name="prefix"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private string AdjustOptionsName(string prefix, string name)
        {
            // Hash so that prefix + option name can't cause a collision. 
            byte[] buffer = Encoding.UTF8.GetBytes(prefix ?? "");
            var sha1 = System.Security.Cryptography.SHA1.Create();
            var hash = sha1.ComputeHash(buffer);
            prefix = Convert.ToBase64String(hash);

            return (prefix) + (name ?? Options.DefaultName);
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
            var options = createOptions();
            AdjustOptions(options, TenantContext?.Id);
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

        /// <summary>
        /// Adjust the options by running the configured action.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="tenantSubstitution"></param>
        private void AdjustOptions(TOptions options, string tenantSubstitution)
        {
            if (TenantContext != null)
            {
                _tenantConfig(options, TenantContext);
            }
        }
    }
}