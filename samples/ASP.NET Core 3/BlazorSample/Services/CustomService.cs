using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.ProtectedBrowserStorage;
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

        public CustomService(IOptions<CustomOptions> optionsAccessor)
        {
            this.optionsAccessor = optionsAccessor;
        }

        public async Task<CustomOptions> GetOptionsAsync()
        {
            var options = this.optionsAccessor.Value;

            return options;
        }
    }
}
