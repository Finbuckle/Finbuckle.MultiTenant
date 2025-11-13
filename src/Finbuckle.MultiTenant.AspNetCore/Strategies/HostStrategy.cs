// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Text.RegularExpressions;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Finbuckle.MultiTenant.AspNetCore.Strategies;

/// <summary>
/// A strategy that determines the tenant identifier from the request host header using a template pattern.
/// </summary>
public sealed class HostStrategy : IMultiTenantStrategy
{
    private readonly Regex regex;

    /// <summary>
    /// Initializes a new instance of HostStrategy with a template pattern.
    /// </summary>
    /// <param name="template">The template pattern for extracting the tenant identifier from the host. Use "__tenant__" as a placeholder for the identifier.</param>
    /// <exception cref="MultiTenantException">Thrown when the template is invalid.</exception>
    public HostStrategy(string template)
    {
        // match whole domain if just "__tenant__".
        if (template == Constants.TenantToken)
        {
            template = template.Replace(Constants.TenantToken, "(?<identifier>.+)");
        }
        else
        {
            // Check for valid template.
            // Template cannot be null or whitespace.
            if (string.IsNullOrWhiteSpace(template))
            {
                throw new MultiTenantException("Template cannot be null or whitespace.");
            }

            // Wildcard "*" must be only occur once in template.
            if (Regex.Match(template, @"\*(?=.*\*)").Success)
            {
                throw new MultiTenantException("Wildcard \"*\" must be only occur once in template.");
            }

            // Wildcard "*" must be only token in template segment.
            if (Regex.Match(template, @"\*[^\.]|[^\.]\*").Success)
            {
                throw new MultiTenantException("\"*\" wildcard must be only token in template segment.");
            }

            // Wildcard "?" must be only token in template segment.
            if (Regex.Match(template, @"\?[^\.]|[^\.]\?").Success)
            {
                throw new MultiTenantException("\"?\" wildcard must be only token in template segment.");
            }

            template = template.Trim().Replace(".", @"\.");
            string wildcardSegmentsPattern = @"(\.[^\.]+)*";
            string singleSegmentPattern = @"[^\.]+";
            if (template.Substring(template.Length - 3, 3) == @"\.*")
            {
                template = string.Concat(template.AsSpan(0, template.Length - 3), wildcardSegmentsPattern);
            }

            wildcardSegmentsPattern = @"([^\.]+\.)*";
            template = template.Replace(@"*\.", wildcardSegmentsPattern);
            template = template.Replace("?", singleSegmentPattern);
            template = template.Replace(Constants.TenantToken, @"(?<identifier>[^\.]+)");
        }

        regex = new Regex($"^{template}$", RegexOptions.ExplicitCapture | RegexOptions.Compiled,
            TimeSpan.FromMilliseconds(100));
    }

    /// <inheritdoc />
    public Task<string?> GetIdentifierAsync(object context)
    {
        if (context is not HttpContext httpContext)
            return Task.FromResult<string?>(null);

        var host = httpContext.Request.Host;

        if (!host.HasValue)
            return Task.FromResult<string?>(null);

        string? identifier = null;

        var match = regex.Match(host.Host);

        if (match.Success)
        {
            identifier = match.Groups["identifier"].Value;
        }

        return Task.FromResult(identifier);
    }
}