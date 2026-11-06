# Parbad.Storage.Cache

Stores the payments and transactions of [Parbad](https://www.nuget.org/packages/Parbad/), the online payment library
for .NET, in a memory cache or in a distributed cache.

## Memory cache

```csharp
builder.Services.AddParbad()
       .ConfigureStorage(storage => storage.UseMemoryCache());
```

The data lives in the memory of the application, so it is lost when the application restarts, and it is not shared
between servers. Use it for development and tests; in production, use a database, for example with
[Parbad.Storage.EntityFrameworkCore](https://www.nuget.org/packages/Parbad.Storage.EntityFrameworkCore/).

## Distributed cache

Uses the `IDistributedCache` of your application (Redis, SQL Server, ...):

```csharp
builder.Services.AddStackExchangeRedisCache(options => options.Configuration = redisConnectionString);

builder.Services.AddParbad()
       .ConfigureStorage(storage => storage.UseDistributedCache());
```

The data is kept as long as the cache keeps it; set `CacheEntryOptions` so that it does not expire too soon.

## Options

Both storages take options: `CacheKey` (default `parbad.storage.cache`) and `CacheEntryOptions`.

```csharp
storage.UseDistributedCache(options => options.CacheKey = "shop.parbad");
```

## More information

* [GitHub](https://github.com/Sina-Soltani/Parbad)
* [Documentation](https://github.com/Sina-Soltani/Parbad/wiki)
