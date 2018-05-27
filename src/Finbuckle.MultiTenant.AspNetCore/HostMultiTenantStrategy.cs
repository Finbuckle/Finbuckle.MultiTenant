using System;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.AspNetCore;
using Microsoft.AspNetCore.Http;
using Finbuckle.MultiTenant.Core;
using Finbuckle.MultiTenant.Core.Abstractions;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Finbuckle.MultiTenant.AspNetCore
{
    public class HostMultiTenantStrategy : IMultiTenantStrategy
    {
        private readonly string regex;
        private readonly ILogger<HostMultiTenantStrategy> logger;

        public HostMultiTenantStrategy(string template, ILogger<HostMultiTenantStrategy> logger = null)
        {
            // Check for valid template. Template cannot have "*" on each side of __tenant__ placeholder.
            if (string.IsNullOrWhiteSpace(template) ||
                Regex.Match(template, @"^.*\*.*\.__tenant__\..*\*.*$").Success)
            {
                throw new MultiTenantException("Invalid host template.");
            }

            template = template.Trim().Replace(".", @"\.");
            string wildcardSegmentsPattern = @"(\.[^\.]+)#";
            string singleSegmentPattern = @"[^\.]+";
            if (template.Substring(template.Length - 3, 3) == @"\.*")
            {
                template = template.Substring(0, template.Length - 3) + wildcardSegmentsPattern;
            }

            wildcardSegmentsPattern = @"([^\.]+\.)#";
            template = template.Replace(@"*\.", wildcardSegmentsPattern);
            template = template.Replace("?", singleSegmentPattern);
            template = template.Replace("__tenant__", @"(?<identifier>[^\.]+)");
            template = $"^{template}$".Replace("#", "*");

            this.regex = template;
            this.logger = logger;
        }

        public string GetIdentifier(object context)
        {
            if (!typeof(HttpContext).IsAssignableFrom(context.GetType()))
                throw new MultiTenantException(null,
                    new ArgumentException("\"context\" type must be of type HttpContext", nameof(context)));

            var host = (context as HttpContext).Request.Host;

            Utilities.TryLogInfo(logger, $"Host:  \"{host.Host ?? "<null>"}\"");

            if (host.HasValue == false)
                return null;

            string identifier = null;

            var match = Regex.Match(host.Host, regex,
                RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase,
                TimeSpan.FromMilliseconds(100));

            if (match.Success)
            {
                identifier = match.Groups["identifier"].Value;
            }

            Utilities.TryLogInfo(logger, $"Found identifier:  \"{identifier ?? "<null>"}\"");

            return identifier;
        }
    }
}