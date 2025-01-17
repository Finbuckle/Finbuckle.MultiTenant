

using Finbuckle.MultiTenant;
using MassTransit;

using MassTransitSample.Consumers;

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

using Scalar.AspNetCore;

namespace MassTransitSample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddLogging();

            builder.Services.AddMultiTenant<TenantInfo>()
                .WithConfigurationStore() // Store is in appsettings.json
                .WithHeaderStrategy("X-Tenant") // Use the http header to determine the tenant this defaults to header '__tenant__' but can be overridden by passing a string parameter.
                                                // In this case the HTTP header is X-Tenant and the mass transit header is __tenant__.
                                                // This is done to show how different headers can be used for different purposes. And because the MassTransitSample does not support headers that start with underscores.
                .WithMassTransitHeaderStrategy(); // Required for MassTransit to maintain tenant context. This adds or reads the tenant identifier from the MassTransit message headers. Defaults to '__tenant__' but can be overridden by passing a string parameter.
                                                  // NOTE the header strategy can use a different header from the HTTP strategy. This is useful if you want to use different headers for different purposes.
                                                  // Be sure to use the same header in MassTransit Strategies in the other projects that consume or produce messages.


            builder.Services.AddMassTransit(x =>
            {
                x.AddConsumer<GettingStartedConsumer>(); // The MassTransit Consumer that will be used to consume messages.

                // You can use the following code to add filters to the MassTransit pipeline.
                // This is individual filters for each type of MassTransit action.
                // This is not required if you use the AddTenantFilters extension method. See uncommented section below.

                //x.UsingInMemory((IBusRegistrationContext context, IInMemoryBusFactoryConfigurator cfg) => //using in memory for simplicity. Please replace with your preferred transport method.
                //{
                //    cfg.UseConsumeFilter(typeof(TenantConsumeFilter<>), context); // Required if wanting to have a MassTransit Consumer and maintain tenant context. To use this filter, .WithMassTransitHeaderStrategy() must be called in the MultiTenantBuilder.
                //    cfg.UsePublishFilter(typeof(TenantPublishFilter<>), context); // Required if wanting to have a MassTransit Publisher and maintain tenant context. To use this filter, .WithMassTransitHeaderStrategy() must be called in the MultiTenantBuilder.
                //    cfg.UseSendFilter(typeof(TenantPublishFilter<>), context); // Required if wanting to have a MassTransit Sender and maintain tenant context. To use this filter, .WithMassTransitHeaderStrategy() must be called in the MultiTenantBuilder.
                //    cfg.ConfigureEndpoints(context);
                //});

                // This is a single add command that can be used to apply all FinBuckle.MultiTenant filters to the MassTransit pipeline.
                x.UsingInMemory((IBusRegistrationContext context, IInMemoryBusFactoryConfigurator cfg) => //using in memory for simplicity. Please replace with your preferred transport method.
                {
                    cfg.AddTenantFilters(context); // Required if wanting to have a MassTransit Consumer and maintain tenant context. To use this filter, .WithMassTransitHeaderStrategy() must be called in the MultiTenantBuilder.
                    cfg.ConfigureEndpoints(context);
                });
            });

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi(opts =>
            {
                opts.AddOperationTransformer((operation, context, cancellationToken) =>
                {
                    // Ensure the Parameters collection is initialized
                    operation.Parameters ??= new List<OpenApiParameter>();

                    // Add the required custom header parameter
                    operation.Parameters.Add(new OpenApiParameter
                    {
                        Name = "X-Tenant",
                        In = ParameterLocation.Header,
                        Required = true,
                        Schema = new OpenApiSchema
                        {
                            Type = "string"
                        }
                    });

                    return Task.CompletedTask;
                });

            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            app.UseMultiTenant();

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }

        internal sealed class TenantHeaderTransformer : IOpenApiDocumentTransformer
        {
            public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }
    }
}
