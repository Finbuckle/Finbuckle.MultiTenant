using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Finbuckle.MultiTenant.Blazor
{
    public class TenantSession
    {
        private readonly ConcurrentDictionary<string, object> sessionDictionary = new ConcurrentDictionary<string, object>();

        /// <summary>
        /// Try to get a value from the dictionary.
        /// </summary>
        /// <typeparam name="TValue">The type of the value requested.</typeparam>
        /// <param name="key">The key of the stored value.</param>
        /// <param name="value">The resulting value.</param>
        /// <returns>A boolean indicating whether or not the value was found in the dictionary.</returns>
        public bool TryGetValue<TValue>(string key, [MaybeNullWhen(false)] out TValue value)
        {
            if (this.sessionDictionary.TryGetValue(key, out var v))
            {
                if (v is TValue)
                {
                    value = (TValue)v;
                    return true;
                }

                value = default;
                return false;
            }
            else
            {
                value = default;
                return false;
            }
        }

        /// <summary>
        /// Store a value in the dictionary.
        /// </summary>
        /// <typeparam name="TValue">The type of the value to be stored.</typeparam>
        /// <param name="key">The key of the stored value.</param>
        /// <param name="value">The value to be stored.</param>
        public void SetValue<TValue>(string key, TValue value)
        {
            this.sessionDictionary.AddOrUpdate(key, value, (k, v) => { return value; });
        }

        /// <summary>
        /// Try to remove a value from the dictionary.
        /// </summary>
        /// <param name="key">The key of the stored value.</param>
        /// <returns>A boolean indicating whether or not the value was found in the dictionary.</returns>
        public bool TryRemove(string key)
        {
            if (this.sessionDictionary.TryRemove(key, out _))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
