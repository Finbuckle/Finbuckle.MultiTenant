using IdentitySampleApp.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Finbuckle.MultiTenant.AspNetCore.Extensions;
using Finbuckle.MultiTenant.Extensions;
using IdentitySampleApp;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<AppIdentityDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<AppIdentityDbContext>();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages().AddRazorPagesOptions(options =>
{
    // This adds the __tenant__ route parameter to all Razor Pages routes, e.g. the Identity UI pages.
    options.Conventions.Add(new MultiTenantPageRouteModelConvention());
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnValidatePrincipal = context =>
    {
        return Task.CompletedTask;
    };
});

builder.Services.AddMultiTenant<AppTenantInfo>()
    .WithRouteStrategy()
    .WithConfigurationStore()
    .WithPerTenantAuthentication();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseMultiTenant();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets().ShortCircuit();
app.MapControllerRoute(
        name: "default",
        pattern: "{__tenant__}/{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
app.MapControllerRoute(
        name: "default_notenant",
        pattern: "/{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
app.MapRazorPages()
    .WithStaticAssets();

SampleHelper.SeedIdentity(app);

app.Run();