// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

//    Portions of this file are derived from the .NET Foundation source file located at:
//    https://github.com/dotnet/aspnetcore/blob/main/src/Http/Authentication.Core/src/AuthenticationSchemeProvider.cs

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

// ReSharper disable once CheckNamespace
namespace Finbuckle.MultiTenant.AspNetCore
{
    /// <summary>
    /// Implements <see cref="IAuthenticationSchemeProvider"/>.
    /// </summary>
    internal class MultiTenantAuthenticationSchemeProvider : IAuthenticationSchemeProvider
    {
        private readonly IAuthenticationSchemeProvider _inner;

        /// <summary>
        /// Creates an instance of <see cref="MultiTenantAuthenticationSchemeProvider"/>
        /// using the specified <paramref name="options"/> and decorates the existing <paramref name="inner"/>.
        /// </summary>
        /// <param name="inner">The <see cref="IAuthenticationSchemeProvider"/> to decorate.</param>
        /// <param name="options">The <see cref="AuthenticationOptions"/> options.</param>
        public MultiTenantAuthenticationSchemeProvider(IAuthenticationSchemeProvider inner, IOptions<AuthenticationOptions> options)
            : this(inner, options, new Dictionary<string, AuthenticationScheme>(StringComparer.Ordinal))
        {
        }

        /// <summary>
        /// Creates an instance of <see cref="MultiTenantAuthenticationSchemeProvider"/>
        /// using the specified <paramref name="options"/> and <paramref name="schemes"/>. This instance decorates the existing <paramref name="inner"/>.
        /// </summary>
        /// <param name="inner">The <see cref="IAuthenticationSchemeProvider"/> to decorate.</param>
        /// <param name="options">The <see cref="AuthenticationOptions"/> options.</param>
        /// <param name="schemes">The dictionary used to store authentication schemes.</param>
        public MultiTenantAuthenticationSchemeProvider(IAuthenticationSchemeProvider inner, IOptions<AuthenticationOptions> options, IDictionary<string, AuthenticationScheme> schemes)
        {
            _inner = inner;
            _optionsProvider = options;

            _schemes = schemes ?? throw new ArgumentNullException(nameof(schemes));
            _requestHandlers = new List<AuthenticationScheme>();

            foreach (var builder in _optionsProvider.Value.Schemes)
            {
                var scheme = builder.Build();
                // ReSharper disable once VirtualMemberCallInConstructor
                // As-is from MS source.
                AddScheme(scheme);
            }
        }

        private readonly IOptions<AuthenticationOptions> _optionsProvider;
        private readonly object _lock = new object();

        private readonly IDictionary<string, AuthenticationScheme> _schemes;
        private readonly List<AuthenticationScheme> _requestHandlers;

        private Task<AuthenticationScheme?> GetDefaultSchemeAsync()
            => _optionsProvider.Value.DefaultScheme != null
            ? GetSchemeAsync(_optionsProvider.Value.DefaultScheme)
            : Task.FromResult<AuthenticationScheme?>(null);

        /// <summary>
        /// Returns the scheme for this tenant that will be used by default for <see cref="IAuthenticationService.AuthenticateAsync(HttpContext, string)"/>.
        /// This is typically specified via <see cref="AuthenticationOptions.DefaultAuthenticateScheme"/>.
        /// Otherwise, this will fallback to <see cref="AuthenticationOptions.DefaultScheme"/>.
        /// </summary>
        /// <returns>The scheme that will be used by default for <see cref="IAuthenticationService.AuthenticateAsync(HttpContext, string)"/> or null if not found.</returns>
        public virtual Task<AuthenticationScheme?> GetDefaultAuthenticateSchemeAsync()
            => _optionsProvider.Value.DefaultAuthenticateScheme != null
            ? GetSchemeAsync(_optionsProvider.Value.DefaultAuthenticateScheme)
            : GetDefaultSchemeAsync();

        /// <summary>
        /// Returns the scheme for this tenant that will be used by default for <see cref="IAuthenticationService.ChallengeAsync(HttpContext, string, AuthenticationProperties)"/>.
        /// This is typically specified via <see cref="AuthenticationOptions.DefaultChallengeScheme"/>.
        /// Otherwise, this will fallback to <see cref="AuthenticationOptions.DefaultScheme"/>.
        /// </summary>
        /// <returns>The scheme that will be used by default for <see cref="IAuthenticationService.ChallengeAsync(HttpContext, string, AuthenticationProperties)"/> or null if not found.</returns>
        public virtual Task<AuthenticationScheme?> GetDefaultChallengeSchemeAsync()
            => _optionsProvider.Value.DefaultChallengeScheme != null
            ? GetSchemeAsync(_optionsProvider.Value.DefaultChallengeScheme)
            : GetDefaultSchemeAsync();

