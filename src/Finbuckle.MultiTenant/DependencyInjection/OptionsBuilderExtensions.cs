// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

//    Portions of this file are derived from the .NET Foundation source file located at:
//    https://github.com/dotnet/runtime/blob/main/src/libraries/Microsoft.Extensions.Options/src/OptionsBuilder.cs

using Finbuckle.MultiTenant;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

// ReSharper disable once CheckNamespace
namespace Finbuckle.Extensions.DependencyInjection;

public static class OptionsBuilderExtensions
{
    public static OptionsBuilder<TOptions> ConfigurePerTenant<TOptions, TTenantInfo>(
        this OptionsBuilder<TOptions> optionsBuilder, Action<TOptions, TTenantInfo> configureOptions)
        where TOptions : class
        where TTenantInfo : class, ITenantInfo, new()
    {
        FinbuckleServiceCollectionExtensions.ConfigurePerTenantReqs<TOptions>(optionsBuilder.Services);

        optionsBuilder.Services.AddTransient<IConfigureOptions<TOptions>>(sp =>
            new ConfigureNamedOptions<TOptions, TTenantInfo?>(
                optionsBuilder.Name,
                sp.GetService<TTenantInfo>(),
                (options, dep) =>
                {
                    if (dep is not null)
                        configureOptions(options, dep);
                }));
        
        return optionsBuilder;
    }

    public static OptionsBuilder<TOptions> ConfigurePerTenant<TOptions, TDep, TTenantInfo>(
        this OptionsBuilder<TOptions> optionsBuilder, Action<TOptions, TDep, TTenantInfo> configureOptions)
        where TOptions : class
        where TDep : class
        where TTenantInfo : class, ITenantInfo, new()
    {
        FinbuckleServiceCollectionExtensions.ConfigurePerTenantReqs<TOptions>(optionsBuilder.Services);

        optionsBuilder.Services.AddTransient<IConfigureOptions<TOptions>>(sp =>
            new ConfigureNamedOptions<TOptions, TDep, TTenantInfo?>(
                optionsBuilder.Name,
                sp.GetRequiredService<TDep>(),
                sp.GetService<TTenantInfo>(),
                (options, dep, tenantInfo) =>
                {
                    if (tenantInfo is not null)
                        configureOptions(options, dep, tenantInfo);
                }));

        return optionsBuilder;
    }

    public static OptionsBuilder<TOptions> ConfigurePerTenant<TOptions, TDep1, TDep2, TTenantInfo>(
        this OptionsBuilder<TOptions> optionsBuilder, Action<TOptions, TDep1, TDep2, TTenantInfo> configureOptions)
        where TOptions : class
        where TDep1 : class
        where TDep2 : class
        where TTenantInfo : class, ITenantInfo, new()
    {
        FinbuckleServiceCollectionExtensions.ConfigurePerTenantReqs<TOptions>(optionsBuilder.Services);

        optionsBuilder.Services.AddTransient<IConfigureOptions<TOptions>>(sp =>
            new ConfigureNamedOptions<TOptions, TDep1, TDep2, TTenantInfo?>(
                optionsBuilder.Name,
                sp.GetRequiredService<TDep1>(),
                sp.GetRequiredService<TDep2>(),
                sp.GetService<TTenantInfo>(),
                (options, dep1, dep2, tenantInfo) =>
                {
                    if (tenantInfo is not null)
                        configureOptions(options, dep1, dep2, tenantInfo);
                }));

        return optionsBuilder;
    }

    public static OptionsBuilder<TOptions> ConfigurePerTenant<TOptions, TDep1, TDep2, TDep3, TTenantInfo>(
        this OptionsBuilder<TOptions> optionsBuilder,
        Action<TOptions, TDep1, TDep2, TDep3, TTenantInfo> configureOptions)
        where TOptions : class
        where TDep1 : class
        where TDep2 : class
        where TDep3 : class
        where TTenantInfo : class, ITenantInfo, new()
    {
        FinbuckleServiceCollectionExtensions.ConfigurePerTenantReqs<TOptions>(optionsBuilder.Services);

        optionsBuilder.Services.AddTransient<IConfigureOptions<TOptions>>(sp =>
            new ConfigureNamedOptions<TOptions, TDep1, TDep2, TDep3, TTenantInfo?>(
                optionsBuilder.Name,
                sp.GetRequiredService<TDep1>(),
                sp.GetRequiredService<TDep2>(),
                sp.GetRequiredService<TDep3>(),
                sp.GetService<TTenantInfo>(),
                (options, dep1, dep2, dep3, tenantInfo) =>
                {
                    if (tenantInfo is not null)
                        configureOptions(options, dep1, dep2, dep3, tenantInfo);
                }));

        return optionsBuilder;
    }

