# Data Isolation with Entity Framework Core

## Introduction

Data isolation is one of the most important considerations in a multi-tenant app. Whether each tenant has its own
database, a shared database, or a hybrid approach can make a significant different in app design. Finbuckle.MultiTenant
supports each of these models by associating a connection string with each tenant.

## Separate Databases

If each tenant uses a separate database then add a `ConnectionString` property to the app's `ITenantInfo`
implementation. and use it in the `OnConfiguring` method of the database context class. The tenant info can be obtained
by injecting a `IMultiTenantContextAccessor<TTenantInfo>` into the database context class constructor.

```csharp
public class AppTenantInfo : ITenantInfo
{
    public string Id { get; set; }
    public string Identifier { get; set; }
    public string Name { get; set; }
    public string ConnectionString { get; set; }
}

public class MyAppDbContext : DbContext
{
   // AppTenantInfo is the app's custom implementation of ITenantInfo which 
   private AppTenantInfo TenantInfo { get; set; }

   public MyAppDbContext(IMultiTenantContextAccessor<AppTenantInfo> multiTenantContextAccessor)
   {
       // get the current tenant info at the time of construction
       TenantInfo = multiTenantContextAccessor.tenantInfo;
   } 

   protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
   {
       // use the connection string to connect to the per-tenant database
       optionsBuilder.UseSqlServer(TenantInfo.ConnectionString);
   }
   ...
}
```

This approach does not require the added complexity described below for a shared database approach, but does come with
its own complexity in operating and maintaining a larger number of database instances and infrastructure.

## Shared Database

In shared database scenarios it is important to make sure that queries and commands for a tenant do not affect the data
belonging to other tenants. Finbuckle.MultiTenant handles this automatically and removes the need to sprinkle "where"
clauses all over an app. Internally a shadow `TenantId` property is added (or used if already present) to multi-tenant
entity types and managed as the database context is used. It also performs validation and related options for handling
null or mismatched tenants.

Finbuckle.MultiTenant provides two different ways to utilize this behavior in a database context class:

