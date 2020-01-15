# Data Isolation with Entity Framework Core

## Introduction
Data isolation is one of the most important considerations in a multitenant app. Whether each tenant has its own database, a shared database, or a hybrid approach can make a significant different in app design. Finbuckle.MultiTenant supports each of these models by associating a connection string with each tenant. Tenants using the same connection string will share a database and accordingly those with a unique connection string will have separate databases.

In shared database scenarios it is important to make sure that queries and commands for a tenant do not affect the data belonging to other tenants. Finbuckle.MultiTenant handles this automatically and removes the need to sprinkle "where" clauses all over an app. Designating an entity type as multitenant tells Finbuckle.MultiTenant to ensure isolation of both queries and create/update/delete commands.

The easiest way to configure a shared database is to derive a db context from `MultiTenantDbContext` which contains the added functionality needed to accomplish tenant data isolation. In some situations it is desirable to add multitenant functionality to an existing db context in which case deriving from `MultiTenantDbContext` is not possible. See further below for how to accomplish the same functionality in these cases.

## MultiTenant Entity Types

Regardless of how the db context is configured, the library will need to know which entity types should be treated as multitenant. When the db context is initialized, a shadow property named `TenantId` is added to the data model for designated entity types. This property is used internally to filter all requests and commands. If there already is a defined string property named "TenantId" then Finbuckle.Multitenant will use the existing property.

There are two ways to designate an entity type as multitenant: 

1. the `[MultiTenant]` data attribute; and
2. the fluent API entity type builder extension method `IsMultiTenant`. 

Entity types not designated via one of these methods are not isolated per-tenant and thus are visible to all tenants.

### Using the [MultiTenant] attribute
The `[MultiTenant]` attribute is recognized by `MultiTenantDbContext`-derived db contexts and can be configured to do so for other db contexts by calling the `SetupMultiTenant` extension method on `ModelBuilder` in `OnModelCreating` as described further below.

```cs
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

```cs
protected override void OnModelCreating(ModelBuilder builder)
{
    // Not needed if db context derives from MultiTenantDbContext
    builder.SetupMultiTenant();
}
```

### Using the fluent API
Alternatively, the fluent API entity type builder extension method `IsMultiTenant` can be called in `OnModelCreating` to provide the same functionality for db contexts that do not derive from `MultiTenantDbContext`. Note that `SetupMultiTenant` is not needed.

```cs
protected override void OnModelCreating(ModelBuilder builder)
{
    // Configure an entity type to be multitenant.
    builder.Entity<MyEntityType>().IsMultiTenant();
}
```

`IsMultiTenant` uses a query filter for data isolation and will automatically merge its query filter with an existing query filter is one is present. For that reason, if the type to be multitenant has a global query filter, `IsMultiTenant` should be called *after* `HasQueryFilter`. 

```cs
protected override void OnModelCreating(ModelBuilder builder)
{
    // set a global query filter, e.g. to support soft delete
    builder.Entity<MyEntityType>().HasQueryFilter(p => !p.IsDeleted);

    // Configure an entity type to be multitenant (will merge with existing call to HasQueryFilter)
    builder.Entity<MyEntityType>().IsMultiTenant();
}

```

The fluent API can also be used from within `IEntityTypeConfiguration<TEntity>` classes.

```cs
public class BlogEntityTypeConfiguration : IEntityTypeConfiguration<Blog>
{
    public void Configure(EntityTypeBuilder<Blog> builder)
    {
        builder.IsMultiTenant();
    }
}
```
```cs
protected override void OnModelCreating(ModelBuilder builder)
{
    builder.ApplyConfiguration(new BlogEntityTypeConfiguration());
    // or builder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);

    base.OnModelCreating(builder);
}
```

## Deriving from MultiTenantDbContext
See the data isolation sample projects in the [GitHub repository](https://github.com/Finbuckle/Finbuckle.MultiTenant/tree/master/samples) for examples of this approach.

Start by adding the `Finbuckle.MultiTenant` package to the project:
```{.bash}
dotnet add package Finbuckle.MultiTenant
```
Alternatively just add the `Finbuckle.MultiTenant.EntityFrameworkCore` package if not using ASP.NET Core.

The `MultiTenantDbContext` has two constructors which should be called from any derived db context. Make sure to forward the `TenantInfo` and, if applicable the `DbContextOptions<T>` into the base constructor.

```cs
public class BloggingDbContext : MultiTenantDbContext
{
    public BloggingDbContext(TenantInfo tenantInfo) : base(tenantInfo) { }

