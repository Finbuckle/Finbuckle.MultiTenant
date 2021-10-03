// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

//    Portions of this file are derived from the .NET Foundation source file located at:
//    https://github.com/aspnet/Options/blob/dev/src/Microsoft.Extensions.Options/OptionsManager.cs

using Microsoft.Extensions.Options;

namespace Finbuckle.MultiTenant.Options
{
    /// <summary>
    /// Implementation of IOptions and IOptionsSnapshot that uses dependency injection for its private cache.
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    public class MultiTenantOptionsManager<TOptions> : IOptions<TOptions>, IOptionsSnapshot<TOptions> where TOptions : class, new()
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