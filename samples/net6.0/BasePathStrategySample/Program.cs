using Finbuckle.MultiTenant;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Add MultiTenant
builder.Services.AddMultiTenant<TenantInfo>()
    .WithBasePathStrategy(options => options.RebaseAspNetCorePathBase = true)
    .WithConfigurationStore();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseStaticFiles();

app.UseMultiTenant();
app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();