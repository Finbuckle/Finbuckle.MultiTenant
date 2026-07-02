// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Finbuckle.MultiTenant.Extensions;

/// <summary>
/// Extension methods for configuring options per tenant.
/// </summary>
public static class OptionsBuilderExtensions
{
    /// <summary>
    /// Registers an action used to configure an options type per tenant.
    /// </summary>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    /// <typeparam name="TTenantInfo">The tenant info type.</typeparam>
    /// <param name="optionsBuilder">The options builder instance.</param>
    /// <param name="configureOptions">The action used to configure the options for each tenant.</param>
    /// <returns>The options builder for chaining.</returns>
    public static OptionsBuilder<TOptions> ConfigurePerTenant<TOptions, TTenantInfo>(
        this OptionsBuilder<TOptions> optionsBuilder, Action<TOptions, TTenantInfo> configureOptions)
        where TOptions : class
        where TTenantInfo : ITenantInfo
    {
        ArgumentNullException.ThrowIfNull(configureOptions);

        ServiceCollectionExtensions.ConfigurePerTenantReqs<TOptions>(optionsBuilder.Services);

        optionsBuilder.Services.AddTransient<IConfigureOptions<TOptions>>(sp =>
            new ConfigureNamedOptions<TOptions, ITenantContext<TTenantInfo>>(
                optionsBuilder.Name,
                sp.GetRequiredService<ITenantContext<TTenantInfo>>(),
                (options, tenantContext) =>
                {
                    var tenantInfo = tenantContext.TenantInfo;
                    if (tenantInfo is not null)
                        configureOptions(options, tenantInfo);
                }));

        return optionsBuilder;
    }

    /// <summary>
    /// Registers an action used to configure an options type per tenant with one dependency.
    /// </summary>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    /// <typeparam name="TDep">The dependency type.</typeparam>
    /// <typeparam name="TTenantInfo">The tenant info type.</typeparam>
    /// <param name="optionsBuilder">The options builder instance.</param>
    /// <param name="configureOptions">The action used to configure the options for each tenant.</param>
    /// <returns>The options builder for chaining.</returns>
    public static OptionsBuilder<TOptions> ConfigurePerTenant<TOptions, TDep, TTenantInfo>(
        this OptionsBuilder<TOptions> optionsBuilder, Action<TOptions, TDep, TTenantInfo> configureOptions)
        where TOptions : class
        where TDep : class
        where TTenantInfo : ITenantInfo
    {
        ArgumentNullException.ThrowIfNull(configureOptions);

        ServiceCollectionExtensions.ConfigurePerTenantReqs<TOptions>(optionsBuilder.Services);

        optionsBuilder.Services.AddTransient<IConfigureOptions<TOptions>>(sp =>
            new ConfigureNamedOptions<TOptions, TDep, ITenantContext<TTenantInfo>>(
                optionsBuilder.Name,
                sp.GetRequiredService<TDep>(),
                sp.GetRequiredService<ITenantContext<TTenantInfo>>(),
                (options, dep, tenantContext) =>
                {
                    var tenantInfo = tenantContext.TenantInfo;
                    if (tenantInfo is not null)
                        configureOptions(options, dep, tenantInfo);
                }));

        return optionsBuilder;
    }

    /// <summary>
    /// Registers an action used to configure an options type per tenant with two dependencies.
    /// </summary>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    /// <typeparam name="TDep1">The first dependency type.</typeparam>
    /// <typeparam name="TDep2">The second dependency type.</typeparam>
    /// <typeparam name="TTenantInfo">The tenant info type.</typeparam>
    /// <param name="optionsBuilder">The options builder instance.</param>
    /// <param name="configureOptions">The action used to configure the options for each tenant.</param>
    /// <returns>The options builder for chaining.</returns>
    public static OptionsBuilder<TOptions> ConfigurePerTenant<TOptions, TDep1, TDep2, TTenantInfo>(
        this OptionsBuilder<TOptions> optionsBuilder, Action<TOptions, TDep1, TDep2, TTenantInfo> configureOptions)
        where TOptions : class
        where TDep1 : class
        where TDep2 : class
        where TTenantInfo : ITenantInfo
    {
        ArgumentNullException.ThrowIfNull(configureOptions);

        ServiceCollectionExtensions.ConfigurePerTenantReqs<TOptions>(optionsBuilder.Services);

        optionsBuilder.Services.AddTransient<IConfigureOptions<TOptions>>(sp =>
            new ConfigureNamedOptions<TOptions, TDep1, TDep2, ITenantContext<TTenantInfo>>(
                optionsBuilder.Name,
                sp.GetRequiredService<TDep1>(),
                sp.GetRequiredService<TDep2>(),
                sp.GetRequiredService<ITenantContext<TTenantInfo>>(),
                (options, dep1, dep2, tenantContext) =>
                {
                    var tenantInfo = tenantContext.TenantInfo;
                    if (tenantInfo is not null)
                        configureOptions(options, dep1, dep2, tenantInfo);
                }));

        return optionsBuilder;
    }

