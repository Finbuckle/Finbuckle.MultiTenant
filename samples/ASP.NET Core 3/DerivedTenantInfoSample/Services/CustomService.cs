using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DerivedTenantInfoSample.Services
{
    public class CustomService
    {
        private readonly IOptions<CustomOptions> optionsAccessor;

        public CustomService(IOptions<CustomOptions> optionsAccessor)
        {
            this.optionsAccessor = optionsAccessor;
        }

        public Task<CustomOptions> GetOptions()
        {
            return Task.FromResult(this.optionsAccessor.Value);
        }
    }
}
