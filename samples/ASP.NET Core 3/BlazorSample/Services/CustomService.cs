using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorSample.Services
{
    public class CustomService
    {
        private readonly IOptions<CustomOptions> optionsAccessor;
        private readonly AuthenticationStateProvider authenticationStateProvider;

        public CustomService(IOptions<CustomOptions> optionsAccessor, AuthenticationStateProvider authenticationStateProvider)
        {
            this.optionsAccessor = optionsAccessor;

            this.authenticationStateProvider = authenticationStateProvider;
        }

        public async Task<CustomOptions> GetOptionsAsync()
        {
            var options = this.optionsAccessor.Value;

            var authenticationState = await this.authenticationStateProvider.GetAuthenticationStateAsync();
            return options;
        }
    }
}
