using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using IdentitySample;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IdentitySample.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();

// Add MultiTenant
builder.Services.AddMultiTenant<AppTenantInfo>()
    .WithBasePathStrategy(options => options.RebaseAspNetCorePathBase = true)
    .WithConfigurationStore()
    .WithPerTenantAuthentication();

var app = builder.Build();

// Apply migrations if needed
var store = app.Services.GetRequiredService<IMultiTenantStore<AppTenantInfo>>();
foreach(var tenant in await store.GetAllAsync())
{
    await using var db = new ApplicationDbContext(tenant);
    await db.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseMultiTenant();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();