    /// <summary>
    /// Registers an action used to configure an options type per tenant with three dependencies.
    /// </summary>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    /// <typeparam name="TDep1">The first dependency type.</typeparam>
    /// <typeparam name="TDep2">The second dependency type.</typeparam>
    /// <typeparam name="TDep3">The third dependency type.</typeparam>
    /// <typeparam name="TTenantInfo">The tenant info type.</typeparam>
    /// <param name="optionsBuilder">The options builder instance.</param>
    /// <param name="configureOptions">The action used to configure the options for each tenant.</param>
    /// <returns>The options builder for chaining.</returns>
    public static OptionsBuilder<TOptions> ConfigurePerTenant<TOptions, TDep1, TDep2, TDep3, TTenantInfo>(
        this OptionsBuilder<TOptions> optionsBuilder,
        Action<TOptions, TDep1, TDep2, TDep3, TTenantInfo> configureOptions)
        where TOptions : class
        where TDep1 : class
        where TDep2 : class
        where TDep3 : class
        where TTenantInfo : ITenantInfo
    {
        ArgumentNullException.ThrowIfNull(configureOptions);

        ServiceCollectionExtensions.ConfigurePerTenantReqs<TOptions>(optionsBuilder.Services);

        optionsBuilder.Services.AddTransient<IConfigureOptions<TOptions>>(sp =>
            new ConfigureNamedOptions<TOptions, TDep1, TDep2, TDep3, ITenantContext<TTenantInfo>>(
                optionsBuilder.Name,
                sp.GetRequiredService<TDep1>(),
                sp.GetRequiredService<TDep2>(),
                sp.GetRequiredService<TDep3>(),
                sp.GetRequiredService<ITenantContext<TTenantInfo>>(),
                (options, dep1, dep2, dep3, tenantContext) =>
                {
                    var tenantInfo = tenantContext.TenantInfo;
                    if (tenantInfo is not null)
                        configureOptions(options, dep1, dep2, dep3, tenantInfo);
                }));

        return optionsBuilder;
    }

    /// <summary>
    /// Registers an action used to configure an options type per tenant with four dependencies.
    /// </summary>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    /// <typeparam name="TDep1">The first dependency type.</typeparam>
    /// <typeparam name="TDep2">The second dependency type.</typeparam>
    /// <typeparam name="TDep3">The third dependency type.</typeparam>
    /// <typeparam name="TDep4">The fourth dependency type.</typeparam>
    /// <typeparam name="TTenantInfo">The tenant info type.</typeparam>
    /// <param name="optionsBuilder">The options builder instance.</param>
    /// <param name="configureOptions">The action used to configure the options for each tenant.</param>
    /// <returns>The options builder for chaining.</returns>
    public static OptionsBuilder<TOptions> ConfigurePerTenant<TOptions, TDep1, TDep2, TDep3, TDep4, TTenantInfo>(
        this OptionsBuilder<TOptions> optionsBuilder,
        Action<TOptions, TDep1, TDep2, TDep3, TDep4, TTenantInfo> configureOptions)
        where TOptions : class
        where TDep1 : class
        where TDep2 : class
        where TDep3 : class
        where TDep4 : class
        where TTenantInfo : ITenantInfo
    {
        ArgumentNullException.ThrowIfNull(configureOptions);

        ServiceCollectionExtensions.ConfigurePerTenantReqs<TOptions>(optionsBuilder.Services);

        optionsBuilder.Services.AddTransient<IConfigureOptions<TOptions>>(sp =>
            new ConfigureNamedOptions<TOptions, TDep1, TDep2, TDep3, TDep4, ITenantContext<TTenantInfo>>(
                optionsBuilder.Name,
                sp.GetRequiredService<TDep1>(),
                sp.GetRequiredService<TDep2>(),
                sp.GetRequiredService<TDep3>(),
                sp.GetRequiredService<TDep4>(),
                sp.GetRequiredService<ITenantContext<TTenantInfo>>(),
                (options, dep1, dep2, dep3, dep4, tenantContext) =>
                {
                    var tenantInfo = tenantContext.TenantInfo;
                    if (tenantInfo is not null)
                        configureOptions(options, dep1, dep2, dep3, dep4, tenantInfo);
                }));

        return optionsBuilder;
    }