    public BloggingDbContext(TenantInfo tenantInfo, DbContextOptions<BloggingDbContext> options) :
        base(tenantInfo, options) { }

    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Post> Posts { get; set; }
}
```

If relying on the `ConnectionString` property of the `TenantInfo` then the db context will need to configures itself in its `OnConfiguring` method using its inherited `ConnectionString` property. This property returns the connection string for the current `TenantInfo`.

```cs
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

If the derived db context overrides `OnModelCreating` is it recommended that the base class `OnModelCreating` method is called last so that the multitenant query filters are not overwritten.

```cs
public class BloggingDbContext : MultiTenantDbContext
{
...
protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // set a global query filter, e.g. to support soft delete
        modelBuilder.Entity<Post>().HasQueryFilter(p => !p.IsDeleted);
        
        // call the base library implementation AFTER the above
        base.OnModelCreating(modelBuilder);
    }
...
}
```

## Adding MultiTenant functionality to an existing DbContext
Start by adding the `Finbuckle.MultiTenant` package to the project:
```{.bash}
dotnet add package Finbuckle.MultiTenant
```
Alternatively just add the `Finbuckle.MultiTenant.EntityFrameworkCore` package if not using ASP.NET Core.

Next, implement `IMultiTenantDbContext` on the db context. These interface properties ensure that the extension methods will have the information needed to provide proper data isolation.

```cs
public class MyDbContext : DbContext, IMultiTenantDbContext
{
    ...
    public TenantInfo TenantInfo { get; }
    public TenantMismatchMode TenantMismatchMode { get; };
    public TenantNotSetMode TenantNotSetMode { get; };
    ...
}
```
The db context will need to ensure that these properties haves values, e.g. through constructors, setters, or default values.

Finally, call the library extension methods as decribed below. This requires overriding the `OnModelCreating`, `SaveChanges`, and `SaveChangesAsync` methods.

In `OnModelCreating` use the `EntityTypeBuilder` fluent API extension method `IsMultiTenant` to designate entity types as multitenant. Call `SetupMultiTenant` on the `ModelBuilder` to configure each entity type marked with the `[MultiTenant]` data attribute. This is only needed if using the attribute and internally uses the `IsMultiTenant` fluent API. Make sure to call the base class `OnModelCreating` method if necessary, such as if inheriting from `IdentityDbContext`.

```cs
protected override void OnModelCreating(ModelBuilder builder)
{
    // If necessary call the base class method.
    // Recommendede to be called first.
    base.OnModelCreating(builder);

    // Setup all entity types marked with the [MultiTenant] data attribute
    builder.SetupMultiTenant();

    // Configure an entity type to be multitenant.
    builder.Entity<MyEntityType>().IsMultiTenant();

}
```

In `SaveChanges` and `SaveChangesAsync` call the `IMultiTenantDbContext` extension method `EnforceMultiTenant` **before** calling the base class method. This ensures proper data isolation and behavior for `TenantMismatchMode` and `TenantNotSetMode`.

```cs
public override int SaveChanges(bool acceptAllChangesOnSuccess)
{
    this.EnforceMultiTenant();
    return base.SaveChanges(acceptAllChangesOnSuccess);
}

public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
    CancellationToken cancellationToken = default(CancellationToken))
{
    this.EnforceMultiTenant();
    return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
}
```

