using Finbuckle.MultiTenant;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Configure MultiTenant to use our AppTenantInfo class with the route strategy and in-memory store.
var tenantList = BuildTenantList();
builder.Services.AddMultiTenant<AppTenantInfo>()
    .WithRouteStrategy()
    .WithInMemoryStore(options =>
    {
        options.Tenants = tenantList;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Add the MultiTenant middleware.
app.UseMultiTenant();

// Define summaries in English, French, and German.
var summaries = new Dictionary<string, string[]>
{
    ["en"] = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"],
    ["fr"] = ["Glacial", "Vivifiant", "Frais", "Froid", "Doux", "Chaud", "Clément", "Très chaud", "Accablant", "Brûlant"],
    ["de"] = ["Eiskalt", "Frisch", "Kühl", "Kalt", "Mild", "Warm", "Lau", "Heiß", "Schwül", "Glühend"]
};

// Return weather with the summaries in the tenant's preferred language.
app.MapGet("/{__tenant__}/weatherforecast", (string __tenant__, HttpContext http) =>
{
    // Note: __tenant__ parameter isn't used but required for OpenAPI to work properly.
    
    // Get the MultiTenantContext instance.
    var mtc = http.GetMultiTenantContext<AppTenantInfo>();
    
    // Set language to the tenant's preferred or default to english.
    var language = mtc.TenantInfo?.PreferredLanguage ?? "en";
    
    var selectedSummaries = summaries[language];
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            selectedSummaries[Random.Shared.Next(selectedSummaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

// Define tenants for the in-memory store.
List<AppTenantInfo> BuildTenantList() =>
[
    new AppTenantInfo
    {
        Id = "tenant-001",
        Identifier = "acme",
        Name = "Acme Corporation",
        PreferredLanguage = "en"
    },

    new AppTenantInfo
    {
        Id = "tenant-002",
        Identifier = "globex",
        Name = "Globex GmbH",
        PreferredLanguage = "de"
    },

    new AppTenantInfo
    {
        Id = "tenant-003",
        Identifier = "parisian",
        Name = "Parisian Foods",
        PreferredLanguage = "fr"
    }
];

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}