# Data Isolation with Entity Framework Core

## Introduction
Data isolation is one of the most important considerations in a multitenant app. Whether each tenant has its own database, a shared database, or a hybrid approach can make a significant different in app design. Finbuckle.MultiTenant supports each of these models by associating a connection string with each tenant.

## Separate Databases
If each tenant uses a separate database then the `ConnectionString` tenant info 
property can be used directly in the `OnConfiguring` method of the database
context class to configure the connection. The `TenantInfo` instance can be
injected into the database context using either an `ITenantInfo` or custom
`ITenantInfo` implementation (as configured with `AddMultiTenant<T>`) parameter
on the database context constructor.

```cs
public class MyAppDbContext : DbContext
{
   private TTenantInfo TenantInfo { get; set; }

   public MyAppDbContext(MyTenantInfo tenantInfo)
   {
       // DI will pass in the tenant info for the current request.
       // ITenantInfo is also injectable.
       TenantInfo = tenantInfo;
   } 

   protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
   {
       // Use the connection string to connect to the per-tenant database.
       optionsBuilder.UseSqlServer(TenantInfo.ConnectionString);
   }
   ...
}
```

This approach does not require the added complexity described below for a shared
database approach, but does come with its own complexity in operating and
maintaining a larger number of database instances and infrastructure.

## Shared Database
In shared database scenarios it is important to make sure that queries and commands for a tenant do not affect the data belonging to other tenants. Finbuckle.MultiTenant handles this automatically and removes the need to sprinkle "where" clauses all over an app. Internally a "shadow" tenant ID property is added (or used if already present) to multitenant entity types and managed as the database context is used. It also performs validation and related options for handling null or mismatched tenants.