    public static OptionsBuilder<TOptions> ConfigurePerTenant<TOptions, TDep1, TDep2, TDep3, TDep4, TTenantInfo>(
        this OptionsBuilder<TOptions> optionsBuilder,
        Action<TOptions, TDep1, TDep2, TDep3, TDep4, TTenantInfo> configureOptions)
        where TOptions : class
        where TDep1 : class
        where TDep2 : class
        where TDep3 : class
        where TDep4 : class
        where TTenantInfo : class, ITenantInfo, new()
    {
        FinbuckleServiceCollectionExtensions.ConfigurePerTenantReqs<TOptions>(optionsBuilder.Services);

        optionsBuilder.Services.AddTransient<IConfigureOptions<TOptions>>(sp =>
            new ConfigureNamedOptions<TOptions, TDep1, TDep2, TDep3, TDep4, TTenantInfo?>(
                optionsBuilder.Name,
                sp.GetRequiredService<TDep1>(),
                sp.GetRequiredService<TDep2>(),
                sp.GetRequiredService<TDep3>(),
                sp.GetRequiredService<TDep4>(),
                sp.GetService<TTenantInfo>(),
                (options, dep1, dep2, dep3, dep4, tenantInfo) =>
                {
                    if (tenantInfo is not null)
                        configureOptions(options, dep1, dep2, dep3, dep4, tenantInfo);
                }));

        return optionsBuilder;
    }

