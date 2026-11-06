# Parbad.Storage.EntityFrameworkCore

Stores the payments and transactions of [Parbad](https://www.nuget.org/packages/Parbad/), the online payment library
for .NET, with Entity Framework Core, in any database that EF Core supports (SQL Server, PostgreSQL, MySQL, SQLite,
Oracle, ...).

The package is built against EF Core 8 and works with the EF Core version of your application (8, 9 or 10).

## In your application's database

Add Parbad's entities to your own `DbContext`:

```csharp
public class ShopDbContext : DbContext
{
    public ShopDbContext(DbContextOptions<ShopDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.UseParbad();
    }
}
```

Register the context with `AddDbContextFactory` (or `AddPooledDbContextFactory`) and tell Parbad to use it:

```csharp
using Parbad.Storage.EntityFrameworkCore.Builder;

builder.Services.AddDbContextFactory<ShopDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddParbad()
       .ConfigureStorage(storage => storage.UseEfCore<ShopDbContext>());
```

Parbad creates a short-lived context from the factory for each of its operations. It therefore never saves changes
that your code has pending on its own context. `AddDbContextFactory` also registers `ShopDbContext` itself, so you can
keep injecting it as before.

Parbad's tables are part of your model, so they are created and updated by your own migrations:

```
dotnet ef migrations add AddParbad
dotnet ef database update
```

## In a database of its own

Parbad's own `ParbadDataContext` keeps the tables in a separate database, with separate migrations:

```csharp
builder.Services.AddParbad()
       .ConfigureStorage(storage => storage.UseEfCore(options =>
       {
           options.ConfigureDbContext = db => db.UseSqlServer(connectionString,
                                                              sql => sql.MigrationsAssembly("Shop.Migrations"));
       }));
```

## Tables

The tables are `payment` and `transaction` in the schema `parbad`. To change them:

```csharp
modelBuilder.UseParbad(options =>
{
    options.DefaultSchema = "shop";
    options.PaymentTableOptions.Name = "parbad_payment";
    options.TransactionTableOptions.Name = "parbad_transaction";
});
```

With `ParbadDataContext`, set the same properties on the options of `UseEfCore(options => ...)`.

## Upgrading from Parbad 3.x

Parbad 4 adds three columns to the payment table: `status`, `refunded_amount` and `bank_reference`. Add a migration
after the upgrade. Existing rows need no data migration: their status follows from their existing columns, and their
refunds from their transactions.

## More information

* [GitHub](https://github.com/Sina-Soltani/Parbad)
* [Documentation](https://github.com/Sina-Soltani/Parbad/wiki)