        /// <summary>
        /// Returns the scheme for this tenant that will be used by default for <see cref="IAuthenticationService.ForbidAsync(HttpContext, string, AuthenticationProperties)"/>.
        /// This is typically specified via <see cref="AuthenticationOptions.DefaultForbidScheme"/>.
        /// Otherwise, this will fallback to <see cref="GetDefaultChallengeSchemeAsync"/> .
        /// </summary>
        /// <returns>The scheme that will be used by default for <see cref="IAuthenticationService.ForbidAsync(HttpContext, string, AuthenticationProperties)"/> or null if not found.</returns>
        public virtual Task<AuthenticationScheme?> GetDefaultForbidSchemeAsync()
            => _optionsProvider.Value.DefaultForbidScheme != null
            ? GetSchemeAsync(_optionsProvider.Value.DefaultForbidScheme)
            : GetDefaultChallengeSchemeAsync();

        /// <summary>
        /// Returns the scheme for this tenant that will be used by default for <see cref="IAuthenticationService.SignInAsync(HttpContext, string, System.Security.Claims.ClaimsPrincipal, AuthenticationProperties)"/>.
        /// This is typically specified via <see cref="AuthenticationOptions.DefaultSignInScheme"/>.
        /// Otherwise, this will fallback to <see cref="AuthenticationOptions.DefaultScheme"/>.
        /// </summary>
        /// <returns>The scheme that will be used by default for <see cref="IAuthenticationService.SignInAsync(HttpContext, string, System.Security.Claims.ClaimsPrincipal, AuthenticationProperties)"/> or null if not found.</returns>
        public virtual Task<AuthenticationScheme?> GetDefaultSignInSchemeAsync()
            => _optionsProvider.Value.DefaultSignInScheme != null
            ? GetSchemeAsync(_optionsProvider.Value.DefaultSignInScheme)
            : GetDefaultSchemeAsync();

        /// <summary>
        /// Returns the scheme for this tenant that will be used by default for <see cref="IAuthenticationService.SignOutAsync(HttpContext, string, AuthenticationProperties)"/>.
        /// This is typically specified via <see cref="AuthenticationOptions.DefaultSignOutScheme"/>.
        /// Otherwise this will fallback to <see cref="GetDefaultSignInSchemeAsync"/> if that supoorts sign out.
        /// </summary>
        /// <returns>The scheme that will be used by default for <see cref="IAuthenticationService.SignOutAsync(HttpContext, string, AuthenticationProperties)"/> or null if not found.</returns>
        public virtual Task<AuthenticationScheme?> GetDefaultSignOutSchemeAsync()
            => _optionsProvider.Value.DefaultSignOutScheme != null
            ? GetSchemeAsync(_optionsProvider.Value.DefaultSignOutScheme)
            : GetDefaultSignInSchemeAsync();

        /// <summary>
        /// Returns the <see cref="AuthenticationScheme"/> matching the name, or null.
        /// </summary>
        /// <param name="name">The name of the authenticationScheme.</param>
        /// <returns>The scheme or null if not found.</returns>
        public virtual async Task<AuthenticationScheme?> GetSchemeAsync(string name)
        {
            AuthenticationScheme? scheme = null;

            if (_inner != null)
            {
                scheme = await _inner.GetSchemeAsync(name);
            }

            if (scheme == null)
            {
                scheme = _schemes.ContainsKey(name) ? _schemes[name] : null;
            }

            return scheme;
        }

        /// <summary>
        /// Returns the scheme for this tenants in priority order for request handling.
        /// </summary>
        /// <returns>The schemes in priority order for request handling</returns>
        public virtual Task<IEnumerable<AuthenticationScheme>> GetRequestHandlerSchemesAsync()
            // ReSharper disable once InconsistentlySynchronizedField
            // As-is from MS source
            => Task.FromResult<IEnumerable<AuthenticationScheme>>(_requestHandlers);

        /// <summary>
        /// Registers a scheme for use by <see cref="IAuthenticationService"/>.
        /// </summary>
        /// <param name="scheme">The scheme.</param>
        public virtual void AddScheme(AuthenticationScheme scheme)
        {
            if (_schemes.ContainsKey(scheme.Name))
            {
                throw new InvalidOperationException("Scheme already exists: " + scheme.Name);
            }
            lock (_lock)
            {
                if (_schemes.ContainsKey(scheme.Name))
                {
                    throw new InvalidOperationException("Scheme already exists: " + scheme.Name);
                }
                if (typeof(IAuthenticationRequestHandler).IsAssignableFrom(scheme.HandlerType))
                {
                    _requestHandlers.Add(scheme);
                }
                _schemes[scheme.Name] = scheme;
            }
        }

        /// <summary>
        /// Removes a scheme, preventing it from being used by <see cref="IAuthenticationService"/>.
        /// </summary>
        /// <param name="name">The name of the authenticationScheme being removed.</param>
        public virtual void RemoveScheme(string name)
        {
            if (!_schemes.ContainsKey(name))
            {
                return;
            }
            lock (_lock)
            {
                if (_schemes.ContainsKey(name))
                {
                    var scheme = _schemes[name];
                    _requestHandlers.Remove(scheme);
                    _schemes.Remove(name);
                }
            }
        }

        public virtual Task<IEnumerable<AuthenticationScheme>> GetAllSchemesAsync()
            => Task.FromResult<IEnumerable<AuthenticationScheme>>(_schemes.Values);
    }
}