    public static OptionsBuilder<TOptions> ConfigurePerTenant<TOptions, TDep1, TDep2, TDep3, TDep4, TDep5, TTenantInfo>(
        this OptionsBuilder<TOptions> optionsBuilder,
        Action<TOptions, TDep1, TDep2, TDep3, TDep4, TDep5, TTenantInfo> configureOptions)
        where TOptions : class
        where TDep1 : class
        where TDep2 : class
        where TDep3 : class
        where TDep4 : class
        where TDep5 : class
        where TTenantInfo : class, ITenantInfo, new()
    {
        FinbuckleServiceCollectionExtensions.ConfigurePerTenantReqs<TOptions>(optionsBuilder.Services);

        optionsBuilder.Services.AddTransient<IConfigureOptions<TOptions>>(sp =>
        {
            var tenantInfo = sp.GetService<TTenantInfo>();
            return new ConfigureNamedOptions<TOptions, TDep1, TDep2, TDep3, TDep4, TDep5>(
                optionsBuilder.Name,
                sp.GetRequiredService<TDep1>(),
                sp.GetRequiredService<TDep2>(),
                sp.GetRequiredService<TDep3>(),
                sp.GetRequiredService<TDep4>(),
                sp.GetRequiredService<TDep5>(),
                (options, dep1, dep2, dep3, dep4, dep5) =>
                {
                    if (tenantInfo is not null)
                        configureOptions(options, dep1, dep2, dep3, dep4, dep5, tenantInfo);
                });
        });

        return optionsBuilder;
    }
}
//
// /// <summary>
// /// Used to configure <typeparamref name="TOptions"/> instances per-tenant.
// /// </summary>
// /// <typeparam name="TOptions">The type of options being requested.</typeparam>
// /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
// public class PerTenantOptionsBuilder<TOptions, TTenantInfo>
//     where TOptions : class
//     where TTenantInfo : class, ITenantInfo, new()
// {
//     private const string DefaultValidationFailureMessage = "A validation error has occurred.";
//
//     /// <summary>
//     /// The default name of the <typeparamref name="TOptions"/> instance.
//     /// </summary>
//     public string Name { get; }
//
//     /// <summary>
//     /// The <see cref="IServiceCollection"/> for the options being configured.
//     /// </summary>
//     public IServiceCollection Services { get; }
//
//     /// <summary>
//     /// Constructor.
//     /// </summary>
//     /// <param name="services">The <see cref="IServiceCollection"/> for the options being configured.</param>
//     /// <param name="name">The default name of the <typeparamref name="TOptions"/> instance, if null <see cref="Options.DefaultName"/> is used.</param>
//     public PerTenantOptionsBuilder(IServiceCollection services, string? name)
//     {
//         Services = services ?? throw new ArgumentNullException(nameof(services));
//         Name = name ?? Options.DefaultName;
//     }
//
//     /// <summary>
//     /// Registers an action used to configure a particular type of options.
//     /// Note: These are run before all <seealso cref="PostConfigure(Action{TOptions})"/>.
//     /// </summary>
//     /// <param name="configureOptions">The action used to configure the options.</param>
//     /// <returns>The current <see cref="PerTenantOptionsBuilder{TOptions, TTenantInfo}"/>.</returns>
//     public virtual PerTenantOptionsBuilder<TOptions, TTenantInfo> Configure(
//         Action<TOptions, TTenantInfo> configureOptions)
//     {
//         if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));
//         Services.ConfigurePerTenant(Name, configureOptions);
//         return this;
//     }
//
//     /// <summary>
//     /// Registers an action used to configure a particular type of options.
//     /// Note: These are run before all <seealso cref="PostConfigure(Action{TOptions})"/>.
//     /// </summary>
//     /// <typeparam name="TDep">A dependency used by the action.</typeparam>
//     /// <param name="configureOptions">The action used to configure the options.</param>
//     /// <returns>The current <see cref="PerTenantOptionsBuilder{TOptions, TTenantInfo}"/>.</returns>
//     public virtual PerTenantOptionsBuilder<TOptions, TTenantInfo> Configure<TDep>(
//         Action<TOptions, TDep, TTenantInfo> configureOptions)
//         where TDep : class
//     {
//         if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));
//
//         Services.AddTransient<IConfigureOptions<TOptions>>(sp =>
//         {
//             var multiTenantContextAccessor = sp.GetRequiredService<IMultiTenantContextAccessor<TTenantInfo>>();
//
//             return new ConfigureNamedOptions<TOptions, TDep>(Name, sp.GetRequiredService<TDep>(), (options, dep1) =>
//             {
//                 var multiTenantContext = multiTenantContextAccessor.MultiTenantContext;
//                 if (multiTenantContext.HasResolvedTenant)
//                     configureOptions(options, dep1, multiTenantContext.TenantInfo);
//             });
//         });
//
//         return this;
//     }
//
//     /// <summary>
//     /// Registers an action used to configure a particular type of options.
//     /// Note: These are run before all <seealso cref="PostConfigure(Action{TOptions})"/>.
//     /// </summary>
//     /// <typeparam name="TDep1">The first dependency used by the action.</typeparam>
//     /// <typeparam name="TDep2">The second dependency used by the action.</typeparam>
//     /// <param name="configureOptions">The action used to configure the options.</param>
//     /// <returns>The current <see cref="PerTenantOptionsBuilder{TOptions, TTenantInfo}"/>.</returns>
//     public virtual PerTenantOptionsBuilder<TOptions, TTenantInfo> Configure<TDep1, TDep2>(
//         Action<TOptions, TDep1, TDep2> configureOptions)
//         where TDep1 : class
//         where TDep2 : class
//     {
//         if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));
//
//         Services.AddTransient<IConfigureOptions<TOptions>>(sp =>
//             new ConfigureNamedOptions<TOptions, TDep1, TDep2>(Name, sp.GetRequiredService<TDep1>(),
//                 sp.GetRequiredService<TDep2>(), configureOptions));
//         return this;
//     }
//
//     /// <summary>
//     /// Registers an action used to configure a particular type of options.
//     /// Note: These are run before all <seealso cref="PostConfigure(Action{TOptions})"/>.
//     /// </summary>
//     /// <typeparam name="TDep1">The first dependency used by the action.</typeparam>
//     /// <typeparam name="TDep2">The second dependency used by the action.</typeparam>
//     /// <typeparam name="TDep3">The third dependency used by the action.</typeparam>
//     /// <param name="configureOptions">The action used to configure the options.</param>
//     /// <returns>The current <see cref="PerTenantOptionsBuilder{TOptions, TTenantInfo}"/>.</returns>
//     public virtual PerTenantOptionsBuilder<TOptions, TTenantInfo> Configure<TDep1, TDep2, TDep3>(
//         Action<TOptions, TDep1, TDep2, TDep3> configureOptions)
//         where TDep1 : class
//         where TDep2 : class
//         where TDep3 : class
//     {
//         if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));
//
//         Services.AddTransient<IConfigureOptions<TOptions>>(
//             sp => new ConfigureNamedOptions<TOptions, TDep1, TDep2, TDep3>(
//                 Name,
//                 sp.GetRequiredService<TDep1>(),
//                 sp.GetRequiredService<TDep2>(),
//                 sp.GetRequiredService<TDep3>(),
//                 configureOptions));
//         return this;
//     }
//
//     /// <summary>
//     /// Registers an action used to configure a particular type of options.
//     /// Note: These are run before all <seealso cref="PostConfigure(Action{TOptions})"/>.
//     /// </summary>
//     /// <typeparam name="TDep1">The first dependency used by the action.</typeparam>
//     /// <typeparam name="TDep2">The second dependency used by the action.</typeparam>
//     /// <typeparam name="TDep3">The third dependency used by the action.</typeparam>
//     /// <typeparam name="TDep4">The fourth dependency used by the action.</typeparam>
//     /// <param name="configureOptions">The action used to configure the options.</param>
//     /// <returns>The current <see cref="PerTenantOptionsBuilder{TOptions, TTenantInfo}"/>.</returns>
//     public virtual PerTenantOptionsBuilder<TOptions, TTenantInfo> Configure<TDep1, TDep2, TDep3, TDep4>(
//         Action<TOptions, TDep1, TDep2, TDep3, TDep4> configureOptions)
//         where TDep1 : class
//         where TDep2 : class
//         where TDep3 : class
//         where TDep4 : class
//     {
//         if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));
//
//         Services.AddTransient<IConfigureOptions<TOptions>>(
//             sp => new ConfigureNamedOptions<TOptions, TDep1, TDep2, TDep3, TDep4>(
//                 Name,
//                 sp.GetRequiredService<TDep1>(),
//                 sp.GetRequiredService<TDep2>(),
//                 sp.GetRequiredService<TDep3>(),
//                 sp.GetRequiredService<TDep4>(),
//                 configureOptions));
//         return this;
//     }
//
//     /// <summary>
//     /// Registers an action used to configure a particular type of options.
//     /// Note: These are run before all <seealso cref="PostConfigure(Action{TOptions})"/>.
//     /// </summary>
//     /// <typeparam name="TDep1">The first dependency used by the action.</typeparam>
//     /// <typeparam name="TDep2">The second dependency used by the action.</typeparam>
//     /// <typeparam name="TDep3">The third dependency used by the action.</typeparam>
//     /// <typeparam name="TDep4">The fourth dependency used by the action.</typeparam>
//     /// <typeparam name="TDep5">The fifth dependency used by the action.</typeparam>
//     /// <param name="configureOptions">The action used to configure the options.</param>
//     /// <returns>The current <see cref="PerTenantOptionsBuilder{TOptions, TTenantInfo}"/>.</returns>
//     public virtual PerTenantOptionsBuilder<TOptions, TTenantInfo> Configure<TDep1, TDep2, TDep3, TDep4, TDep5>(
//         Action<TOptions, TDep1, TDep2, TDep3, TDep4, TDep5> configureOptions)
//         where TDep1 : class
//         where TDep2 : class
//         where TDep3 : class
//         where TDep4 : class
//         where TDep5 : class
//     {
//         if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));
//
//         Services.AddTransient<IConfigureOptions<TOptions>>(
//             sp => new ConfigureNamedOptions<TOptions, TDep1, TDep2, TDep3, TDep4, TDep5>(
//                 Name,
//                 sp.GetRequiredService<TDep1>(),
//                 sp.GetRequiredService<TDep2>(),
//                 sp.GetRequiredService<TDep3>(),
//                 sp.GetRequiredService<TDep4>(),
//                 sp.GetRequiredService<TDep5>(),
//                 configureOptions));
//         return this;
//     }
//
//     /// <summary>
//     /// Registers an action used to configure a particular type of options.
//     /// Note: These are run after all <seealso cref="Configure(Action{TOptions})"/>.
//     /// </summary>
//     /// <param name="configureOptions">The action used to configure the options.</param>
//     /// <returns>The current <see cref="PerTenantOptionsBuilder{TOptions, TTenantInfo}"/>.</returns>
//     public virtual PerTenantOptionsBuilder<TOptions, TTenantInfo> PostConfigure(Action<TOptions> configureOptions)
//     {
//         if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));
//
//         Services.AddSingleton<IPostConfigureOptions<TOptions>>(
//             new PostConfigureOptions<TOptions>(Name, configureOptions));
//         return this;
//     }
//
//     /// <summary>
//     /// Registers an action used to post configure a particular type of options.
//     /// Note: These are run after all <seealso cref="Configure(Action{TOptions})"/>.
//     /// </summary>
//     /// <typeparam name="TDep">The dependency used by the action.</typeparam>
//     /// <param name="configureOptions">The action used to configure the options.</param>
//     /// <returns>The current <see cref="PerTenantOptionsBuilder{TOptions, TTenantInfo}"/>.</returns>
//     public virtual PerTenantOptionsBuilder<TOptions, TTenantInfo> PostConfigure<TDep>(
//         Action<TOptions, TDep> configureOptions)
//         where TDep : class
//     {
//         if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));
//
//         Services.AddTransient<IPostConfigureOptions<TOptions>>(sp =>
//             new PostConfigureOptions<TOptions, TDep>(Name, sp.GetRequiredService<TDep>(), configureOptions));
//         return this;
//     }
//
//     /// <summary>
//     /// Registers an action used to post configure a particular type of options.
//     /// Note: These are run after all <seealso cref="Configure(Action{TOptions})"/>.
//     /// </summary>
//     /// <typeparam name="TDep1">The first dependency used by the action.</typeparam>
//     /// <typeparam name="TDep2">The second dependency used by the action.</typeparam>
//     /// <param name="configureOptions">The action used to configure the options.</param>
//     /// <returns>The current <see cref="PerTenantOptionsBuilder{TOptions, TTenantInfo}"/>.</returns>
//     public virtual PerTenantOptionsBuilder<TOptions, TTenantInfo> PostConfigure<TDep1, TDep2>(
//         Action<TOptions, TDep1, TDep2> configureOptions)
//         where TDep1 : class
//         where TDep2 : class
//     {
//         if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));
//
//         Services.AddTransient<IPostConfigureOptions<TOptions>>(sp =>
//             new PostConfigureOptions<TOptions, TDep1, TDep2>(Name, sp.GetRequiredService<TDep1>(),
//                 sp.GetRequiredService<TDep2>(), configureOptions));
//         return this;
//     }
//
//     /// <summary>
//     /// Registers an action used to post configure a particular type of options.
//     /// Note: These are run after all <seealso cref="Configure(Action{TOptions})"/>.
//     /// </summary>
//     /// <typeparam name="TDep1">The first dependency used by the action.</typeparam>
//     /// <typeparam name="TDep2">The second dependency used by the action.</typeparam>
//     /// <typeparam name="TDep3">The third dependency used by the action.</typeparam>
//     /// <param name="configureOptions">The action used to configure the options.</param>
//     /// <returns>The current <see cref="PerTenantOptionsBuilder{TOptions, TTenantInfo}"/>.</returns>
//     public virtual PerTenantOptionsBuilder<TOptions, TTenantInfo> PostConfigure<TDep1, TDep2, TDep3>(
//         Action<TOptions, TDep1, TDep2, TDep3> configureOptions)
//         where TDep1 : class
//         where TDep2 : class
//         where TDep3 : class
//     {
//         if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));
//
//         Services.AddTransient<IPostConfigureOptions<TOptions>>(
//             sp => new PostConfigureOptions<TOptions, TDep1, TDep2, TDep3>(
//                 Name,
//                 sp.GetRequiredService<TDep1>(),
//                 sp.GetRequiredService<TDep2>(),
//                 sp.GetRequiredService<TDep3>(),
//                 configureOptions));
//         return this;
//     }
//
//     /// <summary>
//     /// Registers an action used to post configure a particular type of options.
//     /// Note: These are run after all <seealso cref="Configure(Action{TOptions})"/>.
//     /// </summary>
//     /// <typeparam name="TDep1">The first dependency used by the action.</typeparam>
//     /// <typeparam name="TDep2">The second dependency used by the action.</typeparam>
//     /// <typeparam name="TDep3">The third dependency used by the action.</typeparam>
//     /// <typeparam name="TDep4">The fourth dependency used by the action.</typeparam>
//     /// <param name="configureOptions">The action used to configure the options.</param>
//     /// <returns>The current <see cref="PerTenantOptionsBuilder{TOptions, TTenantInfo}"/>.</returns>
//     public virtual PerTenantOptionsBuilder<TOptions, TTenantInfo> PostConfigure<TDep1, TDep2, TDep3, TDep4>(
//         Action<TOptions, TDep1, TDep2, TDep3, TDep4> configureOptions)
//         where TDep1 : class
//         where TDep2 : class
//         where TDep3 : class
//         where TDep4 : class
//     {
//         if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));
//
//         Services.AddTransient<IPostConfigureOptions<TOptions>>(
//             sp => new PostConfigureOptions<TOptions, TDep1, TDep2, TDep3, TDep4>(
//                 Name,
//                 sp.GetRequiredService<TDep1>(),
//                 sp.GetRequiredService<TDep2>(),
//                 sp.GetRequiredService<TDep3>(),
//                 sp.GetRequiredService<TDep4>(),
//                 configureOptions));
//         return this;
//     }
//
//     /// <summary>
//     /// Registers an action used to post configure a particular type of options.
//     /// Note: These are run after all <seealso cref="Configure(Action{TOptions})"/>.
//     /// </summary>
//     /// <typeparam name="TDep1">The first dependency used by the action.</typeparam>
//     /// <typeparam name="TDep2">The second dependency used by the action.</typeparam>
//     /// <typeparam name="TDep3">The third dependency used by the action.</typeparam>
//     /// <typeparam name="TDep4">The fourth dependency used by the action.</typeparam>
//     /// <typeparam name="TDep5">The fifth dependency used by the action.</typeparam>
//     /// <param name="configureOptions">The action used to configure the options.</param>
//     /// <returns>The current <see cref="PerTenantOptionsBuilder{TOptions, TTenantInfo}"/>.</returns>
//     public virtual PerTenantOptionsBuilder<TOptions, TTenantInfo> PostConfigure<TDep1, TDep2, TDep3, TDep4, TDep5>(
//         Action<TOptions, TDep1, TDep2, TDep3, TDep4, TDep5> configureOptions)
//         where TDep1 : class
//         where TDep2 : class
//         where TDep3 : class
//         where TDep4 : class
//         where TDep5 : class
//     {
//         if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));
//
//         Services.AddTransient<IPostConfigureOptions<TOptions>>(
//             sp => new PostConfigureOptions<TOptions, TDep1, TDep2, TDep3, TDep4, TDep5>(
//                 Name,
//                 sp.GetRequiredService<TDep1>(),
//                 sp.GetRequiredService<TDep2>(),
//                 sp.GetRequiredService<TDep3>(),
//                 sp.GetRequiredService<TDep4>(),
//                 sp.GetRequiredService<TDep5>(),
//                 configureOptions));
//         return this;
//     }
//
//     /// <summary>
//     /// Register a validation action for an options type using a default failure message.
//     /// </summary>
//     /// <param name="validation">The validation function.</param>
//     /// <returns>The current <see cref="PerTenantOptionsBuilder{TOptions, TTenantInfo}"/>.</returns>
//     public virtual PerTenantOptionsBuilder<TOptions, TTenantInfo> Validate(Func<TOptions, bool> validation)
//         => Validate(validation: validation, failureMessage: DefaultValidationFailureMessage);
//
//     /// <summary>
//     /// Register a validation action for an options type.
//     /// </summary>
//     /// <param name="validation">The validation function.</param>
//     /// <param name="failureMessage">The failure message to use when validation fails.</param>
//     /// <returns>The current <see cref="PerTenantOptionsBuilder{TOptions, TTenantInfo}"/>.</returns>
//     public virtual PerTenantOptionsBuilder<TOptions, TTenantInfo> Validate(Func<TOptions, bool> validation,
//         string failureMessage)
//     {
//         if (validation == null) throw new ArgumentNullException(nameof(validation));
//
//         Services.AddSingleton<IValidateOptions<TOptions>>(
//             new ValidateOptions<TOptions>(Name, validation, failureMessage));
//         return this;
//     }
//
//     /// <summary>
//     /// Register a validation action for an options type using a default failure message.
//     /// </summary>
//     /// <typeparam name="TDep">The dependency used by the validation function.</typeparam>
//     /// <param name="validation">The validation function.</param>
//     /// <returns>The current <see cref="PerTenantOptionsBuilder{TOptions, TTenantInfo}"/>.</returns>
//     public virtual PerTenantOptionsBuilder<TOptions, TTenantInfo> Validate<TDep>(Func<TOptions, TDep, bool> validation)
//         where TDep : notnull
//         => Validate(validation: validation, failureMessage: DefaultValidationFailureMessage);
//
//     /// <summary>
//     /// Register a validation action for an options type.
//     /// </summary>
//     /// <typeparam name="TDep">The dependency used by the validation function.</typeparam>
//     /// <param name="validation">The validation function.</param>
//     /// <param name="failureMessage">The failure message to use when validation fails.</param>
//     /// <returns>The current <see cref="PerTenantOptionsBuilder{TOptions, TTenantInfo}"/>.</returns>
//     public virtual PerTenantOptionsBuilder<TOptions, TTenantInfo> Validate<TDep>(Func<TOptions, TDep, bool> validation,
//         string failureMessage)
//         where TDep : notnull
//     {
//         if (validation == null) throw new ArgumentNullException(nameof(validation));
//
//         Services.AddTransient<IValidateOptions<TOptions>>(sp =>
//             new ValidateOptions<TOptions, TDep>(Name, sp.GetRequiredService<TDep>(), validation, failureMessage));
//         return this;
//     }
//
//     /// <summary>
//     /// Register a validation action for an options type using a default failure message.
//     /// </summary>
//     /// <typeparam name="TDep1">The first dependency used by the validation function.</typeparam>
//     /// <typeparam name="TDep2">The second dependency used by the validation function.</typeparam>
//     /// <param name="validation">The validation function.</param>
//     /// <returns>The current <see cref="PerTenantOptionsBuilder{TOptions, TTenantInfo}"/>.</returns>
//     public virtual PerTenantOptionsBuilder<TOptions, TTenantInfo> Validate<TDep1, TDep2>(
//         Func<TOptions, TDep1, TDep2, bool> validation)
//         where TDep1 : notnull
//         where TDep2 : notnull
//         => Validate(validation: validation, failureMessage: DefaultValidationFailureMessage);
//
//     /// <summary>
//     /// Register a validation action for an options type.
//     /// </summary>
//     /// <typeparam name="TDep1">The first dependency used by the validation function.</typeparam>
//     /// <typeparam name="TDep2">The second dependency used by the validation function.</typeparam>
//     /// <param name="validation">The validation function.</param>
//     /// <param name="failureMessage">The failure message to use when validation fails.</param>
//     /// <returns>The current <see cref="PerTenantOptionsBuilder{TOptions, TTenantInfo}"/>.</returns>
//     public virtual PerTenantOptionsBuilder<TOptions, TTenantInfo> Validate<TDep1, TDep2>(
//         Func<TOptions, TDep1, TDep2, bool> validation,
//         string failureMessage)
//         where TDep1 : notnull
//         where TDep2 : notnull
//     {
//         if (validation == null) throw new ArgumentNullException(nameof(validation));
//
//
//         Services.AddTransient<IValidateOptions<TOptions>>(sp =>
//             new ValidateOptions<TOptions, TDep1, TDep2>(Name,
//                 sp.GetRequiredService<TDep1>(),
//                 sp.GetRequiredService<TDep2>(),
//                 validation,
//                 failureMessage));
//         return this;
//     }
//
//     /// <summary>
//     /// Register a validation action for an options type using a default failure message.
//     /// </summary>
//     /// <typeparam name="TDep1">The first dependency used by the validation function.</typeparam>
//     /// <typeparam name="TDep2">The second dependency used by the validation function.</typeparam>
//     /// <typeparam name="TDep3">The third dependency used by the validation function.</typeparam>
//     /// <param name="validation">The validation function.</param>
//     /// <returns>The current <see cref="PerTenantOptionsBuilder{TOptions, TTenantInfo}"/>.</returns>
//     public virtual PerTenantOptionsBuilder<TOptions, TTenantInfo> Validate<TDep1, TDep2, TDep3>(
//         Func<TOptions, TDep1, TDep2, TDep3, bool> validation)
//         where TDep1 : notnull
//         where TDep2 : notnull
//         where TDep3 : notnull
//         => Validate(validation: validation, failureMessage: DefaultValidationFailureMessage);
//
//     /// <summary>
//     /// Register a validation action for an options type.
//     /// </summary>
//     /// <typeparam name="TDep1">The first dependency used by the validation function.</typeparam>
//     /// <typeparam name="TDep2">The second dependency used by the validation function.</typeparam>
//     /// <typeparam name="TDep3">The third dependency used by the validation function.</typeparam>
//     /// <param name="validation">The validation function.</param>
//     /// <param name="failureMessage">The failure message to use when validation fails.</param>
//     /// <returns>The current <see cref="PerTenantOptionsBuilder{TOptions, TTenantInfo}"/>.</returns>
//     public virtual PerTenantOptionsBuilder<TOptions, TTenantInfo> Validate<TDep1, TDep2, TDep3>(
//         Func<TOptions, TDep1, TDep2, TDep3, bool> validation, string failureMessage)
//         where TDep1 : notnull
//         where TDep2 : notnull
//         where TDep3 : notnull
//     {
//         if (validation == null) throw new ArgumentNullException(nameof(validation));
//
//         Services.AddTransient<IValidateOptions<TOptions>>(sp =>
//             new ValidateOptions<TOptions, TDep1, TDep2, TDep3>(Name,
//                 sp.GetRequiredService<TDep1>(),
//                 sp.GetRequiredService<TDep2>(),
//                 sp.GetRequiredService<TDep3>(),
//                 validation,
//                 failureMessage));
//         return this;
//     }
//
//     /// <summary>
//     /// Register a validation action for an options type using a default failure message.
//     /// </summary>
//     /// <typeparam name="TDep1">The first dependency used by the validation function.</typeparam>
//     /// <typeparam name="TDep2">The second dependency used by the validation function.</typeparam>
//     /// <typeparam name="TDep3">The third dependency used by the validation function.</typeparam>
//     /// <typeparam name="TDep4">The fourth dependency used by the validation function.</typeparam>
//     /// <param name="validation">The validation function.</param>
//     /// <returns>The current <see cref="PerTenantOptionsBuilder{TOptions, TTenantInfo}"/>.</returns>
//     public virtual PerTenantOptionsBuilder<TOptions, TTenantInfo> Validate<TDep1, TDep2, TDep3, TDep4>(
//         Func<TOptions, TDep1, TDep2, TDep3, TDep4, bool> validation)
//         where TDep1 : notnull
//         where TDep2 : notnull
//         where TDep3 : notnull
//         where TDep4 : notnull
//         => Validate(validation: validation, failureMessage: DefaultValidationFailureMessage);
//
//     /// <summary>
//     /// Register a validation action for an options type.
//     /// </summary>
//     /// <typeparam name="TDep1">The first dependency used by the validation function.</typeparam>
//     /// <typeparam name="TDep2">The second dependency used by the validation function.</typeparam>
//     /// <typeparam name="TDep3">The third dependency used by the validation function.</typeparam>
//     /// <typeparam name="TDep4">The fourth dependency used by the validation function.</typeparam>
//     /// <param name="validation">The validation function.</param>
//     /// <param name="failureMessage">The failure message to use when validation fails.</param>
//     /// <returns>The current <see cref="PerTenantOptionsBuilder{TOptions, TTenantInfo}"/>.</returns>
//     public virtual PerTenantOptionsBuilder<TOptions, TTenantInfo> Validate<TDep1, TDep2, TDep3, TDep4>(
//         Func<TOptions, TDep1, TDep2, TDep3, TDep4, bool> validation, string failureMessage)
//         where TDep1 : notnull
//         where TDep2 : notnull
//         where TDep3 : notnull
//         where TDep4 : notnull
//     {
//         if (validation == null) throw new ArgumentNullException(nameof(validation));
//
//         Services.AddTransient<IValidateOptions<TOptions>>(sp =>
//             new ValidateOptions<TOptions, TDep1, TDep2, TDep3, TDep4>(Name,
//                 sp.GetRequiredService<TDep1>(),
//                 sp.GetRequiredService<TDep2>(),
//                 sp.GetRequiredService<TDep3>(),
//                 sp.GetRequiredService<TDep4>(),
//                 validation,
//                 failureMessage));
//         return this;
//     }
//
//     /// <summary>
//     /// Register a validation action for an options type using a default failure message.
//     /// </summary>
//     /// <typeparam name="TDep1">The first dependency used by the validation function.</typeparam>
//     /// <typeparam name="TDep2">The second dependency used by the validation function.</typeparam>
//     /// <typeparam name="TDep3">The third dependency used by the validation function.</typeparam>
//     /// <typeparam name="TDep4">The fourth dependency used by the validation function.</typeparam>
//     /// <typeparam name="TDep5">The fifth dependency used by the validation function.</typeparam>
//     /// <param name="validation">The validation function.</param>
//     /// <returns>The current <see cref="PerTenantOptionsBuilder{TOptions, TTenantInfo}"/>.</returns>
//     public virtual PerTenantOptionsBuilder<TOptions, TTenantInfo> Validate<TDep1, TDep2, TDep3, TDep4, TDep5>(
//         Func<TOptions, TDep1, TDep2, TDep3, TDep4, TDep5, bool> validation)
//         where TDep1 : notnull
//         where TDep2 : notnull
//         where TDep3 : notnull
//         where TDep4 : notnull
//         where TDep5 : notnull
//         => Validate(validation: validation, failureMessage: DefaultValidationFailureMessage);
//
//     /// <summary>
//     /// Register a validation action for an options type.
//     /// </summary>
//     /// <typeparam name="TDep1">The first dependency used by the validation function.</typeparam>
//     /// <typeparam name="TDep2">The second dependency used by the validation function.</typeparam>
//     /// <typeparam name="TDep3">The third dependency used by the validation function.</typeparam>
//     /// <typeparam name="TDep4">The fourth dependency used by the validation function.</typeparam>
//     /// <typeparam name="TDep5">The fifth dependency used by the validation function.</typeparam>
//     /// <param name="validation">The validation function.</param>
//     /// <param name="failureMessage">The failure message to use when validation fails.</param>
//     /// <returns>The current <see cref="PerTenantOptionsBuilder{TOptions, TTenantInfo}"/>.</returns>
//     public virtual PerTenantOptionsBuilder<TOptions, TTenantInfo> Validate<TDep1, TDep2, TDep3, TDep4, TDep5>(
//         Func<TOptions, TDep1, TDep2, TDep3, TDep4, TDep5, bool> validation, string failureMessage)
//         where TDep1 : notnull
//         where TDep2 : notnull
//         where TDep3 : notnull
//         where TDep4 : notnull
//         where TDep5 : notnull
//     {
//         if (validation == null) throw new ArgumentNullException(nameof(validation));
//
//         Services.AddTransient<IValidateOptions<TOptions>>(sp =>
//             new ValidateOptions<TOptions, TDep1, TDep2, TDep3, TDep4, TDep5>(Name,
//                 sp.GetRequiredService<TDep1>(),
//                 sp.GetRequiredService<TDep2>(),
//                 sp.GetRequiredService<TDep3>(),
//                 sp.GetRequiredService<TDep4>(),
//                 sp.GetRequiredService<TDep5>(),
//                 validation,
//                 failureMessage));
//         return this;
//     }
// }