1. Implement `IMultiTenantDbContext` and used the helper methods as
   [described below](#adding-multitenant-functionality-to-an-existing-dbcontext), or
2. Derive from `MultiTenantDbContext` which handles the details for you.

The first option is more complex, but provides enhanced flexibility and allows existing database context classes (which
may derive from a base class) to utilize per-tenant data isolation. The second option is easier, but provides less
flexibility. These approaches are both explained further below.

Regardless of how the database context is configured, the context will need to know which entity types should be treated
as multi-tenant (i.e. which entity types are to be isolated per tenant) When the database context is initialized, a
shadow property named `TenantId` is added to the data model for designated entity types. This property is used
internally to filter all requests and commands. If there already is a defined string property named `TenantId` then it
will be used.

There are two ways to designate an entity type as multi-tenant:

1. apply the `[MultiTenant]` data attribute
2. use the fluent API entity type builder extension method `IsMultiTenant`

Entity types not designated via one of these methods are not isolated per-tenant all instances are shared across all
tenants.

## Using the `[MultiTenant]` attribute

The `[MultiTenant]` attribute designates a class to be isolated per-tenant when it is used as an entity type in a
database context:

```csharp
// tenants will only see their own blog posts
[MultiTenant]
public class BlogPost
{
    ...
}

// roles will be the same for all tenants
public class Roles
{
    ...
}

public class BloggingDbContext : MultiTenantDbContext
{
    public BloggingDbContext(IMultiTenantContextAccessor multiTenantContextAccessor) : base(multiTenantContextAccessor)
    {
    }
    
    public DbSet<BlogPost> BlogPosts { get; set; } // this will be multi-tenant!
    public DbSet<Roles> Roles { get; set; } // not multi-tenant!
}

```

Database context classes derived from `MultiTenantDbContext` will automatically respect the `[MultiTenant]` attribute.
Otherwise, a database context class can be configured to respect the attribute by calling `ConfigureMultiTenant` in the
`OnModelCreating` method.

```csharp
protected override void OnModelCreating(ModelBuilder builder)
{
    // not needed if database context derives from MultiTenantDbContext
    builder.ConfigureMultiTenant();
}
```

## Using the fluent API

The fluent API entity type builder extension method `IsMultiTenant` can be called in `OnModelCreating` to provide the
multi-tenant functionality for entity types:

```csharp
protected override void OnModelCreating(ModelBuilder builder)
{
    // Configure an entity type to be multi-tenant.
    builder.Entity<MyEntityType>().IsMultiTenant();
}
```

This approach is more flexible than using the `[MultiTenant]` attribute because it can be used for types which do not
have the attribute, e.g. from another assembly.

`IsMultiTenant()` returns an `MultiTenantEntityTypeBuilder` instance which enables further multi-tenant configuration of
the entity type via `AdjustKey`,`AdjustIndex`, `AdjustIndexes`, and `AdjustUniqueIndexes`. See [Keys and Indexes] for
more details.

## Existing Query Filters

`IsMultiTenant` and the `[MultiTenant]` attribute use a query filter for data isolation and will automatically merge its
query filter with an existing query filter is one is present. For that reason, if the type to be multi-tenant has an
existing query filter, `IsMultiTenant` and `ConfigureMultiTenant` should be called *after* the existing query filter is
configured:

```csharp
protected override void OnModelCreating(ModelBuilder builder)
{
    // set a global query filter, e.g. to support soft delete
    builder.Entity<MyEntityType>().HasQueryFilter(p => !p.IsDeleted);

    // configure an entity type to be multi-tenant (will merge with existing call to HasQueryFilter)
    builder.Entity<MyEntityType>().IsMultiTenant();
}

```

## Adding MultiTenant functionality to an existing DbContext

This approach is more flexible than deriving from `MultiTenantDbContext`, but needs more configuration. It requires
implementing `IMultiTenantDbContext` and following a strict convention of helper method calls.

Start by adding the `Finbuckle.MultiTenant.EntityFrameworkCore` package to the project:

```{.bash}
dotnet add package Finbuckle.MultiTenant.EntityFrameworkCore
```

Next, implement `IMultiTenantDbContext` on the context. These interface properties ensure that the extension methods
will have the information needed to provide proper data isolation.

```csharp
public class MyDbContext : DbContext, IMultiTenantDbContext
{
    ...
    public ITenantInfo TenantInfo { get; }
    public TenantMismatchMode TenantMismatchMode { get; }
    public TenantNotSetMode TenantNotSetMode { get; }
    ...
}
```

The database context will need to ensure that these properties haves values, either through constructors, setters, or
default values.

> In earlier version of Finbuckle.MultiTenant `ITenantInfo` and the app implementation where available via dependency
> injection, but this was removed in v7.0.0 for consistency. Instead, inject the `IMultiTenantContextAccessor` and use
> it to set the `TenantInfo` property in the database context constructor.

Finally, call the library extension methods as described below. This requires overriding
the `OnModelCreating`, `SaveChanges`, and `SaveChangesAsync` methods.

In `OnModelCreating` use the `EntityTypeBuilder` fluent API extension method `IsMultiTenant` to designate entity types
as multi-tenant. Call `ConfigureMultiTenant` on the `ModelBuilder` to configure each entity type marked with
the `[MultiTenant]` data attribute. This is only needed if using the attribute and internally uses the `IsMultiTenant`
fluent API. Make sure to call the base class `OnModelCreating` method if necessary, such as if inheriting
from `IdentityDbContext`.

```csharp
protected override void OnModelCreating(ModelBuilder builder)
{
    // If necessary call the base class method.
    // Recommended to be called first.
    base.OnModelCreating(builder);

    // Configure all entity types marked with the [MultiTenant] data attribute
    builder.ConfigureMultiTenant();

    // Configure an entity type to be multi-tenant.
    builder.Entity<MyEntityType>().IsMultiTenant();
}
```

In `SaveChanges` and `SaveChangesAsync` call the `IMultiTenantDbContext` extension method `EnforceMultiTenant` before
calling the base class method. This ensures proper data isolation and behavior for `TenantMismatchMode`
and `TenantNotSetMode`.

```csharp
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

Now, whenever this database context is used it will only set and query records for the current tenant.

## Deriving from `MultiTenantDbContext`

This approach is easier bit requires inheriting from `MultiTenantDbContext` which may not always be possible. It is
simply a pre-configured implementation of `IMultiTenantDbContext` with the helper methods as described above in
[Adding MultiTenant Functionality to an Existing DbContext](#adding-multitenant-functionality-to-an-existing-dbcontext)

Start by adding the `Finbuckle.MultiTenant.EntityFrameworkCore` package to the project:

```{.bash}
dotnet add package Finbuckle.MultiTenant.EntityFrameworkCore
```

The `MultiTenantDbContext` has two constructors which should be called from any derived database context. Make sure to
forward the `IMultiTenatContextAccessor` and, if applicable the `DbContextOptions<T>` into the base constructor.
Variants of these constructors that pass `ITenantInfo` to the base constructor are also available, but these will not be
used for dependency injection.

```csharp
public class BloggingDbContext : MultiTenantDbContext
{
    // these constructors are called when dependency injection is used
    public BloggingDbContext(IMultiTenantContextAccessor multiTenantContextAccessor) : base(multiTenantContextAccessor)
    {
    }
    
    public BloggingDbContext(IMultiTenantContextAccessor multiTenantContextAccessor, DbContextOptions<BloggingDbContext> options) :
        base(multiTenantContextAccessor, options)
    {
    }
    
    // these constructors are useful for testing or other use cases where depdenency injection is not used
    public BloggingDbContext(ITenantInfo tenantInfo) : base(tenantInfo) { }

    public BloggingDbContext(ITenantInfo tenantInfo, DbContextOptions<BloggingDbContext> options) :
        base(tenantInfo, options) { }

    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Post> Posts { get; set; }
}
```

If the derived database context overrides `OnModelCreating` is it recommended that the base class `OnModelCreating`
method is called last so that the multi-tenant query filters are not overwritten.

```csharp
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

Now, whenever this database context is used it will only set and query records for the current tenant.

## Hybrid Per-tenant and Shared Databases

When using a shared database context based on `IMultiTenantDbContext` it is simple extend into a hybrid approach simply
by assigning some tenants to a separate shared database (or its own completely isolated database) via the tenant info
connection string property.

## Design Time Instantiation

Given that a multi-tenant database context usually requires a tenant to function, design time instantiation can be
challenging. By default, for things like migrations and command line tools Entity Framework core attempts to create an
instance of the context using dependency injection, however usually no valid tenant exists in these cases and DI fails.
For this reason it is recommended to use a [design time factory](https://docs.microsoft.com/en-us/ef/core/miscellaneous/cli/dbcontext-creation#from-a-design-time-factory) wherein a dummy `ITenantInfo` is
constructed with the desired connection string and passed to the database context constructor.

## Registering with ASP.NET Core

When registering the database context as a service in ASP.NET Core it is important to take into account whether the
connection string and/or provider will vary per-tenant. If so, it is recommended to set the connection string and
provider in the `OnConfiguring` database context method as described above rather than in the `AddDbContext` service
registration method.

## Adding Data

Added entities are automatically associated with the current `TenantInfo`. If an entity is associated with a
different `TenantInfo` then a `MultiTenantException` is thrown in `SaveChanges` or `SaveChangesAsync`.

```csharp
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

```csharp
// Will only return "My Blog".
var db = new BloggingDbContext(myTenantInfo, null);
var tenantBlog = db.Blogs.First();

// Will only return "Your Blog".
db = new BloggingDbContext(yourTenantInfo, null);
var tenantBlogs = db.Blogs.First(); 
```

`IgnoreQueryFilters` can be used to bypass the filter for LINQ queries.

```csharp
// TenantBlogs will contain all blogs, regardless of tenant.
var db = new BloggingDbContext(myTenantInfo, null);
var tenantBlogs = db.Blogs.IgnoreQueryFilters().ToList(); 
```

The query filter is applied only at the root level of a query. Any entity classes loaded via `Include` or `ThenInclude`
are not filtered, but if all entity classes involved in a query have the `[MultiTenant]` attribute then all results are
associated to the same tenant.

## Updating and Deleting Data

Updated or deleted entities are checked to make sure they are associated with the `TenantInfo`. If an entity is
associated with a different `TenantInfo` then a `MultiTenantException` is thrown in `SaveChanges` or `SaveChangesAsync`.

```csharp
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

## Keys and Indexes

When configuring a multi-tenant entity type it is often useful to include the implicit `TenantId` column in the primary
key and/or indexes. The `MultiTenantEntityTypeBuilder` instance returned from `IsMultiTenant()` provides the following
methods for this purpose:

* `AdjustKey(IMutableKey, ModelBuilder)` - Alters the existing defined key to add the implicit `TenantId`. Note that
  this will also impact entities with a dependent foreign key and may add an implicit `Tenant Id` there as well.
* `AdjustIndex(IMutableIndex)` - Alters an existing index include the implicit `TenantId`.
* `AdjustIndexes()` - Alters all existing indexes to include the implicit `TenantId`.
* `AdjustUniqueIndexes()` - Alters only all existing unique indexes to include te implicit `TenantId`.

```csharp
protected override void OnModelCreating(ModelBuilder builder)
{
    // Configure an entity type to be multi-tenant, adjust the existing keys and indexes
    var key = builder.Entity<Blog>().Metadata.GetKeys().First();
    builder.Entity<MyEntityType>().IsMultiTenant().AdjustKey(key, builder).AdjustIndexes();
}
```

## Tenant Mismatch Mode

Normally Finbuckle.MultiTenant will automatically coordinate the `TenantId` property of each entity. However, in certain
situations the `TenantId` can be manually set.

By default, attempting to add or update an entity with a different `TenantId` property throws a `MultiTenantException`
during a call to `SaveChanges` or `SaveChangesAsync`. This behavior can be changed by setting the `TenantMismatchMode`
property on the database context:

* `TenantMismatchMode.Throw` - A `MultiTenantException` is thrown (default).
* `TenantMismatchMode.Ignore` - The entity is added or updated without modifying its `TenantId`.
* `TenantMismatchMode.Overwrite` - The entity's `TenantId` is overwritten to match the database context's
  current `TenantInfo`.

## Tenant Not Set Mode

If the `TenantId` on an entity is manually set to null the default behavior is to overwrite the `TenantId` for added
entities or to throw a `MultiTenantException` for updated entities. This occurs during a call to `SaveChanges`
or `SaveChangesAsync`. This behavior can be changed by setting the `TenantNotSetMode` property on the database context:

* `TenantNotSetMode.Throw` - For added entities the null `TenantId` will be overwritten to match the database context's
  current `TenantInfo`. For updated entities a `MultiTenantException` is thrown (default).
* `TenantNotSetMode.Overwrite` - The entity's `TenantId` is overwritten to match the database context's
  current `TenantInfo`.
