using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Finbuckle.MultiTenant.AspNetCore.Strategies;

/// <summary>
/// A link generator that adds the current tenant to the default ambient values by promoting specified ambient route values to explicit route values.
/// </summary>
public class MultiTenantAmbientValueLinkGenerator : LinkGenerator
{
    private readonly LinkGenerator _inner;
    private readonly IList<string> _ambientValueKeys;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiTenantAmbientValueLinkGenerator"/> class.
    /// </summary>
    /// <param name="inner">The inner link generator to delegate to.</param>
    /// <param name="ambientValueKeys">The list of ambient value keys to promote to explicit values.</param>
    public MultiTenantAmbientValueLinkGenerator(LinkGenerator inner, IList<string> ambientValueKeys)
    {
        _inner = inner;
        _ambientValueKeys = ambientValueKeys;
    }

    /// <summary>
    /// Promotes ambient values to explicit route values for the specified keys.
    /// </summary>
    /// <param name="values">The explicit route values.</param>
    /// <param name="ambientValues">The ambient route values.</param>
    /// <returns>A tuple containing the new explicit values and updated ambient values.</returns>
    private (RouteValueDictionary newValues, RouteValueDictionary? newAmbientValues) PromoteAmbientValues(
        RouteValueDictionary values, RouteValueDictionary? ambientValues)
    {
        if (ambientValues == null)
            return (values, null);

        // Copy them so we don't affect anything outside our call chain.
        var newValues = new RouteValueDictionary(values);
        var newAmbientValues = new RouteValueDictionary(ambientValues);

        foreach (var key in _ambientValueKeys)
        {
            // Do we even have this ambient value?
            if (newAmbientValues.TryGetValue(key, out var value))
            {
                // Try to add it to the regular values.
                if (newValues.TryAdd(key, value))
                {
                    // Remove from ambient value if successful.
                    newAmbientValues.Remove(key);
                }
            }
        }

        return (newValues, newAmbientValues);
    }

    /// <inheritdoc />
    public override string? GetPathByAddress<TAddress>(HttpContext httpContext, TAddress address,
        RouteValueDictionary values, RouteValueDictionary? ambientValues = null, PathString? pathBase = null,
        FragmentString fragment = default, LinkOptions? options = null)
    {
        var promotedValues = PromoteAmbientValues(values, ambientValues);
        return _inner.GetPathByAddress(httpContext,
            address,
            promotedValues.newValues,
            promotedValues.newAmbientValues,
            pathBase,
            fragment,
            options);
    }

    /// <inheritdoc />
    public override string? GetPathByAddress<TAddress>(TAddress address, RouteValueDictionary values,
        PathString pathBase = default, FragmentString fragment = default, LinkOptions? options = null)
    {
        return _inner.GetPathByAddress(address,
            values,
            pathBase,
            fragment,
            options);
    }

    /// <inheritdoc />
    public override string? GetUriByAddress<TAddress>(HttpContext httpContext, TAddress address,
        RouteValueDictionary values, RouteValueDictionary? ambientValues = null, string? scheme = null,
        HostString? host = null, PathString? pathBase = null, FragmentString fragment = default,
        LinkOptions? options = null)
    {
        var promotedValues = PromoteAmbientValues(values, ambientValues);
        return _inner.GetUriByAddress(httpContext,
            address,
            promotedValues.newValues,
            promotedValues.newAmbientValues,
            scheme,
            host,
            pathBase,
            fragment,
            options);
    }

    /// <inheritdoc />
    public override string? GetUriByAddress<TAddress>(TAddress address, RouteValueDictionary values, string scheme,
        HostString host, PathString pathBase = default, FragmentString fragment = default, LinkOptions? options = null)
    {
        return _inner.GetUriByAddress(address,
            values,
            scheme,
            host,
            pathBase,
            fragment,
            options);
    }
}