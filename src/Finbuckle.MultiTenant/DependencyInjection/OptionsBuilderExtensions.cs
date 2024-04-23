// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

//    Portions of this file are derived from the .NET Foundation source file located at:
//    https://github.com/dotnet/runtime/blob/main/src/libraries/Microsoft.Extensions.Options/src/OptionsBuilder.cs

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

// ReSharper disable once CheckNamespace
namespace Finbuckle.MultiTenant;

/// <summary>
/// This static class provides extension methods for the OptionsBuilder class.
/// These methods allow for the configuration and post-configuration of options on a per-tenant basis.
/// Additional dependencies can also be included in the configuration.
/// </summary>
public static class OptionsBuilderExtensions
{
    /// <summary>
    /// Configures the options for a specific tenant.
    /// </summary>
    /// <typeparam name="TOptions">The type of options being configured.</typeparam>
    /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
    /// <param name="optionsBuilder">The options builder.</param>
    /// <param name="configureOptions">The action used to configure the options.</param>
    /// <returns>The same options builder so that multiple calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the configureOptions parameter is null.</exception>
    public static OptionsBuilder<TOptions> ConfigurePerTenant<TOptions, TTenantInfo>(
        this OptionsBuilder<TOptions> optionsBuilder, Action<TOptions, TTenantInfo> configureOptions)
        where TOptions : class
        where TTenantInfo : class, ITenantInfo, new()
    {
        if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

        FinbuckleServiceCollectionExtensions.ConfigurePerTenantReqs<TOptions>(optionsBuilder.Services);

        optionsBuilder.Services.AddTransient<IConfigureOptions<TOptions>>(sp =>
            new ConfigureNamedOptions<TOptions, IMultiTenantContextAccessor<TTenantInfo>>(
                optionsBuilder.Name,
                sp.GetRequiredService<IMultiTenantContextAccessor<TTenantInfo>>(),
                (options, mtcAccessor) =>
                {
                    var tenantInfo = mtcAccessor.MultiTenantContext.TenantInfo;
                    if (tenantInfo is not null)
                        configureOptions(options, tenantInfo);
                }));

        return optionsBuilder;
    }

    /// <summary>
    /// Configures the options for a specific tenant with an additional dependency.
    /// </summary>
    /// <typeparam name="TOptions">The type of options being configured.</typeparam>
    /// <typeparam name="TDep">The type of the additional dependency.</typeparam>
    /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
    /// <param name="optionsBuilder">The options builder.</param>
    /// <param name="configureOptions">The action used to configure the options.</param>
    /// <returns>The same options builder so that multiple calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the configureOptions parameter is null.</exception>
    public static OptionsBuilder<TOptions> ConfigurePerTenant<TOptions, TDep, TTenantInfo>(
        this OptionsBuilder<TOptions> optionsBuilder, Action<TOptions, TDep, TTenantInfo> configureOptions)
        where TOptions : class
        where TDep : class
        where TTenantInfo : class, ITenantInfo, new()
    {
        if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

        FinbuckleServiceCollectionExtensions.ConfigurePerTenantReqs<TOptions>(optionsBuilder.Services);

        optionsBuilder.Services.AddTransient<IConfigureOptions<TOptions>>(sp =>
            new ConfigureNamedOptions<TOptions, TDep, IMultiTenantContextAccessor<TTenantInfo>>(
                optionsBuilder.Name,
                sp.GetRequiredService<TDep>(),
                sp.GetRequiredService<IMultiTenantContextAccessor<TTenantInfo>>(),
                (options, dep, mtcAccessor) =>
                {
                    var tenantInfo = mtcAccessor.MultiTenantContext.TenantInfo;
                    if (tenantInfo is not null)
                        configureOptions(options, dep, tenantInfo);
                }));

        return optionsBuilder;
    }
    
