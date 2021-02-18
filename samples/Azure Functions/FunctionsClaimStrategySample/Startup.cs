
using Finbuckle.MultiTenant;

using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;

[assembly: FunctionsStartup(typeof(FunctionsClaimStrategySample.Startup))]
namespace FunctionsClaimStrategySample
{
    public class Startup : FunctionsStartup
    {
        public IConfiguration Configuration { get; set; }

        public Startup()
        {
            Configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddJsonFile("local.settings.json", true)
                .Build();
        }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddMultiTenant<TenantInfo>()
                    .WithConfigurationStore()
                    .WithClaimStrategy()
                    .WithPerTenantAuthentication();

            builder.Services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultScheme = Microsoft.Identity.Web.Constants.Bearer;
                sharedOptions.DefaultChallengeScheme = Microsoft.Identity.Web.Constants.Bearer;
            })
                .AddMicrosoftIdentityWebApi(Configuration.GetSection(Microsoft.Identity.Web.Constants.AzureAdB2C));
        }
    }
}