## Design Time Instantiation
Given that a multitenant db context usually requires a tenant to function, design time instanciation can be challenging. For db contexts deriving from `MultiTenantDbContext` it is recommended to use a [design time factory](https://docs.microsoft.com/en-us/ef/core/miscellaneous/cli/dbcontext-creation#from-a-design-time-factory) wherein a dummy `TenantInfo` is constructed  with the desired connection string and passed to the db context constructor.

Db contexts not deriving from `MultiTenantDbContext` will need to take similar considerations.

## Registering with ASP.NET Core

When registering the db context as a service in ASP.NET Core it is important to take into account whether the connection string and/or provider will vary per-tenant. If so, it is recommended to set the connection string and provider in the `OnConfiguring` db context method as desribed above rather than in the `AddDbContext` service registration method.

## Adding Data
Added entities are automatically associated with the current `TenantInfo`. If an entity is associated with a different `TenantInfo` then a `MultiTenantException` is thrown in `SaveChanges` or `SaveChangesAsync`.

```cs
// Add a blog for a tenant.
Blog  myBlog = new Blog{ Title = "My Blog" };;
var db = new BloggingDbContext(myTenantInfo, null);
db.Blogs.Add(myBlog));
db.SaveChanges();


// Try to add the same blog to a different tenant.
db = new BloggingDbContext(yourTenantInfo, null);
db.Blogs.Add(myBlog);
await db.SaveChangesAsync(); // Throws MultiTenantException.
```

## Querying Data
Queries only return results associated to the `TenantInfo`.

```cs
// Will only return "My Blog".
var db = new BloggingDbContext(myTenantInfo, null);
var tenantBlog = db.Blogs.First();

// Will only return "Your Blog".
db = new BloggingDbContext(yourTenantInfo, null);
var tenantBlogs = db.Blogs.First(); 
```

`IgnoreQueryFilters` can be used to bypass the filter for LINQ queries.

```cs
// TenantBlogs will contain all blogs, regardless of tenant.
var db = new BloggingDbContext(myTenantInfo, null);
var tenantBlogs = db.Blogs.IgnoreQueryFilters().ToList(); 
```

The query filter is applied only at the root level of a query. Any entity classes loaded via `Include` or `ThenInclude` are not filtered, but if all entity classes involved in a query have the `[MultiTenant]` attribute then all results are associated to the same tenant.

## Updating and Deleting Data
Updated or deleted entities are checked to make sure they are associated with the `TenantInfo`. If an entity is associated with a different `TenantInfo` then a `MultiTenantException` is thrown in `SaveChanges` or `SaveChangesAsync`.

```cs
// Add a blog for a tenant.
Blog  myBlog = new Blog{ Title = "My Blog" };
var db = new BloggingDbContext(myTenantInfo);
db.Blogs.Add(myBlog));
db.SaveChanges();

// Modify and attach the same blog to a different tenant.
db = new BloggingDbContext(yourTenantInfo, null);
db.Blogs.Attach(myBlog);
myBlog.Title = "My Changed Blog";
await db.SaveChangesAsync(); // Throws MultiTenantException.

db.Blogs.Remove(myBlog);
await db.SaveChangesAsync(); // Throws MultiTenantException.
```

## Tenant Mismatch Mode

Normally Finbuckle.MultiTenant will automatically coordinate the `TenantId` property of each entity. However in certain situations the `TenantId` can be manually set.

By default attempting to add or update an entity with a different `TenantId` property throws a `MultiTenantException` during a call to `SaveChanges` or `SaveChangesAsync`. This behavior can be changed by setting the `TenantMismatchMode` property on the database context:

* TenantMismatchMode.Throw - A `MultiTenantException` is thrown (default).
* TenantMismatchMode.Ignore - The entity is added or updated without modifying its `TenantId`.
* TenantMismatchMode.Overwrite - The entity's `TenantId` is overwritten to match the database context's current `TenantInfo`.

## Tenant Not Set Mode

If the `TenantId` on an entity is manually set to null the default behavior is to overwrite the `TenantId` for adde entities or to throw a `MultiTenantException` for updated entities. This occurs during a call to `SaveChanges` or `SaveChangesAsync`. This behavior can be changed by setting the `TenantNotSetMode' property on the database context:

* TenantNotSetMode.Throw - For added entities the null `TenantId` will be overwritten to match the database context's current `TenantInfo`. For updated entities a `MultiTenantException` is thrown (default).
* TenantNotSetMode.Overwrite - The entity's `TenantId` is overwritten to match the database context's current `TenantInfo`.
