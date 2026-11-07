---
sidebar_position: 4
title: Storage
---

# Storage

Parbad stores every payment and its transactions. It uses them to know the state of each payment: to verify a payment
only once, to find the payment of a callback without any data from you, and to give tracking numbers.

The results of `RequestAsync` and `VerifyAsync` give you everything your application needs (tracking number, amount,
transaction code); Parbad's tables are for Parbad's own work.

## EF Core, in your application's database

```bash
dotnet add package Parbad.Storage.EntityFrameworkCore
```

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

Register the context with `AddDbContextFactory` and tell Parbad to use it:

```csharp
using Parbad.Storage.EntityFrameworkCore.Builder;

builder.Services.AddDbContextFactory<ShopDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddParbad()
       .ConfigureStorage(storage => storage.UseEfCore<ShopDbContext>());
```

:::info Why a factory?
Parbad creates a short-lived context from the factory for each of its operations. It therefore never saves changes
that your code has pending on its own context. `AddDbContextFactory` also registers `ShopDbContext` itself, so you can
keep injecting it as before.
:::

Parbad's tables are part of your model, so your migrations create them:

```bash
dotnet ef migrations add AddParbad
dotnet ef database update
```

The tables are `payment` and `transaction` in the schema `parbad`. To change them:

```csharp
modelBuilder.UseParbad(options =>
{
    options.DefaultSchema = "shop";
    options.PaymentTableOptions.Name = "parbad_payment";
    options.TransactionTableOptions.Name = "parbad_transaction";
});
```

## EF Core, in a database of its own

```csharp
builder.Services.AddParbad()
       .ConfigureStorage(storage => storage.UseEfCore(options =>
       {
           options.ConfigureDbContext = db => db.UseSqlServer(connectionString,
                                                              sql => sql.MigrationsAssembly("Shop.Migrations"));
       }));
```

The migrations then belong to Parbad's `ParbadDataContext`:

```bash
dotnet ef migrations add AddParbad --context ParbadDataContext
```

## Memory cache and distributed cache

```bash
dotnet add package Parbad.Storage.Cache
```

```csharp
.ConfigureStorage(storage => storage.UseMemoryCache())

// or, with the IDistributedCache of your application (Redis, SQL Server, ...)
.ConfigureStorage(storage => storage.UseDistributedCache())
```

:::warning
The memory cache loses its data when the application restarts, so payments in progress are lost. Use it for
development and tests only.
:::

## Your own storage

Implement `IStorage` from `Parbad.Storage.Abstractions` and register it:

```csharp
.ConfigureStorage(storage => storage.AddStorage<MyStorage>(ServiceLifetime.Transient))
```

`TryCompletePaymentAsync` must complete a payment in one atomic step, so that two verifications of the same payment
cannot both succeed. The XML documentation of `IStorage` describes every member.