    /// <summary>
    /// Registers an action used to post-configure an options type per tenant.
    /// </summary>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    /// <typeparam name="TTenantInfo">The tenant info type.</typeparam>
    /// <param name="optionsBuilder">The options builder instance.</param>
    /// <param name="configureOptions">The action used to post-configure the options for each tenant.</param>
    /// <returns>The options builder for chaining.</returns>
    public static OptionsBuilder<TOptions> PostConfigurePerTenant<TOptions, TTenantInfo>(
        this OptionsBuilder<TOptions> optionsBuilder, Action<TOptions, TTenantInfo> configureOptions)
        where TOptions : class
        where TTenantInfo : ITenantInfo
    {
        ArgumentNullException.ThrowIfNull(configureOptions);

        ServiceCollectionExtensions.ConfigurePerTenantReqs<TOptions>(optionsBuilder.Services);

        optionsBuilder.Services.AddTransient<IPostConfigureOptions<TOptions>>(sp =>
            new PostConfigureOptions<TOptions, ITenantContext<TTenantInfo>>(
                optionsBuilder.Name,
                sp.GetRequiredService<ITenantContext<TTenantInfo>>(),
                (options, tenantContext) =>
                {
                    var tenantInfo = tenantContext.TenantInfo;
                    if (tenantInfo is not null)
                        configureOptions(options, tenantInfo);
                }));

        return optionsBuilder;
    }

    /// <summary>
    /// Registers an action used to post-configure an options type per tenant with one dependency.
    /// </summary>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    /// <typeparam name="TDep">The dependency type.</typeparam>
    /// <typeparam name="TTenantInfo">The tenant info type.</typeparam>
    /// <param name="optionsBuilder">The options builder instance.</param>
    /// <param name="configureOptions">The action used to post-configure the options for each tenant.</param>
    /// <returns>The options builder for chaining.</returns>
    public static OptionsBuilder<TOptions> PostConfigurePerTenant<TOptions, TDep, TTenantInfo>(
        this OptionsBuilder<TOptions> optionsBuilder, Action<TOptions, TDep, TTenantInfo> configureOptions)
        where TOptions : class
        where TDep : class
        where TTenantInfo : ITenantInfo
    {
        ArgumentNullException.ThrowIfNull(configureOptions);

        ServiceCollectionExtensions.ConfigurePerTenantReqs<TOptions>(optionsBuilder.Services);

        optionsBuilder.Services.AddTransient<IPostConfigureOptions<TOptions>>(sp =>
            new PostConfigureOptions<TOptions, TDep, ITenantContext<TTenantInfo>>(
                optionsBuilder.Name,
                sp.GetRequiredService<TDep>(),
                sp.GetRequiredService<ITenantContext<TTenantInfo>>(),
                (options, dep, tenantContext) =>
                {
                    var tenantInfo = tenantContext.TenantInfo;
                    if (tenantInfo is not null)
                        configureOptions(options, dep, tenantInfo);
                }));

        return optionsBuilder;
    }

    /// <summary>
    /// Registers an action used to post-configure an options type per tenant with two dependencies.
    /// </summary>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    /// <typeparam name="TDep1">The first dependency type.</typeparam>
    /// <typeparam name="TDep2">The second dependency type.</typeparam>
    /// <typeparam name="TTenantInfo">The tenant info type.</typeparam>
    /// <param name="optionsBuilder">The options builder instance.</param>
    /// <param name="configureOptions">The action used to post-configure the options for each tenant.</param>
    /// <returns>The options builder for chaining.</returns>
    public static OptionsBuilder<TOptions> PostConfigurePerTenant<TOptions, TDep1, TDep2, TTenantInfo>(
        this OptionsBuilder<TOptions> optionsBuilder, Action<TOptions, TDep1, TDep2, TTenantInfo> configureOptions)
        where TOptions : class
        where TDep1 : class
        where TDep2 : class
        where TTenantInfo : ITenantInfo
    {
        ArgumentNullException.ThrowIfNull(configureOptions);

        ServiceCollectionExtensions.ConfigurePerTenantReqs<TOptions>(optionsBuilder.Services);

        optionsBuilder.Services.AddTransient<IPostConfigureOptions<TOptions>>(sp =>
            new PostConfigureOptions<TOptions, TDep1, TDep2, ITenantContext<TTenantInfo>>(
                optionsBuilder.Name,
                sp.GetRequiredService<TDep1>(),
                sp.GetRequiredService<TDep2>(),
                sp.GetRequiredService<ITenantContext<TTenantInfo>>(),
                (options, dep1, dep2, tenantContext) =>
                {
                    var tenantInfo = tenantContext.TenantInfo;
                    if (tenantInfo is not null)
                        configureOptions(options, dep1, dep2, tenantInfo);
                }));

        return optionsBuilder;
    }