    /// <summary>
    /// Configures the options for a specific tenant with two additional dependencies.
    /// </summary>
    /// <typeparam name="TOptions">The type of options being configured.</typeparam>
    /// <typeparam name="TDep1">The type of the first additional dependency.</typeparam>
    /// <typeparam name="TDep2">The type of the second additional dependency.</typeparam>
    /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
    /// <param name="optionsBuilder">The options builder.</param>
    /// <param name="configureOptions">The action used to configure the options.</param>
    /// <returns>The same options builder so that multiple calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the configureOptions parameter is null.</exception>
    public static OptionsBuilder<TOptions> ConfigurePerTenant<TOptions, TDep1, TDep2, TTenantInfo>(
        this OptionsBuilder<TOptions> optionsBuilder, Action<TOptions, TDep1, TDep2, TTenantInfo> configureOptions)
        where TOptions : class
        where TDep1 : class
        where TDep2 : class
        where TTenantInfo : class, ITenantInfo, new()
    {
        if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

        FinbuckleServiceCollectionExtensions.ConfigurePerTenantReqs<TOptions>(optionsBuilder.Services);

        optionsBuilder.Services.AddTransient<IConfigureOptions<TOptions>>(sp =>
            new ConfigureNamedOptions<TOptions, TDep1, TDep2, IMultiTenantContextAccessor<TTenantInfo>>(
                optionsBuilder.Name,
                sp.GetRequiredService<TDep1>(),
                sp.GetRequiredService<TDep2>(),
                sp.GetRequiredService<IMultiTenantContextAccessor<TTenantInfo>>(),
                (options, dep1, dep2, mtcAccessor) =>
                {
                    var tenantInfo = mtcAccessor.MultiTenantContext.TenantInfo;
                    if (tenantInfo is not null)
                        configureOptions(options, dep1, dep2, tenantInfo);
                }));

        return optionsBuilder;
    }

    /// <summary>
    /// Configures the options for a specific tenant with two additional dependencies.
    /// </summary>
    /// <typeparam name="TOptions">The type of options being configured.</typeparam>
    /// <typeparam name="TDep1">The type of the first additional dependency.</typeparam>
    /// <typeparam name="TDep2">The type of the second additional dependency.</typeparam>
    /// <typeparam name="TDep3">The type of the third additional dependency.</typeparam>
    /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
    /// <param name="optionsBuilder">The options builder.</param>
    /// <param name="configureOptions">The action used to configure the options.</param>
    /// <returns>The same options builder so that multiple calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the configureOptions parameter is null.</exception>
    public static OptionsBuilder<TOptions> ConfigurePerTenant<TOptions, TDep1, TDep2, TDep3, TTenantInfo>(
        this OptionsBuilder<TOptions> optionsBuilder,
        Action<TOptions, TDep1, TDep2, TDep3, TTenantInfo> configureOptions)
        where TOptions : class
        where TDep1 : class
        where TDep2 : class
        where TDep3 : class
        where TTenantInfo : class, ITenantInfo, new()
    {
        if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

        FinbuckleServiceCollectionExtensions.ConfigurePerTenantReqs<TOptions>(optionsBuilder.Services);

        optionsBuilder.Services.AddTransient<IConfigureOptions<TOptions>>(sp =>
            new ConfigureNamedOptions<TOptions, TDep1, TDep2, TDep3, IMultiTenantContextAccessor<TTenantInfo>>(
                optionsBuilder.Name,
                sp.GetRequiredService<TDep1>(),
                sp.GetRequiredService<TDep2>(),
                sp.GetRequiredService<TDep3>(),
                sp.GetRequiredService<IMultiTenantContextAccessor<TTenantInfo>>(),
                (options, dep1, dep2, dep3, mtcAccessor) =>
                {
                    var tenantInfo = mtcAccessor.MultiTenantContext.TenantInfo;
                    if (tenantInfo is not null)
                        configureOptions(options, dep1, dep2, dep3, tenantInfo);
                }));

        return optionsBuilder;
    }

    /// <summary>
    /// Configures the options for a specific tenant with two additional dependencies.
    /// </summary>
    /// <typeparam name="TOptions">The type of options being configured.</typeparam>
    /// <typeparam name="TDep1">The type of the first additional dependency.</typeparam>
    /// <typeparam name="TDep2">The type of the second additional dependency.</typeparam>
    /// <typeparam name="TDep3">The type of the third additional dependency.</typeparam>
    /// <typeparam name="TDep4">The type of the fourth additional dependency.</typeparam>
    /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
    /// <param name="optionsBuilder">The options builder.</param>
    /// <param name="configureOptions">The action used to configure the options.</param>
    /// <returns>The same options builder so that multiple calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the configureOptions parameter is null.</exception>
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
        if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

        FinbuckleServiceCollectionExtensions.ConfigurePerTenantReqs<TOptions>(optionsBuilder.Services);

