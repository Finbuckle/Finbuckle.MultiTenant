namespace Finbuckle.MultiTenant.AspNetCore.Test.Routing;

public class ExcludeFromMultiTenantResolutionShould
{
    // TODO: rethink this since webhostbuilder is obsolete
    // private const string EndpointStringResponse = "No tenant available.";
    //
    // [Theory]
    // [InlineData("/initech", "initech")]
    // [InlineData("/", "initech")]
    // public async Task ReturnExpectedResponse(string path, string identifier)
    // {
    //     IWebHostBuilder hostBuilder = GetTestHostBuilder(identifier, "__tenant__", path);
    //     using var server = new TestServer(hostBuilder);
    //     var client = server.CreateClient();
    //
    //     var response = await client.GetStringAsync(path);
    //     Assert.Equal(EndpointStringResponse, response);
    //
    //     response = await client.GetStringAsync(path.TrimEnd('/') + "/tenantInfo");
    //     Assert.Equal("initech", response);
    // }
    //
    // private static IWebHostBuilder GetTestHostBuilder(string identifier, string sessionKey, string routePattern)
    // {
    //     return new WebHostBuilder()
    //         .ConfigureServices(services =>
    //         {
    //             services.AddDistributedMemoryCache();
    //             services.AddSession(options =>
    //             {
    //                 options.IdleTimeout = TimeSpan.FromSeconds(5);
    //                 options.Cookie.HttpOnly = true;
    //                 options.Cookie.IsEssential = true;
    //             });
    //
    //             services.AddMultiTenant<TenantInfo>()
    //                 .WithStrategy<SessionStrategy>(ServiceLifetime.Singleton, sessionKey)
    //                 .WithInMemoryStore();
    //
    //             services.AddMvc();
    //         })
    //         .Configure(app =>
    //         {
    //             app.UseRouting();
    //             app.UseSession();
    //             app.Use(async (context, next) =>
    //             {
    //                 context.Session.SetString(sessionKey, identifier);
    //                 await next(context);
    //             });
    //             app.UseMultiTenant();
    //
    //             app.UseEndpoints(endpoints =>
    //             {
    //                 var group = endpoints.MapGroup(routePattern);
    //
    //                 group.Map("/", async context => await WriteResponseAsync(context))
    //                     .ExcludeFromMultiTenantResolution();
    //
    //                 group.Map("/tenantInfo", async context => await WriteResponseAsync(context));
    //             });
    //
    //             var store = app.ApplicationServices.GetRequiredService<IMultiTenantStore<TenantInfo>>();
    //             store.AddAsync(new TenantInfo { Id = identifier, Identifier = identifier }).Wait();
    //         });
    // }
    //
    // private static async Task WriteResponseAsync(HttpContext context)
    // {
    //     var multiTenantContext = context.GetMultiTenantContext<TenantInfo>();
    //
    //     if (multiTenantContext.TenantInfo?.Id is null)
    //     {
    //         await context.Response.WriteAsync(EndpointStringResponse);
    //     }
    //     else
    //     {
    //         await context.Response.WriteAsync(multiTenantContext.TenantInfo.Id);
    //     }
    // }
}