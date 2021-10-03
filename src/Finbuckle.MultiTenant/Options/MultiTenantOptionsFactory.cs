// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

//    Portions of this file are derived from the .NET Foundation source file located at:
//    https://github.com/aspnet/Options/blob/dev/src/Microsoft.Extensions.Options/OptionsFactory.cs

using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace Finbuckle.MultiTenant.Options
{
    /// <summary>
    /// Implementation of IOptionsFactory.
    /// </summary>
    /// <typeparam name="TOptions">The type of options being requested.</typeparam>
    internal class MultiTenantOptionsFactory<TOptions, TTenantInfo> : IOptionsFactory<TOptions>
        where TOptions : class, new()
        where TTenantInfo : class, ITenantInfo, new()
    {
        private readonly IEnumerable<IConfigureOptions<TOptions>> configureOptions;
        private readonly IEnumerable<ITenantConfigureOptions<TOptions, TTenantInfo>> tenantConfigureOptions;
        private readonly IMultiTenantContextAccessor<TTenantInfo> multiTenantContextAccessor;
        private readonly IEnumerable<IPostConfigureOptions<TOptions>> postConfigureOptions;

        /// <summary>
        /// Initializes a new instance with the specified options configurations.
        /// </summary>
        /// <param name="configureOptions">The configuration actions to run.</param>
        /// <param name="postConfigures">The initialization actions to run.</param>
        public MultiTenantOptionsFactory(IEnumerable<IConfigureOptions<TOptions>> configureOptions, IEnumerable<IPostConfigureOptions<TOptions>> postConfigureOptions, IEnumerable<ITenantConfigureOptions<TOptions, TTenantInfo>> tenantConfigureOptions, IMultiTenantContextAccessor<TTenantInfo> multiTenantContextAccessor)
        {
            this.configureOptions = configureOptions;
            this.tenantConfigureOptions = tenantConfigureOptions;
            this.multiTenantContextAccessor = multiTenantContextAccessor;
            this.postConfigureOptions = postConfigureOptions;
        }

        public TOptions Create(string name)
        {
            var options = new TOptions();
            foreach (var setup in configureOptions)
            {
                if (setup is IConfigureNamedOptions<TOptions> namedSetup)
                {
                    namedSetup.Configure(name, options);
                }
                else if (name == Microsoft.Extensions.Options.Options.DefaultName)
                {
                    setup.Configure(options);
                }
            }

            // Configure tenant options.
            if(multiTenantContextAccessor?.MultiTenantContext?.TenantInfo != null)
            {
                foreach(var tenantConfigureOption in tenantConfigureOptions)
                    tenantConfigureOption.Configure(options, multiTenantContextAccessor.MultiTenantContext.TenantInfo);
            }

            foreach (var post in postConfigureOptions)
            {
                post.PostConfigure(name, options);
            }
            return options;
        }
    }
}
