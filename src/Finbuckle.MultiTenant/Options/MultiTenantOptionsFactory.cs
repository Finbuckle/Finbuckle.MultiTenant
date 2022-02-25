// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

//    Portions of this file are derived from the .NET Foundation source file located at:
//    https://github.com/dotnet/runtime/blob/5aad989cebe00f0987fcb842ea5b7cbe986c67df/src/libraries/Microsoft.Extensions.Options/src/OptionsFactory.cs

using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace Finbuckle.MultiTenant.Options
{
    /// <summary>
    /// Implementation of IOptionsFactory.
    /// </summary>
    /// <typeparam name="TOptions">The type of options being requested.</typeparam>
    public class MultiTenantOptionsFactory<TOptions, TTenantInfo> : IOptionsFactory<TOptions>
        where TOptions : class, new()
        where TTenantInfo : class, ITenantInfo, new()
    {
        private readonly IConfigureOptions<TOptions>[] _configureOptions;
        private readonly IPostConfigureOptions<TOptions>[] _postConfigureOptions;
        private readonly IValidateOptions<TOptions>[] _validations;

        private readonly ITenantConfigureOptions<TOptions, TTenantInfo>[] _tenantConfigureOptions;
        private readonly ITenantConfigureNamedOptions<TOptions, TTenantInfo>[] _tenantConfigureNamedOptions;
        private readonly IMultiTenantContextAccessor<TTenantInfo> _multiTenantContextAccessor;

        /// <summary>
        /// Initializes a new instance with the specified options configurations.
        /// </summary>
        public MultiTenantOptionsFactory(IEnumerable<IConfigureOptions<TOptions>> configureOptions, IEnumerable<IPostConfigureOptions<TOptions>> postConfigureOptions, IEnumerable<IValidateOptions<TOptions>> validations, IEnumerable<ITenantConfigureOptions<TOptions, TTenantInfo>> tenantConfigureOptions, IEnumerable<ITenantConfigureNamedOptions<TOptions, TTenantInfo>> tenantConfigureNamedOptions, IMultiTenantContextAccessor<TTenantInfo> multiTenantContextAccessor)
        {
            // The default DI container uses arrays under the covers. Take advantage of this knowledge
            // by checking for an array and enumerate over that, so we don't need to allocate an enumerator.
            // When it isn't already an array, convert it to one, but don't use System.Linq to avoid pulling Linq in to
            // small trimmed applications.

            _configureOptions = configureOptions as IConfigureOptions<TOptions>[] ?? new List<IConfigureOptions<TOptions>>(configureOptions).ToArray();
            _postConfigureOptions = postConfigureOptions as IPostConfigureOptions<TOptions>[] ?? new List<IPostConfigureOptions<TOptions>>(postConfigureOptions).ToArray();
            _validations = validations as IValidateOptions<TOptions>[] ?? new List<IValidateOptions<TOptions>>(validations).ToArray();
            _tenantConfigureOptions = tenantConfigureOptions as ITenantConfigureOptions<TOptions, TTenantInfo>[] ?? new List<ITenantConfigureOptions<TOptions, TTenantInfo>>(tenantConfigureOptions).ToArray();
            _tenantConfigureNamedOptions = tenantConfigureNamedOptions as ITenantConfigureNamedOptions<TOptions, TTenantInfo>[] ?? new List<ITenantConfigureNamedOptions<TOptions, TTenantInfo>>(tenantConfigureNamedOptions).ToArray();
            _multiTenantContextAccessor = multiTenantContextAccessor;
        }

        public TOptions Create(string name)
        {
            var options = new TOptions();
            foreach (var setup in _configureOptions)
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
            if (_multiTenantContextAccessor?.MultiTenantContext?.TenantInfo != null)
            {
                foreach (var tenantConfigureOption in _tenantConfigureOptions)
                    tenantConfigureOption.Configure(options, _multiTenantContextAccessor.MultiTenantContext.TenantInfo);
            }

            // Configure tenant named options.
            if (_multiTenantContextAccessor?.MultiTenantContext?.TenantInfo != null)
            {
                foreach (var tenantConfigureNamedOption in _tenantConfigureNamedOptions)
                    tenantConfigureNamedOption.Configure(name, options, _multiTenantContextAccessor.MultiTenantContext.TenantInfo);
            }

            foreach (var post in _postConfigureOptions)
            {
                post.PostConfigure(name, options);
            }

            if (_validations.Length > 0)
            {
                var failures = new List<string>();
                foreach (IValidateOptions<TOptions> validate in _validations)
                {
                    ValidateOptionsResult result = validate.Validate(name, options);
                    if (result is { Failed: true })
                    {
                        failures.AddRange(result.Failures);
                    }
                }
                if (failures.Count > 0)
                {
                    throw new OptionsValidationException(name, typeof(TOptions), failures);
                }
            }

            return options;
        }
    }
}