Finbuckle.MultiTenant provides two different ways to utilize this behavior in a database context class:
1. Implement `IMultiTenantDbContext` and used the helper methods as
[described below](#adding-multitenant-functionality-to-an-existing-dbcontext), or
2. Derive from `MultiTenantDbContext` which handles the details for you.

The first option is more complex, but provides enhanced flexibility and allows existing database context classes (which may derive from a base class) to utilize per-tenant data isolation. The second option option is easier, but provides less flexibility. These approaches are both explained further below.

Regardless of how the db context is configured, the context will need to know which entity types should be treated as multitenant
(i.e. which entity types are to be isolated per tenant) When the db context is initialized, a shadow property named `TenantId` is added to the data model for designated entity types. This property is used internally to filter all requests and commands. If there already is a defined string property named "TenantId" then it will be used.

There are two ways to designate an entity type as multitenant: 

1. the `[MultiTenant]` data attribute
2. the fluent API entity type builder extension method `IsMultiTenant`. 

Entity types not designated via one of these methods are not isolated per-tenant
all instances are shared across all tenants.

## Using the [MultiTenant] attribute
The `[MultiTenant]` attribute designates a class to be isolated per-tenant when
it is used as an entity type in a database context:

```cs
// Tenants will only see their own blog posts.
[MultiTenant]
public class BlogPost
{
    ...
}

// Roles will be the same for all tenants.
public class Roles
{
    ...
}

public class BloggingDbContext : MultiTenantDbContext
{
    public DbSet<BlogPost> BlogPosts { get; set; } // This will be multitenant!
    public DbSet<Roles> Roles { get; set; } // Not multitenant!
}

```

Database context classes derived from `MultiTenantDbContext` will automatically
respect the `[MultiTenant]` attribute. Otherwise a database context class can
be configured to respect the attribute by calling `ConfigureMultiTenant` in the
`OnModelCreating` method.

```cs
protected override void OnModelCreating(ModelBuilder builder)
{
    // Not needed if db context derives from MultiTenantDbContext
    builder.ConfigureMultiTenant();
}
```

## Using the fluent API
The fluent API entity type builder extension method `IsMultiTenant` can be called in `OnModelCreating` to provide the
multitenant functionality for entity types:

```cs
protected override void OnModelCreating(ModelBuilder builder)
{
    // Configure an entity type to be multitenant.
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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfiguration(new BlogEntityTypeConfiguration());
        // or builder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);

        base.OnModelCreating(builder);
    }
}
```

This approach is more flexible than using the `[MultiTenant]` attribute because
it can be used for types which do not have the attribute, e.g. from another assembly.

## Existing Query Filters
`IsMultiTenant` and the `[MultiTenant]` attribute use a query filter for data isolation and will automatically merge its query filter with an existing query filter is one is present. For that reason, if the type to be multitenant has a existing query filter, `IsMultiTenant` and `ConfigureMultiTenant` should be called *after* the existing query filter is configured: 

```cs
protected override void OnModelCreating(ModelBuilder builder)
{
    // set a global query filter, e.g. to support soft delete
    builder.Entity<MyEntityType>().HasQueryFilter(p => !p.IsDeleted);

    // Configure an entity type to be multitenant (will merge with existing call to HasQueryFilter)
    builder.Entity<MyEntityType>().IsMultiTenant();
}

```

## Adding MultiTenant functionality to an existing DbContext
This approach is more flexible than deriving from `MultiTenantDbContext`, but 
needs more configuration. It requires implementing `IMultiTenantDbContext` and
following a strict convention of helper method calls.

Start by adding the `Finbuckle.MultiTenant.EntityFrameworkCore` package to the project:
```{.bash}
dotnet add package Finbuckle.MultiTenant.EntityFrameworkCore
```

Next, implement `IMultiTenantDbContext` on the context. These interface properties ensure that the extension methods will have the information needed to provide proper data isolation.

```cs
public class MyDbContext : DbContext, IMultiTenantDbContext
{
    ...
    public ITenantInfo TenantInfo { get; }
    public TenantMismatchMode TenantMismatchMode { get; }
    public TenantNotSetMode TenantNotSetMode { get; }
    ...
}
```
The db context will need to ensure that these properties haves values, e.g. through constructors, setters, or default values.

Finally, call the library extension methods as described below. This requires overriding the `OnModelCreating`, `SaveChanges`, and `SaveChangesAsync` methods.

In `OnModelCreating` use the `EntityTypeBuilder` fluent API extension method `IsMultiTenant` to designate entity types as multitenant. Call `ConfigureMultiTenant` on the `ModelBuilder` to configure each entity type marked with the `[MultiTenant]` data attribute. This is only needed if using the attribute and internally uses the `IsMultiTenant` fluent API. Make sure to call the base class `OnModelCreating` method if necessary, such as if inheriting from `IdentityDbContext`.

```cs
protected override void OnModelCreating(ModelBuilder builder)
{
    // If necessary call the base class method.
    // Recommended to be called first.
    base.OnModelCreating(builder);

    // Configure all entity types marked with the [MultiTenant] data attribute
    builder.ConfigureMultiTenant();

    // Configure an entity type to be multitenant.
    builder.Entity<MyEntityType>().IsMultiTenant();
}
```

In `SaveChanges` and `SaveChangesAsync` call the `IMultiTenantDbContext` extension method `EnforceMultiTenant` before calling the base class method. This ensures proper data isolation and behavior for `TenantMismatchMode` and `TenantNotSetMode`.

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

Now, whenever this db context is used it will only set and query records
for the current tenant.

## Deriving from MultiTenantDbContext
This approach is easier bit requires inheriting from `MultiTenantDbContext` which
may not alway be possible. It is simply a pre-configured implementation of
`IMultiTenantDbContext` with the helper methods as described below in
(Adding MultiTenant Functionality to an Existing DbContext)
[#adding-multitenant-functionality-to-an-existing-dbcontext]

Start by adding the `Finbuckle.MultiTenant.EntityFrameworkCore` package to the project:
```{.bash}
dotnet add package Finbuckle.MultiTenant.EntityFrameworkCore
```

The `MultiTenantDbContext` has two constructors which should be called from any derived db context. Make sure to forward the `ITenantInfo` and, if applicable the `DbContextOptions<T>` into the base constructor.

```cs
public class BloggingDbContext : MultiTenantDbContext
{
    public BloggingDbContext(ITenantInfo tenantInfo) : base(tenantInfo) { }

    public BloggingDbContext(ITenantInfo tenantInfo, DbContextOptions<BloggingDbContext> options) :
        base(tenantInfo, options) { }

    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Post> Posts { get; set; }
}
```

If relying on the `ConnectionString` property of the `TenantInfo` then the db context will need to configures itself in its `OnConfiguring` method using its inherited `ConnectionString` property:

```cs
public class BloggingDbContext : MultiTenantDbContext
{
   ...
   protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
   {
       optionsBuilder.UseSqlServer(TenantInfo.ConnectionString);\
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

And that's it. Whenever this db context is used it will only set and query records
for the current tenant.

## Hybrid Per-tenant and Shared Databases
When using a shared database database context based on `IMultiTenantDbContext` it is
simple extend into a hybrid approach simply by assigning some tenants to a separate
shared database (or its own completely isolated database) via the tenant info
connection string property.

## Design Time Instantiation
Given that a multitenant db context usually requires a tenant to function, design time instantiation can be challenging.
By default for things like migrations and command line tools Entity Framework core attempts to create an instance of the context
using dependency injection, however usually no valid tenant exists in these cases and DI fails.
For this reason it is recommended to use a [design time factory](https://docs.microsoft.com/en-us/ef/core/miscellaneous/cli/dbcontext-creation#from-a-design-time-factory) wherein a dummy `ITenantInfo` is constructed  with the desired connection string and passed to the db context constructor.

## Registering with ASP.NET Core

When registering the db context as a service in ASP.NET Core it is important to take into account whether the connection string and/or provider will vary per-tenant. If so, it is recommended to set the connection string and provider in the `OnConfiguring` db context method as described above rather than in the `AddDbContext` service registration method.

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
