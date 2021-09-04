using System;
using Microsoft.Extensions.Options;

namespace Finbuckle.MultiTenant.AspNetCore.OptionsCacheReset.Internal
{
    internal class MultiTenantOptionMark
    {
        public MultiTenantOptionMark(Type optionType)
        {
            OptionsMonitorCacheOptionType = typeof(IOptionsMonitorCache<>).MakeGenericType(optionType);
            OptionsCacheOptionType = typeof(IOptions<>).MakeGenericType(optionType);
        }

        public Type OptionsMonitorCacheOptionType { get; }
        public Type OptionsCacheOptionType { get; }
    }
}