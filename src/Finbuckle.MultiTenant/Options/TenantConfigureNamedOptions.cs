// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Xml.Linq;

namespace Finbuckle.MultiTenant.Options;

/// <inheritdoc />
public class TenantConfigureNamedOptions<TOptions, TTenantInfo> : ITenantConfigureNamedOptions<TOptions, TTenantInfo>, IConfigureNamedOptions<TOptions>
    where TOptions : class, new()
    where TTenantInfo : class, ITenantInfo, new()
{
    /// <summary>
    /// Gets the name of the named option for configuration.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public string? Name { get; }
    private readonly Action<TOptions, TTenantInfo> configureOptions;
    private readonly IMultiTenantContextAccessor<TTenantInfo>? multiTenantContextAccessor;

    /// <summary>
    /// Constructs a new instance of TenantConfigureNamedOptions.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="configureOptions"></param>
    public TenantConfigureNamedOptions(string? name, Action<TOptions, TTenantInfo> configureOptions)
        : this(name, configureOptions, null)
    { }

    /// <summary>
    /// Constructs a new instance of TenantConfigureNamedOptions.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="configureOptions"></param>
    /// <param name="multiTenantContextAccessor"></param>
    public TenantConfigureNamedOptions(string? name, Action<TOptions, TTenantInfo> configureOptions, IMultiTenantContextAccessor<TTenantInfo>? multiTenantContextAccessor)
    {
        Name = name;
        this.configureOptions = configureOptions;
        this.multiTenantContextAccessor = multiTenantContextAccessor;
    }

    /// <inheritdoc />
    public void Configure(string name, TOptions options, TTenantInfo tenantInfo)
    {
        // Null name is used to configure all named options.
        if (Name == null || name == Name)
        {
            configureOptions(options, tenantInfo);
        }
    }

    /// <inheritdoc />
    public void Configure(string? name, TOptions options)
    {
        var tenantInfo = multiTenantContextAccessor?.MultiTenantContext?.TenantInfo;
        if (tenantInfo != null)
        {
            Configure(name, options, tenantInfo);
        }
    }

    /// <inheritdoc />
    public void Configure(TOptions options)
    {
        var tenantInfo = multiTenantContextAccessor?.MultiTenantContext?.TenantInfo;
        if (tenantInfo != null)
        {
            Configure(Microsoft.Extensions.Options.Options.DefaultName, options);
        }
    }
}