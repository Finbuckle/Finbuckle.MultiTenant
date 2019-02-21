### Introduction
Data isolation is one of the most important considerations in a multitenant app. Whether each tenant has its own database, a shared database, or a hybrid approach can make a significant different in app design. Finbuckle.MultiTenant supports each of these models by associating a connection string with each tenant. Tenant's using the same connection string will share a database and accordingly those with a unique connection string will have separate databases.

In shared database scenarios it is important to make sure that queries and commands for a tenant do not affect the data belonging to other tenant's. Finbuckle.MultiTenant handles this automatically and removes the need to sprinkle "where" clauses all over an app. Applying the `MultiTenant` data attribute to an entity and using the `MultiTenantDbContext` as a base for class for an app's own database context tells Finbuckle.MultiTenant to ensure isolation of both queries and create/update/delete commands.

Internally Finbuckle.MultiTenant uses the `HasQueryFilter` function to set a filter on `TenantId` for the current tenant for all queries. For create/update/delete commands the framework checks entities during `SaveChanges` or `SaveChangesAsync` to ensure matches. This behavior can be modified as documented below.

### Configuration
Add the `Finbuckle.MultiTenant.EntityFrameworkCore` package to the project:
```{.bash}
dotnet add package Finbuckle.MultiTenant.EntityFrameworkCore
```

Derive the database context from `MultiTenantDbContext`. Make sure to forward the `TenantContext` and `DbContextOptions<T>` into the base constructor:

```
public class BloggingDbContext : MultiTenantDbContext
{
    public BloggingDbContext(TenantContext tenantContext, DbContextOptions<BloggingDbContext> options) :
        base(tenantContext, options) { }

    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Post> Posts { get; set; }
}
```

There is also a base constructor which takes a connection string parameter instead of a `TenantContext`. Use this for [design time context creation](https://docs.microsoft.com/en-us/ef/core/miscellaneous/cli/dbcontext-creation) for use with migrations or other tools. This will effectively behave as if the `TenantContext` is null for any queries or commands.

If using multiple databases and relying on the `ConnectionString` property of the `TenantContext` then the database context will need to configures itself in its `OnConfiguring` method using its inherited `ConnectionString` property. This property returns the connection string for the current `TenantContext`:

```
public class BloggingDbContext : MultiTenantDbContext
{
   ...
   protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
   {
       // ConnectionString will resolve to the ConnectionString property for the current tenant.
       optionsBuilder.UseSqlServer(ConnectionString);
       // optionsBuilder.UseSqlite(ConnectionString);
   }
   ...
}
```

>{.small} If the derived database context overrides OnModelCreating is it critical that the base class OnModelCreating is called.

Finally, add the `[MultiTenant]` data annotation to entity classes which should be isolated per tenant. If an entity is common to all tenants, then do not apply the attribute:

```
[MultiTenant]
public class Blog
{
    ...
}

[MultiTenant]
public class Post
{
    ...
}
```

When the context is initialized, a shadow property named `TenantId` is added to the data model for these classes. This property is used internally to filter all requests and commands. If there already is a defined string property named "TenantId" then Finbuckle.Multitenant will use the existing property.

### Configuring with ASP.NET Core

If using ASP.NET Core [configure Finbuckle.MultiTenant](GettingStarted) as desired.

Next, add the desired services in the `ConfigureServices` method of your `Startup` class. If using dependency injection for the database context make sure to call `AddDbContext<T>`. Dependency injection will also inject the `TenantContext` into the database context constructor:

```
public class Startup
{
    ...
    public void ConfigureServices(IServiceCollection services)
    {
        ...        
        services.AddMultiTenant()
            .WithInMemoryStore(...)
            .WithBasePathStrategy();
        ...
        services.AddDbContext<BloggingDbContext>();
        ...
    }
    ...
}
```

>{.small} Do not use any of the configuration methods that sets a database provider or connection string if using the `AddDbContext` delegate overload&mdash;the delegate will not have access to the current `TenantContext` or its connection string.

### Adding Data
Added entities are automatically associated with the current `TenantContext`. If an entity is associated with a different `TenantContext` then a `MultiTenantException` is thrown in `SaveChanges` or `SaveChangesAsync`:

```
// Add a blog for a tenant.
Blog  myBlog = new Blog{ Title = "My Blog" };;
var db = new BloggingDbContext(myTenantContext, null);
db.Blogs.Add(myBlog));
db.SaveChanges();


// Try to add the same blog to a different tenant.
db = new BloggingDbContext(yourTenantContext, null);
db.Blogs.Add(myBlog);
await db.SaveChangesAsync(); // Throws MultiTenantException.
```

### Querying Data
Queries only return results associated to the `TenantContext`:

```
// Will only return "My Blog".
var db = new BloggingDbContext(myTenantContext, null);
var tenantBlog = db.Blogs.First();

// Will only return "Your Blog".
db = new BloggingDbContext(yourTenantContext, null);
var tenantBlogs = db.Blogs.First(); 
```

`IgnoreQueryFilters` can be used to bypass the filter for LINQ queries:

```
// TenantBlogs will contain all blogs, regardless of tenant.
var db = new BloggingDbContext(myTenantContext, null);
var tenantBlogs = db.Blogs.IgnoreQueryFilters().ToList(); 
```

The query filter is applied only at the root level of a query. Any entity classes loaded via `Include` or `ThenInclude` are not filtered, but if all entity classes involved in a query have the `MultiTenant` data annotation then all results are associated to the same `TenantContext`.

### Updating and Deleting Data
Updated or deleted entities are checked to make sure they are associated with the `TenantContext`. If an entity is associated with a different `TenantContext` then a `MultiTenantException` is thrown in `SaveChanges` or `SaveChangesAsync`:

```
// Add a blog for a tenant.
Blog  myBlog = new Blog{ Title = "My Blog" };
var db = new BloggingDbContext(myTenantContext);
db.Blogs.Add(myBlog));
db.SaveChanges();

// Modify and attach the same blog to a different tenant.
db = new BloggingDbContext(yourTenantContext, null);
db.Blogs.Attach(myBlog);
myBlog.Title = "My Changed Blog";
await db.SaveChangesAsync(); // Throws MultiTenantException.

db.Blogs.Remove(myBlog);
await db.SaveChangesAsync(); // Throws MultiTenantException.
```

### Tenant Mismatch Mode

Normally Finbuckle.MultiTenant will automatically coordinate the `TenantId` property of each entity. However in certain situations the `TenantId` can be manually set.

By default attempting to add or update an entity with a different `TenantId` property throws a `MultiTenantException` during a call to `SaveChanges` or `SaveChangesAsync`. This behavior can be changed by setting the `TenantMismatchMode` property on the database context:

* TenantMismatchMode.Throw - A `MultiTenantException' is thrown (default).
* TenantMismatchMode.Ignore - The entity is added or updated without modifying its `TenantId`.
* TenantMismatchMode.Overwrite - The entity's `TenantId` is overwritten to match the database context's current `TenantContext`.

### Tenant Not Set Mode

If the `TenantId` on an entity is manually set to null the default behavior is to overwrite the `TenantId` for adde entities or to throw a `MultiTenantException` for updated entities. This occurs during a call to `SaveChanges` or `SaveChangesAsync`. This behavior can be changed by setting the `TenantNotSetMode' property on the database context:

* TenantMismatchMode.Throw - For added entities the null `TenantId` will be overwritten to match the database context's current `TenantContext`. For updated entities a `MultiTenantException` is thrown (default).
* TenantMismatchMode.Overwrite - The entity's `TenantId` is overwritten to match the database context's current `TenantContext`.