    /// <summary>
    /// Registers an action used to post-configure an options type per tenant with three dependencies.
    /// </summary>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    /// <typeparam name="TDep1">The first dependency type.</typeparam>
    /// <typeparam name="TDep2">The second dependency type.</typeparam>
    /// <typeparam name="TDep3">The third dependency type.</typeparam>
    /// <typeparam name="TTenantInfo">The tenant info type.</typeparam>
    /// <param name="optionsBuilder">The options builder instance.</param>
    /// <param name="configureOptions">The action used to post-configure the options for each tenant.</param>
    /// <returns>The options builder for chaining.</returns>
    public static OptionsBuilder<TOptions> PostConfigurePerTenant<TOptions, TDep1, TDep2, TDep3, TTenantInfo>(
        this OptionsBuilder<TOptions> optionsBuilder,
        Action<TOptions, TDep1, TDep2, TDep3, TTenantInfo> configureOptions)
        where TOptions : class
        where TDep1 : class
        where TDep2 : class
        where TDep3 : class
        where TTenantInfo : ITenantInfo
    {
        ArgumentNullException.ThrowIfNull(configureOptions);

        ServiceCollectionExtensions.ConfigurePerTenantReqs<TOptions>(optionsBuilder.Services);

        optionsBuilder.Services.AddTransient<IPostConfigureOptions<TOptions>>(sp =>
            new PostConfigureOptions<TOptions, TDep1, TDep2, TDep3, ITenantContext<TTenantInfo>>(
                optionsBuilder.Name,
                sp.GetRequiredService<TDep1>(),
                sp.GetRequiredService<TDep2>(),
                sp.GetRequiredService<TDep3>(),
                sp.GetRequiredService<ITenantContext<TTenantInfo>>(),
                (options, dep1, dep2, dep3, tenantContext) =>
                {
                    var tenantInfo = tenantContext.TenantInfo;
                    if (tenantInfo is not null)
                        configureOptions(options, dep1, dep2, dep3, tenantInfo);
                }));

        return optionsBuilder;
    }

    /// <summary>
    /// Registers an action used to post-configure an options type per tenant with four dependencies.
    /// </summary>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    /// <typeparam name="TDep1">The first dependency type.</typeparam>
    /// <typeparam name="TDep2">The second dependency type.</typeparam>
    /// <typeparam name="TDep3">The third dependency type.</typeparam>
    /// <typeparam name="TDep4">The fourth dependency type.</typeparam>
    /// <typeparam name="TTenantInfo">The tenant info type.</typeparam>
    /// <param name="optionsBuilder">The options builder instance.</param>
    /// <param name="configureOptions">The action used to post-configure the options for each tenant.</param>
    /// <returns>The options builder for chaining.</returns>
    public static OptionsBuilder<TOptions> PostConfigurePerTenant<TOptions, TDep1, TDep2, TDep3, TDep4, TTenantInfo>(
        this OptionsBuilder<TOptions> optionsBuilder,
        Action<TOptions, TDep1, TDep2, TDep3, TDep4, TTenantInfo> configureOptions)
        where TOptions : class
        where TDep1 : class
        where TDep2 : class
        where TDep3 : class
        where TDep4 : class
        where TTenantInfo : ITenantInfo
    {
        ArgumentNullException.ThrowIfNull(configureOptions);

        ServiceCollectionExtensions.ConfigurePerTenantReqs<TOptions>(optionsBuilder.Services);

        optionsBuilder.Services.AddTransient<IPostConfigureOptions<TOptions>>(sp =>
            new PostConfigureOptions<TOptions, TDep1, TDep2, TDep3, TDep4, ITenantContext<TTenantInfo>>(
                optionsBuilder.Name,
                sp.GetRequiredService<TDep1>(),
                sp.GetRequiredService<TDep2>(),
                sp.GetRequiredService<TDep3>(),
                sp.GetRequiredService<TDep4>(),
                sp.GetRequiredService<ITenantContext<TTenantInfo>>(),
                (options, dep1, dep2, dep3, dep4, tenantContext) =>
                {
                    var tenantInfo = tenantContext.TenantInfo;
                    if (tenantInfo is not null)
                        configureOptions(options, dep1, dep2, dep3, dep4, tenantInfo);
                }));

        return optionsBuilder;
    }
}