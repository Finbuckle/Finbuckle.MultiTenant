# Data Isolation with Entity Framework Core

## Introduction

Data isolation is one of the most important considerations in a multi-tenant app. Whether each tenant has its own
database, a shared database, or a hybrid approach can make a significant different in app design. MultiTenant
supports each of these models by associating a connection string with each tenant.

## Separate Databases

If each tenant uses a separate database then add a `ConnectionString` property to the app's `TenantInfo`
implementation. and use it in the `OnConfiguring` method of the database context class. The tenant info can be obtained
by injecting a `IMultiTenantContextAccessor<TTenantInfo>` into the database context class constructor.

```csharp
public class AppTenantInfo : ITenantInfo
{
    public required string Id { get; init; }
    public required string Identifier { get; init; }
    public string? Name { get; init; }
    public string? ConnectionString { get; init; }
}

public class MyAppDbContext : DbContext
{
   private AppTenantInfo? TenantInfo { get; set; }

   public MyAppDbContext(IMultiTenantContextAccessor<AppTenantInfo> multiTenantContextAccessor)
   {
       // get the current tenant info at the time of construction
       TenantInfo = multiTenantContextAccessor.MultiTenantContext.TenantInfo;
   } 

   protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
   {
       // use the connection string to connect to the per-tenant database
       optionsBuilder.UseSqlServer(TenantInfo?.ConnectionString);
   }
   ...
}
```

This approach does not require the added complexity described below for a shared database approach, but does come with
its own complexity in operating and maintaining a larger number of database instances and infrastructure.

## Shared Database

In shared database scenarios it is important to make sure that queries and commands for a tenant do not affect the data
belonging to other tenants. MultiTenant handles this automatically and removes the need to sprinkle "where"
clauses all over your app. Internally a shadow `TenantId` property is added (or an existing one is used if already present)
to multi-tenant entity types and managed as the database context is used. It also performs validation and related options for handling
null or mismatched tenants.

MultiTenant provides two different ways to utilize this behavior in a database context class:

1. Implement `IMultiTenantDbContext` and use the provided helper methods as described in
   [Adding MultiTenant Functionality to an Existing DbContext](#adding-multitenant-functionality-to-an-existing-dbcontext),
   or
2. Derive from `MultiTenantDbContext` which handles most of the details for
   you as described in [Deriving from MultiTenantDbContext](#deriving-from-multitenantdbcontext).

The first option is more complex, but provides enhanced flexibility and allows existing database context classes (which
may derive from a base class) to utilize per-tenant data isolation. The second option is easier, but provides less
flexibility. These approaches are both explained in detail further below.

## Hybrid Per-tenant and Shared Databases

When using a shared database context based on `IMultiTenantDbContext` it is simple extend into a hybrid approach simply
by assigning some tenants to a separate shared database (or its own completely isolated database) via a tenant info
connection string property as described above in [separate databases](#separate-databases).

## Configuring and Using a Shared Database

Whether implementing `IMultiTenantDbContext` directly or deriving from `MultiTenantDbContext`, the context will need to
know which entity types should be treated as multi-tenant (i.e. which entity types are to be isolated per tenant) When
the database context is initialized, a shadow property named `TenantId` is added to the data model for designated entity
types. This property is used internally to filter all requests and commands. If there already is a defined string
property named `TenantId` then it will be used.

There are two ways to designate an entity type as multi-tenant:

1. apply the `[MultiTenant]` data attribute
2. use the fluent API entity type builder extension method `IsMultiTenant`

Entity types not designated via one of these methods are not isolated per-tenant; all instances are shared across all
tenants. You can also explicitly mark an entity type as non-multi-tenant using the `IsNotMultiTenant()` fluent API
method, which is useful for overriding the `[MultiTenant]` attribute or for clearly documenting shared entities.

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
the entity type via `AdjustKey`,`AdjustIndex`, `AdjustIndexes`, and `AdjustUniqueIndexes`. See [Keys and Indexes](#keys-and-indexes) for
more details.

### Excluding Entities from Multi-Tenancy

In some scenarios, you may need to explicitly mark an entity as non-multi-tenant, even in a multi-tenant database context.
The fluent API extension method `IsNotMultiTenant` can be used to exclude specific entity types from tenant isolation:

```csharp
protected override void OnModelCreating(ModelBuilder builder)
{
    // Configure an entity type to be explicitly non-multi-tenant.
    builder.Entity<SharedConfiguration>().IsNotMultiTenant();
    builder.Entity<GlobalSetting>().IsNotMultiTenant();
}
```

This is useful when you have entities that should be shared across all tenants in a shared database scenario. For example:
- System-wide configuration settings
- Shared reference data (e.g., countries, states, currencies)
- Global audit logs
- Cross-tenant reports or analytics

Entities marked with `IsNotMultiTenant()`:
- Will not have a `TenantId` property added (shadow or otherwise)
- Will not be filtered by tenant in queries
- Will be accessible to all tenants
- If previously configured with `IsMultiTenant()`, the tenant query filter will be removed

This method is particularly useful when:
1. You have a mix of tenant-specific and shared entities in the same database context
2. You need to override the `[MultiTenant]` attribute on an entity type from another assembly
3. You want to explicitly document which entities are intentionally shared across tenants

## Existing Query Filters

`IsMultiTenant` and the `[MultiTenant]` attribute use a named global query filter for data isolation and will not 
impact any other named global query filters applied to the entity. See
[using multiple query filters](https://learn.microsoft.com/en-us/ef/core/querying/filters#using-multiple-query-filters)
in the EF Core documentation for more details.

> In earlier version of MultiTenant an anonymous global filter query was used which required 
> special consideration for combining with existing query filters. Since EF Core introduced named global query 
> filters in .NET 10 these considerations are no longer relevant.

## Adding MultiTenant functionality to an existing DbContext

This approach is more flexible than deriving from `MultiTenantDbContext`, but needs more configuration. It requires
implementing `IMultiTenantDbContext` and following a strict convention of helper method calls.

Start by adding the `MultiTenant.EntityFrameworkCore` package to the project:

```{.bash}
dotnet add package Finbuckle.MultiTenant.EntityFrameworkCore
```

Next, implement `IMultiTenantDbContext` on the context. These interface properties ensure that the extension methods
will have the information needed to provide proper data isolation.

```csharp
public class MyDbContext : DbContext, IMultiTenantDbContext
{
    ...
    public ITenantInfo? TenantInfo { get; }
    public TenantMismatchMode TenantMismatchMode { get; }
    public TenantNotSetMode TenantNotSetMode { get; }
    ...
}
```

The database context will need to ensure that these properties haves values, either through constructors, setters, or
default values.

Finally, call the library extension methods as described below. This requires overriding the `OnModelCreating`,
`SaveChanges`, and `SaveChangesAsync` methods.

In `OnModelCreating` use the `EntityTypeBuilder` fluent API extension method `IsMultiTenant` to designate entity types
as multi-tenant. Call `ConfigureMultiTenant` on the `ModelBuilder` to configure each entity type marked with the
`[MultiTenant]` data attribute. This is only needed if using the attribute and internally uses the `IsMultiTenant`
fluent API. Make sure to call the base class `OnModelCreating` method if necessary, such as if inheriting from
`IdentityDbContext`.

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
calling the base class method. This ensures proper data isolation and behavior.

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

Now whenever this database context is used, it will only set and query records for the current tenant.

## Deriving from `MultiTenantDbContext`

This approach is easier but requires inheriting from `MultiTenantDbContext` which may not always be possible if you
already have a base class. `MultiTenantDbContext` a pre-configured implementation of `IMultiTenantDbContext` with the
helper methods as described above in
[Adding MultiTenant Functionality to an Existing DbContext](#adding-multitenant-functionality-to-an-existing-dbcontext)

Start by adding the `MultiTenant.EntityFrameworkCore` package to the project:

```{.bash}
dotnet add package Finbuckle.MultiTenant.EntityFrameworkCore
```

The `MultiTenantDbContext` has two constructors which should be called from any derived database context. Make sure to
forward the `IMultiTenatContextAccessor` and, if applicable the `DbContextOptions<T>` into the base constructor.

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
Now whenever this database context is used it will only set and query records for the current tenant.

## Binding the Tenant to the DbContext

It is recommended that the tenant associated with an instance of your DbContext is set at the time of creation and is 
immutable. MultiTenant is designed with this in mind and `IMultiTenantDbContext` only has a getter for 
the `TenantInfo` property. It is possible to define a setter on your own `IMultiTenantDbContext` implementation but 
doing so will make it difficult ensure data isolation and consistency.

## Dependency Injection Instantiation

For many cases, such as typical ASP.NET Core apps, normal dependency injection registration of a database context is
sufficient. The `AddDbContext` will register the context as a service and provide the necessary dependencies. Injected
instances will automatically be associated with the current tenant.

When registering the database context as a service for use with dependency injection it is important to take into
account whether the connection string and/or provider will vary per-tenant. If so, it is recommended to set the
connection string and provider in the `OnConfiguring` database context method as described above rather than in the
`AddDbContext` service registration method.

## Factory Instantiation

In some cases it may be necessary to create a database context instance without dependency injection, such as in code
that loops through tenants. In this case, the `MultiTenantDbContext.Create` factory method can be used to create a
database context instance for a specific tenant.

```csharp
// create or otherwise obtain a tenant info instance
var tenantInfo = new MyTenantInfo { Id = "id", Identifier = "identifier" };

// create a database context instance for the tenant
var tenantDbContext = MultiTenantDbContext.Create<AppMultiTenantDbContext, AppTenantInfo>(tenantInfo);

// create a database context instance for the tenant with an instance of DbOptions<AppMultiTenantDbContext>
var tenantDbContextWithOptions = MultiTenantDbContext.Create<AppMultiTenantDbContext, AppTenantInfo>(tenantInfo, 
dbOptions);

// create a database context instance for the tenant with an instance of other dependencies
// the final parameter is params object[] so any number of dependency arguments can be used
var tenantDbContextWithOptions = MultiTenantDbContext.Create<AppMultiTenantDbContext, AppTenantInfo>(tenantInfo, 
dep1, dep2, dep3);

// create a database context instance for the tenant with an instance from pulled from a given service provider
var tenantDbContextWithOptions = MultiTenantDbContext.Create<AppMultiTenantDbContext, AppTenantInfo>(tenantInfo, 
serviceProvider);

// create a database context instance for the tenant with an instance from pulled from a given service provider
// and provided explicitly via params object[]
var tenantDbContextWithOptions = MultiTenantDbContext.Create<AppMultiTenantDbContext, AppTenantInfo>(tenantInfo, 
serviceProvider, dep1, dep2, dep3);

// loop through a bunch of tenant instances
foreach (var tenant in tenants)
{
    using var tenantDbContext = MultiTenantDbContext.Create<AppMultiTenantDbContext, AppTenantInfo>(tenant);
    // do something with the database context
}
```

Make sure to dispose of the database context instance when it is no longer needed, or better yet use a `using` block or
variable. This method will work for any database context class expecting a `IMultiTenantContextAccessor` in its
constructor and an options DbContextOptions<T> in its constructor.

## Design Time Instantiation

Given that a multi-tenant database context usually requires a tenant to function, design time instantiation can be
challenging. By default, for things like migrations and command line tools Entity Framework core attempts to create an
instance of the context using dependency injection, however usually no valid tenant exists in these cases and DI fails.
For this reason it is recommended to use
a [design time factory](https://docs.microsoft.com/en-us/ef/core/miscellaneous/cli/dbcontext-creation#from-a-design-time-factory)
wherein a dummy `TenantInfo` with the desired connection string and passed to the database context creation factory
as described above.

## Adding Data

Added entities are automatically associated with the current `TenantInfo`. If an entity is associated with a different
`TenantInfo` then a `MultiTenantException` is thrown in `SaveChanges` or `SaveChangesAsync`. This behavior can be
altered by changing the values of [TenantMisMatchMode](#tenant-mismatch-mode) and
[TenantNotSetMode](#tenant-not-set-mode) on the `IMultiTenantDbContext`.

> EF Core will require a non-null value when adding an entity that has `TenantId` as a part of the primary key.
> If the `TenandId` property is not settable (e.g. it is a shadow property), EF Core will require a non-null value.
> MultiTenant will ensure a `TenantId` is assigned if you call the `EnforceMultiTenantOnTracking` extension 
> method of `IMultiTenantDbContext` on your db context. See [EF Core Tracking](#ef-core-tracking) for more details.

```csharp
Blog  myBlog = new Blog{ TenantId = "1", Title = "My Blog" };

// Add the blog to a db context for a tenant.
var myTenantInfo = new TenantInfo { Id = "1", Identifier = "tenant-1" };
var myDbContext = MultiTenantDbContext.Create<BloggingDbContext, TenantInfo>(myTenantInfo);
myDbContext.Blogs.Add(myBlog);
myDbContext.SaveChanges();

// Try to add the same blog to a different tenant.
var yourTenantInfo = new TenantInfo { Id = "2", Identifier = "tenant-2" };
var yourDbContext = MultiTenantDbContext.Create<BloggingDbContext, TenantInfo>(yourTenantInfo);
yourDbContext.Blogs.Add(myBlog);
await yourDbContext.SaveChangesAsync(); // Throws MultiTenantException.
```

## Querying Data

EF Core Queries will only return results associated to the `TenantInfo`.

```csharp
// Will only return "My Blog".
var myTenantInfo = new TenantInfo { Id = "1", Identifier = "tenant-1" };
var myDbContext = MultiTenantDbContext.Create<BloggingDbContext, TenantInfo>(myTenantInfo);
var tenantBlog = myDbContext.Blogs.First();

// Will only return "Your Blog".
var yourTenantInfo = new TenantInfo { Id = "2", Identifier = "tenant-2" };
var yourDbContext = MultiTenantDbContext.Create<BloggingDbContext, TenantInfo>(yourTenantInfo);
var yourBlogs = yourDbContext.Blogs.First(); 
```
> The global query filter is applied only at the root level of a query. Any entity classes loaded via `Include` or
> `ThenInclude` are not filtered, but if all entity classes involved in a query have the `[MultiTenant]` attribute> 
> then all results are associated to the same tenant. See [global query filter limitations](https://learn.microsoft.com/en-us/ef/core/querying/filters#limitations)
> in the EF Core documentation for more details.

## Query Without the Tenant Filter
`IgnoreQueryFilters` can be used to bypass the filter for LINQ queries.
MultiTenant uses the `MultiTenant.Abstractions.Constants.TenantToken` constant as the global 
query filter name. See [disabling filters](https://learn.microsoft.com/en-us/ef/core/querying/filters?tabs=ef10#disabling-filters)
in the EF Core documentation for more details.

```csharp
// TenantBlogs will contain all blogs, regardless of tenant.
var myTenantInfo = ...;
var db = MultiTenantDbContext.Create<BloggingDbContext, TenantInfo>(myTenantInfo);
var tenantBlogs = db.Blogs.IgnoreQueryFilters(Abstractions.Constants.TenantToken).ToList(); 
```

## Updating and Deleting Data

Updated or deleted entities are checked to make sure they are associated with the `TenantInfo`. If an entity is
associated with a different `TenantInfo` then a `MultiTenantException` is thrown in `SaveChanges` or `SaveChangesAsync`.
This behavior can be altered by changing the values of [TenantMisMatchMode](#tenant-mismatch-mode) and 
[TenantNotSetMode](#tenant-not-set-mode) on the `IMultiTenantDbContext`.

```csharp
// Add a blog for a tenant.
Blog  myBlog = new Blog{ TenantId = "1", Title = "My Blog" };
var myTenantInfo = new TenantInfo { Id = "1", Identifier = "tenant-1" };
var myDbContext = MultiTenantDbContext.Create<BloggingDbContext, TenantInfo>(myTenantInfo);
myDbContext.Blogs.Add(myBlog);
myDbContext.SaveChanges();

// Modify and attach the same blog to a different tenant.
var yourTenantInfo = new TenantInfo { Id = "2", Identifier = "tenant-2" };
var yourDbContext = MultiTenantDbContext.Create<BloggingDbContext, TenantInfo>(yourTenantInfo);
yourDbContext.Blogs.Attach(myBlog);
myBlog.Title = "My Changed Blog";
await yourDbContext.SaveChangesAsync(); // Throws MultiTenantException.

// Delete from the original tenant
myDbContext.Blogs.Remove(myBlog);
await myDbContext.SaveChangesAsync();

// Delete from the other tenant
yourDbContext.Blogs.Remove(myBlog);
await yourDbContext.SaveChangesAsync(); // Throws MultiTenantException.
```

## Keys and Indexes

When configuring a multi-tenant entity type it is often useful to include the implicit `TenantId` column in the primary
key and/or indexes. The `MultiTenantEntityTypeBuilder` instance returned from `IsMultiTenant()` provides the following
methods for this purpose:

* `AdjustKey(IMutableKey, ModelBuilder)` - Alters the existing defined key to add the implicit `TenantId`. Note that
  this will also impact entities with a dependent foreign key and may add an implicit `Tenant Id` there as well. 
  This will also require the use of `EnforceMultiTenantOnTracking` as desbrived below in [EFCore Tracking](#efcore-tracking).
```csharp
protected override void OnModelCreating(ModelBuilder builder)
{
    // Configure an entity type to be multi-tenant, adjust the existing keys and indexes
    var key = builder.Entity<Blog>().Metadata.GetKeys().First();
    builder.Entity<MyEntityType>().IsMultiTenant().AdjustKey(key, builder).AdjustIndexes();
}
```
* `AdjustIndex(IMutableIndex)` - Alters an existing index include the implicit `TenantId`.
* `AdjustIndexes()` - Alters all existing indexes to include the implicit `TenantId`.
* `AdjustUniqueIndexes()` - Alters only all existing unique indexes to include te implicit `TenantId`.

## EF Core Tracking

When attaching an entity to tracking in EFCore using either `Add` or `Attach`, all primary keys are required 
to be non-null. MultiTenant will ensure a `TenantId` is assigned if you call the 
`EnforceMultiTenantOnTracking` extension method of `IMultiTenantDbContext` on your db context. If no `TenantId` is 
initially set then the current `TenantId` of the db context will be used. This applies to both explicit `TenantId` 
properties and implicit `TenantId` shadow properties. It is recommended to call `EnforceMultiTenantOnTracking`
in your db context constructor.

## Tenant Mismatch Mode

Normally MultiTenant will automatically coordinate the `TenantId` property of each entity. However, in certain
situations the `TenantId` can be manually set.

By default, attempting to add or update an entity with a different `TenantId` property throws a `MultiTenantException`
during a call to `SaveChanges` or `SaveChangesAsync`. This behavior can be changed by setting the `TenantMismatchMode`
property on the database context:

* `TenantMismatchMode.Throw` - A `MultiTenantException` is thrown (default).
* `TenantMismatchMode.Ignore` - The entity is added or updated without modifying its `TenantId`.
* `TenantMismatchMode.Overwrite` - The entity's `TenantId` is overwritten to match the database context's current
  `TenantInfo`.

## Tenant Not Set Mode

If the `TenantId` on an entity is manually set to null the default behavior is to overwrite the `TenantId` for added
entities or to throw a `MultiTenantException` for updated entities. This occurs during a call to `SaveChanges`
or `SaveChangesAsync`. This behavior can be changed by setting the `TenantNotSetMode` property on the database context:

* `TenantNotSetMode.Throw` - For added entities the null `TenantId` will be overwritten to match the database context's
  current `TenantInfo`. For updated entities a `MultiTenantException` is thrown (default).
* `TenantNotSetMode.Overwrite` - The entity's `TenantId` is overwritten to match the database context's current
  `TenantInfo`.
