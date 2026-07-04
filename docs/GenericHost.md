# .NET Generic Host Integration

MultiTenant works with any .NET application using dependency injection, not just ASP.NET Core. The .NET Generic Host
app model — used by worker services, console apps, and background processes — is fully supported. This guide covers
tenant resolution in non-web scenarios.

## Package Installation

For Generic Host apps install the `Finbuckle.MultiTenant` core package:

```bash
dotnet add package Finbuckle.MultiTenant
```

If you also need [per-tenant options](Options), add `Finbuckle.MultiTenant.Options`. For
[EF Core data isolation](EFCore), add `Finbuckle.MultiTenant.EntityFrameworkCore`.

> The `Finbuckle.MultiTenant.AspNetCore` package is **not** required and should not be used — its strategies
> and middleware depend on `HttpContext`, which is unavailable in Generic Host apps.

## How It Differs from ASP.NET Core

In ASP.NET Core, the `UseMultiTenant()` middleware automatically resolves the tenant for each HTTP request and
populates the scoped `ITenantContext`. In a Generic Host app there is **no middleware**. You must manually:
1. Create a DI scope for each unit of work (message, job, iteration).
2. Resolve the tenant within that scope using `ITenantResolver`.
3. Seed the scoped `ITenantContext` with the resolved tenant.

Once the scoped `ITenantContext` is populated, [per-tenant options](Options),
[EF Core data isolation](EFCore), and all tenant-aware services work identically to ASP.NET Core.

## Configuring Services

Service registration is the same as any other app. Use `AddMultiTenant<TTenantInfo>()` with strategies and stores
that work without `HttpContext`.

### Compatible Strategies

These strategies from the core `Finbuckle.MultiTenant` package work in any host:

| Strategy | Usage |
|---|---|
| [Delegate Strategy](Strategies#delegate-strategy) | **Most common.** Pass your message, job context, or arbitrary object. |
| [Static Strategy](Strategies#static-strategy) | Useful as a fallback or default tenant. |
| Custom `IMultiTenantStrategy` | Implement your own for full control. |

Strategies from `Finbuckle.MultiTenant.AspNetCore` (Host, Route, Base Path, Header, Claim, Session, HttpContext)
rely on `HttpContext` and are **not** compatible.

### Compatible Stores

All stores work in Generic Host apps:

- [Configuration Store](Stores#configuration-store)
- [In-Memory Store](Stores#in-memory-store)
- [EFCore Store](Stores#efcore-store)
- [Http Remote Store](Stores#http-remote-store)
- [Distributed Cache Store](Stores#distributed-cache-store)
- [Echo Store](Stores#echo-store)
- Custom `IMultiTenantStore`

### Example: Queue Processor Registration

```csharp
using Finbuckle.MultiTenant;

var builder = Host.CreateApplicationBuilder(args);

// Register MultiTenant services with a delegate strategy.
// The delegate receives whatever object you pass to TenantResolver.ResolveAsync().
builder.Services.AddMultiTenant<TenantInfo>()
    .WithDelegateStrategy(async context =>
    {
        if (context is QueueMessage message)
            return message.TenantId;
        return null;
    })
    .WithStaticStrategy("default-tenant") // fallback
    .WithConfigurationStore();

// Register your message handler.
builder.Services.AddScoped<IMessageHandler, MessageHandler>();
```

### Example: Console App Registration

```csharp
using Finbuckle.MultiTenant;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddMultiTenant<TenantInfo>()
    .WithDelegateStrategy(async ctx =>
    {
        // ctx is whatever you pass in
        return ctx as string; // e.g., tenant identifier string
    })
    .WithInMemoryStore(options =>
    {
        options.Tenants.Add(new TenantInfo
        {
            Id = Guid.NewGuid().ToString(),
            Identifier = "tenant-a",
            Name = "Tenant A"
        });
    });
```

## Resolving a Tenant

Resolution is a two-step process:

1. Call `ITenantResolver.ResolveAsync(context)` to determine the tenant.
2. Set the result's `TenantInfo` on the scoped `ITenantContext`.

### Worker Service Pattern (One Tenant per Message)

The most common Generic Host pattern: create a scope per message, resolve the tenant, and process within
that scope.

```csharp
public class QueueProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public QueueProcessor(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var message = await ReceiveMessageAsync(stoppingToken);

            // Each message gets its own scope and tenant.
            await using var scope = _scopeFactory.CreateAsyncScope();
            var services = scope.ServiceProvider;

            // 1. Resolve tenant for this message.
            var resolver = services.GetRequiredService<ITenantResolver<TenantInfo>>();
            var result = await resolver.ResolveAsync(message);

            // 2. Seed the scoped ITenantContext.
            var tenantContext = services.GetRequiredService<ITenantContext<TenantInfo>>();
            if (result.TenantInfo is not null)
                tenantContext.TenantInfo = result.TenantInfo;

            // 3. Process within the scoped tenant.
            var handler = services.GetRequiredService<IMessageHandler>();
            await handler.HandleAsync(message, stoppingToken);
        }
    }

    private async Task<QueueMessage> ReceiveMessageAsync(CancellationToken ct)
    {
        // Your queue receive logic here.
        throw new NotImplementedException();
    }
}
```

### Console App Pattern (Single Tenant at Startup)

For apps that resolve the tenant once at startup, create a scope and use it for the app's lifetime:

```csharp
using Finbuckle.MultiTenant;

var services = new ServiceCollection();
services.AddMultiTenant<TenantInfo>()
    .WithDelegateStrategy(async ctx => ctx as string)
    .WithInMemoryStore(/* ... */);

var provider = services.BuildServiceProvider();

// Create a scope for the app lifetime.
using var scope = provider.CreateScope();
var scopedProvider = scope.ServiceProvider;

// Resolve and set the tenant.
var resolver = scopedProvider.GetRequiredService<ITenantResolver<TenantInfo>>();
var result = await resolver.ResolveAsync("tenant-a");

var tenantContext = scopedProvider.GetRequiredService<ITenantContext<TenantInfo>>();
if (result.TenantInfo is not null)
    tenantContext.TenantInfo = result.TenantInfo;

// All services resolved from scopedProvider now see the tenant.
var options = scopedProvider.GetRequiredService<IOptions<MyOptions>>();
Console.WriteLine($"Tenant: {tenantContext.TenantInfo?.Name}");
```

### Handling Unresolved Tenants

If no strategy produces an identifier or no store finds a match, `result.TenantInfo` will be `null`.
Check `result.IsResolved` before setting the scoped context:

```csharp
var result = await resolver.ResolveAsync(message);
var tenantContext = services.GetRequiredService<ITenantContext<TenantInfo>>();

if (result.IsResolved)
{
    tenantContext.TenantInfo = result.TenantInfo;
}
else
{
    // Handle unresolved: log, skip, use fallback, etc.
    _logger.LogWarning("No tenant resolved for message {MessageId}", message.Id);
    continue;
}
```

## Using TenantResolver Events

The same resolution events available in ASP.NET Core work in Generic Host apps. Configure them
when registering services:

```csharp
builder.Services.AddMultiTenant<TenantInfo>()
    .WithDelegateStrategy(async ctx => /* ... */)
    .WithConfigurationStore()
    .WithPerTenantOptions(); // enables options

// Hook into resolution events.
builder.Services.Configure<MultiTenantOptions<TenantInfo>>(options =>
{
    options.Events.OnTenantResolveCompleted = context =>
    {
        _logger.LogInformation(
            "Tenant resolved: {TenantId} via strategy {Strategy}",
            context.TenantContext.TenantInfo?.Id,
            context.Strategy.GetType().Name);
        return Task.CompletedTask;
    };
});
```

## Per-Tenant Options

[Per-tenant options](Options) work in Generic Host apps just as they do in ASP.NET Core. The only requirement
is that `ITenantContext.TenantInfo` is populated within the current scope. Once set, `IOptions<T>`,
`IOptionsSnapshot<T>`, and `IOptionsMonitor<T>` all return values for the current tenant.

```csharp
// Register per-tenant options.
builder.Services.ConfigurePerTenant<MyOptions, TenantInfo>((options, tenantInfo) =>
{
    options.Setting = tenantInfo.SomeProperty;
});

// In your handler, resolved within a tenant scope:
public class MessageHandler : IMessageHandler
{
    public MessageHandler(IOptionsSnapshot<MyOptions> options)
    {
        // options.Value reflects the current tenant's settings.
    }
}
```

## EF Core Data Isolation

[EF Core data isolation](EFCore) works identically in Generic Host apps. After `ITenantContext` is populated:

- `AddMultiTenantDbContext<T>()` and `AddPooledMultiTenantDbContext<T>()` automatically bind `TenantInfo`.
- Global query filters enforce per-tenant data separation.
- `IMultiTenantDbContext.TenantInfo` reflects the current tenant.

```csharp
builder.Services.AddMultiTenantDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));
```

> When using `AddPooledMultiTenantDbContext`, only the scoped tenant context is rebound. Pooled context
> instances are reused, so `OnConfiguring` is called only once. Use `AddMultiTenantDbContext` if connection
> strings or providers vary per tenant.

## Important Considerations

- **`TenantInfo` can only be set once** per scope. If your worker might encounter multiple circumstances where
  a tenant could be set, use `ITenantContext.IsResolved` to check first, or use a fresh scope each time.
- **`ITenantResolver.ResolveAsync()` returns a new `TenantContext`** — it does not automatically populate the
  scoped one. You must copy `TenantInfo` yourself.
- **`IOptionsMonitor<T>` is scoped** in MultiTenant (unlike standard .NET where it is a singleton). Despite
  the scoped registration, it preserves the same change-notification and cache-invalidation behavior as the
  standard implementation. See [Per-Tenant Options](Options) for details.
- **No authentication integration.** Per-tenant authentication and `WithPerTenantAuthentication()` require
  `Finbuckle.MultiTenant.AspNetCore` and ASP.NET Core's authentication middleware.

## See Also

- [Configuration and Usage](ConfigurationAndUsage) — registration and resolver details
- [Core Concepts](CoreConcepts) — `ITenantContext`, `TenantContext`, and scoped lifetime
- [MultiTenant Strategies](Strategies) — all built-in strategies
- [Per-Tenant Options](Options) — options customization
- [EF Core Data Isolation](EFCore) — shared and separate database patterns