        optionsBuilder.Services.AddTransient<IConfigureOptions<TOptions>>(sp =>
            new ConfigureNamedOptions<TOptions, TDep1, TDep2, TDep3, TDep4, IMultiTenantContextAccessor<TTenantInfo>>(
                optionsBuilder.Name,
                sp.GetRequiredService<TDep1>(),
                sp.GetRequiredService<TDep2>(),
                sp.GetRequiredService<TDep3>(),
                sp.GetRequiredService<TDep4>(),
                sp.GetRequiredService<IMultiTenantContextAccessor<TTenantInfo>>(),
                (options, dep1, dep2, dep3, dep4, mtcAccessor) =>
                {
                    var tenantInfo = mtcAccessor.MultiTenantContext.TenantInfo;
                    if (tenantInfo is not null)
                        configureOptions(options, dep1, dep2, dep3, dep4, tenantInfo);
                }));

        return optionsBuilder;
    }

    // Experimental
    // public static OptionsBuilder<TOptions> ConfigurePerTenant<TOptions, TDep1, TDep2, TDep3, TDep4, TDep5, TTenantInfo>(
    //     this OptionsBuilder<TOptions> optionsBuilder,
    //     Action<TOptions, TDep1, TDep2, TDep3, TDep4, TDep5, TTenantInfo> configureOptions)
    //     where TOptions : class
    //     where TDep1 : class
    //     where TDep2 : class
    //     where TDep3 : class
    //     where TDep4 : class
    //     where TDep5 : class
    //     where TTenantInfo : class, ITenantInfo, new()
    // {
    //     if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));
    //
    //     FinbuckleServiceCollectionExtensions.ConfigurePerTenantReqs<TOptions>(optionsBuilder.Services);
    //
    //     optionsBuilder.Services.AddTransient<IConfigureOptions<TOptions>>(sp =>
    //     {
    //         var mtcAccessor = sp.GetRequiredService<IMultiTenantContextAccessor<TTenantInfo>>();
    //         return new ConfigureNamedOptions<TOptions, TDep1, TDep2, TDep3, TDep4, TDep5>(
    //             optionsBuilder.Name,
    //             sp.GetRequiredService<TDep1>(),
    //             sp.GetRequiredService<TDep2>(),
    //             sp.GetRequiredService<TDep3>(),
    //             sp.GetRequiredService<TDep4>(),
    //             sp.GetRequiredService<TDep5>(),
    //             (options, dep1, dep2, dep3, dep4, dep5) =>
    //             {
    //                 var tenantInfo = mtcAccessor.MultiTenantContext.TenantInfo;
    //                 if (tenantInfo is not null)
    //                     configureOptions(options, dep1, dep2, dep3, dep4, dep5, tenantInfo);
    //             });
    //     });
    //
    //     return optionsBuilder;
    // }

    /// <summary>
    /// Post-configures the options for a specific tenant.
    /// </summary>
    /// <typeparam name="TOptions">The type of options being post-configured.</typeparam>
    /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
    /// <param name="optionsBuilder">The options builder.</param>
    /// <param name="configureOptions">The action used to post-configure the options.</param>
    /// <returns>The same options builder so that multiple calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the configureOptions parameter is null.</exception>
    public static OptionsBuilder<TOptions> PostConfigurePerTenant<TOptions, TTenantInfo>(
        this OptionsBuilder<TOptions> optionsBuilder, Action<TOptions, TTenantInfo> configureOptions)
        where TOptions : class
        where TTenantInfo : class, ITenantInfo, new()
    {
        if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

        FinbuckleServiceCollectionExtensions.ConfigurePerTenantReqs<TOptions>(optionsBuilder.Services);

        optionsBuilder.Services.AddTransient<IPostConfigureOptions<TOptions>>(sp =>
            new PostConfigureOptions<TOptions, IMultiTenantContextAccessor<TTenantInfo>>(
                optionsBuilder.Name,
                sp.GetRequiredService<IMultiTenantContextAccessor<TTenantInfo>>(),
                (options, mtcAccessor) =>
                {
                    var tenantInfo = mtcAccessor.MultiTenantContext.TenantInfo;
                    if (tenantInfo is not null)
                        configureOptions(options, tenantInfo);
                }));

        return optionsBuilder;
    }

    /// <summary>
    /// Post-configures the options for a specific tenant.
    /// </summary>
    /// <typeparam name="TOptions">The type of options being post-configured.</typeparam>
    /// <typeparam name="TDep">The type of the additional dependency.</typeparam>
    /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
    /// <param name="optionsBuilder">The options builder.</param>
    /// <param name="configureOptions">The action used to post-configure the options.</param>
    /// <returns>The same options builder so that multiple calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the configureOptions parameter is null.</exception>
    public static OptionsBuilder<TOptions> PostConfigurePerTenant<TOptions, TDep, TTenantInfo>(
        this OptionsBuilder<TOptions> optionsBuilder, Action<TOptions, TDep, TTenantInfo> configureOptions)
        where TOptions : class
        where TDep : class
        where TTenantInfo : class, ITenantInfo, new()
    {
        if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

        FinbuckleServiceCollectionExtensions.ConfigurePerTenantReqs<TOptions>(optionsBuilder.Services);

        optionsBuilder.Services.AddTransient<IPostConfigureOptions<TOptions>>(sp =>
            new PostConfigureOptions<TOptions, TDep, IMultiTenantContextAccessor<TTenantInfo>>(
                optionsBuilder.Name,
                sp.GetRequiredService<TDep>(),
                sp.GetRequiredService<IMultiTenantContextAccessor<TTenantInfo>>(),
                (options, dep, mtcAccessor) =>
                {
                    var tenantInfo = mtcAccessor.MultiTenantContext.TenantInfo;
                    if (tenantInfo is not null)
                        configureOptions(options, dep, tenantInfo);
                }));

        return optionsBuilder;
    }

    /// <summary>
    /// Post-configures the options for a specific tenant.
    /// </summary>
    /// <typeparam name="TOptions">The type of options being post-configured.</typeparam>
    /// <typeparam name="TDep1">The type of the first additional dependency.</typeparam>
    /// <typeparam name="TDep2">The type of the second additional dependency.</typeparam>
    /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
    /// <param name="optionsBuilder">The options builder.</param>
    /// <param name="configureOptions">The action used to post-configure the options.</param>
    /// <returns>The same options builder so that multiple calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the configureOptions parameter is null.</exception>
    public static OptionsBuilder<TOptions> PostConfigurePerTenant<TOptions, TDep1, TDep2, TTenantInfo>(
        this OptionsBuilder<TOptions> optionsBuilder, Action<TOptions, TDep1, TDep2, TTenantInfo> configureOptions)
        where TOptions : class
        where TDep1 : class
        where TDep2 : class
        where TTenantInfo : class, ITenantInfo, new()
    {
        if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

        FinbuckleServiceCollectionExtensions.ConfigurePerTenantReqs<TOptions>(optionsBuilder.Services);

        optionsBuilder.Services.AddTransient<IPostConfigureOptions<TOptions>>(sp =>
            new PostConfigureOptions<TOptions, TDep1, TDep2, IMultiTenantContextAccessor<TTenantInfo>>(
                optionsBuilder.Name,
                sp.GetRequiredService<TDep1>(),
                sp.GetRequiredService<TDep2>(),
                sp.GetRequiredService<IMultiTenantContextAccessor<TTenantInfo>>(),
                (options, dep1, dep2, mtcAccessor) =>
                {
                    var tenantInfo = mtcAccessor.MultiTenantContext.TenantInfo;
                    if (tenantInfo is not null)
                        configureOptions(options, dep1, dep2, tenantInfo);
                }));

        return optionsBuilder;
    }

    /// <summary>
    /// Post-configures the options for a specific tenant.
    /// </summary>
    /// <typeparam name="TOptions">The type of options being post-configured.</typeparam>
    /// <typeparam name="TDep1">The type of the first additional dependency.</typeparam>
    /// <typeparam name="TDep2">The type of the second additional dependency.</typeparam>
    /// <typeparam name="TDep3">The type of the third additional dependency.</typeparam>
    /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
    /// <param name="optionsBuilder">The options builder.</param>
    /// <param name="configureOptions">The action used to post-configure the options.</param>
    /// <returns>The same options builder so that multiple calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the configureOptions parameter is null.</exception>
    public static OptionsBuilder<TOptions> PostConfigurePerTenant<TOptions, TDep1, TDep2, TDep3, TTenantInfo>(
        this OptionsBuilder<TOptions> optionsBuilder,
        Action<TOptions, TDep1, TDep2, TDep3, TTenantInfo> configureOptions)
        where TOptions : class
        where TDep1 : class
        where TDep2 : class
        where TDep3 : class
        where TTenantInfo : class, ITenantInfo, new()
    {
        if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

        FinbuckleServiceCollectionExtensions.ConfigurePerTenantReqs<TOptions>(optionsBuilder.Services);

        optionsBuilder.Services.AddTransient<IPostConfigureOptions<TOptions>>(sp =>
            new PostConfigureOptions<TOptions, TDep1, TDep2, TDep3, IMultiTenantContextAccessor<TTenantInfo>>(
                optionsBuilder.Name,
                sp.GetRequiredService<TDep1>(),
                sp.GetRequiredService<TDep2>(),
                sp.GetRequiredService<TDep3>(),
                sp.GetRequiredService<IMultiTenantContextAccessor<TTenantInfo>>(),
                (options, dep1, dep2, dep3, mtcAccessor) =>
                {
                    var tenantInfo = mtcAccessor.MultiTenantContext.TenantInfo;
                    if (tenantInfo is not null)
                        configureOptions(options, dep1, dep2, dep3, tenantInfo);
                }));

        return optionsBuilder;
    }

    /// <summary>
    /// Post-configures the options for a specific tenant.
    /// </summary>
    /// <typeparam name="TOptions">The type of options being post-configured.</typeparam>
    /// <typeparam name="TDep1">The type of the first additional dependency.</typeparam>
    /// <typeparam name="TDep2">The type of the second additional dependency.</typeparam>
    /// <typeparam name="TDep3">The type of the third additional dependency.</typeparam>
    /// <typeparam name="TDep4">The type of the fourth additional dependency.</typeparam>
    /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
    /// <param name="optionsBuilder">The options builder.</param>
    /// <param name="configureOptions">The action used to post-configure the options.</param>
    /// <returns>The same options builder so that multiple calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the configureOptions parameter is null.</exception>
    public static OptionsBuilder<TOptions> PostConfigurePerTenant<TOptions, TDep1, TDep2, TDep3, TDep4, TTenantInfo>(
        this OptionsBuilder<TOptions> optionsBuilder,
        Action<TOptions, TDep1, TDep2, TDep3, TDep4, TTenantInfo> configureOptions)
        where TOptions : class
        where TDep1 : class
        where TDep2 : class
        where TDep3 : class
        where TDep4 : class
        where TTenantInfo : class, ITenantInfo, new()
    {
        if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

        FinbuckleServiceCollectionExtensions.ConfigurePerTenantReqs<TOptions>(optionsBuilder.Services);

        optionsBuilder.Services.AddTransient<IPostConfigureOptions<TOptions>>(sp =>
            new PostConfigureOptions<TOptions, TDep1, TDep2, TDep3, TDep4, IMultiTenantContextAccessor<TTenantInfo>>(
                optionsBuilder.Name,
                sp.GetRequiredService<TDep1>(),
                sp.GetRequiredService<TDep2>(),
                sp.GetRequiredService<TDep3>(),
                sp.GetRequiredService<TDep4>(),
                sp.GetRequiredService<IMultiTenantContextAccessor<TTenantInfo>>(),
                (options, dep1, dep2, dep3, dep4, mtcAccessor) =>
                {
                    var tenantInfo = mtcAccessor.MultiTenantContext.TenantInfo;
                    if (tenantInfo is not null)
                        configureOptions(options, dep1, dep2, dep3, dep4, tenantInfo);
                }));

        return optionsBuilder;
    }

    // Experimental
    // public static OptionsBuilder<TOptions> PostConfigurePerTenant<TOptions, TDep1, TDep2, TDep3, TDep4, TDep5,
    //     TTenantInfo>(
    //     this OptionsBuilder<TOptions> optionsBuilder,
    //     Action<TOptions, TDep1, TDep2, TDep3, TDep4, TDep5, TTenantInfo> configureOptions)
    //     where TOptions : class
    //     where TDep1 : class
    //     where TDep2 : class
    //     where TDep3 : class
    //     where TDep4 : class
    //     where TDep5 : class
    //     where TTenantInfo : class, ITenantInfo, new()
    // {
    //     if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));
    //
    //     FinbuckleServiceCollectionExtensions.ConfigurePerTenantReqs<TOptions>(optionsBuilder.Services);
    //
    //     optionsBuilder.Services.AddTransient<IPostConfigureOptions<TOptions>>(sp =>
    //     {
    //         var mtcAccessor = sp.GetRequiredService<IMultiTenantContextAccessor<TTenantInfo>>();
    //         return new PostConfigureOptions<TOptions, TDep1, TDep2, TDep3, TDep4, TDep5>(
    //             optionsBuilder.Name,
    //             sp.GetRequiredService<TDep1>(),
    //             sp.GetRequiredService<TDep2>(),
    //             sp.GetRequiredService<TDep3>(),
    //             sp.GetRequiredService<TDep4>(),
    //             sp.GetRequiredService<TDep5>(),
    //             (options, dep1, dep2, dep3, dep4, dep5) =>
    //             {
    //                 var tenantInfo = mtcAccessor.MultiTenantContext.TenantInfo;
    //                 if (tenantInfo is not null)
    //                     configureOptions(options, dep1, dep2, dep3, dep4, dep5, tenantInfo);
    //             });
    //     });
    //
    //     return optionsBuilder;
    // }
}