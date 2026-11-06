---
sidebar_position: 4
title: ذخیره‌سازی
---

# ذخیره‌سازی

پرباد هر پرداخت و تراکنش‌های آن را ذخیره می‌کند و از آن‌ها برای دانستن وضعیت هر پرداخت استفاده می‌کند: تا هر پرداخت
فقط یک بار تأیید شود، تا پرداخت مربوط به بازگشت از بانک بدون هیچ اطلاعاتی از طرف شما پیدا شود، و تا شماره پیگیری
بدهد.

نتیجه‌های `RequestAsync` و `VerifyAsync` همه چیزی را که برنامه شما لازم دارد (شماره پیگیری، مبلغ، کد تراکنش) در اختیار
شما می‌گذارند؛ جدول‌های پرباد برای کار داخلی خود پرباد هستند.

## EF Core، در پایگاه داده برنامه شما

```bash
dotnet add package Parbad.Storage.EntityFrameworkCore
```

موجودیت‌های پرباد را به `DbContext` خودتان اضافه کنید:

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

context را با `AddDbContextFactory` ثبت کنید و به پرباد بگویید از آن استفاده کند:

```csharp
using Parbad.Storage.EntityFrameworkCore.Builder;

builder.Services.AddDbContextFactory<ShopDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddParbad()
       .ConfigureStorage(storage => storage.UseEfCore<ShopDbContext>());
```

:::info چرا factory؟
پرباد برای هر عملیات خود یک context کوتاه‌عمر از factory می‌سازد. بنابراین هیچ‌وقت تغییراتی را که کد شما روی
context خودش ذخیره‌نشده نگه داشته است ذخیره نمی‌کند. `AddDbContextFactory` خود `ShopDbContext` را هم ثبت می‌کند؛
پس می‌توانید مثل قبل آن را تزریق کنید.
:::

جدول‌های پرباد بخشی از مدل شما هستند، پس migrationهای شما آن‌ها را می‌سازند:

```bash
dotnet ef migrations add AddParbad
dotnet ef database update
```

نام جدول‌ها `payment` و `transaction` در schema `parbad` است. برای تغییر آن‌ها:

```csharp
modelBuilder.UseParbad(options =>
{
    options.DefaultSchema = "shop";
    options.PaymentTableOptions.Name = "parbad_payment";
    options.TransactionTableOptions.Name = "parbad_transaction";
});
```

## EF Core، در یک پایگاه داده جداگانه

```csharp
builder.Services.AddParbad()
       .ConfigureStorage(storage => storage.UseEfCore(options =>
       {
           options.ConfigureDbContext = db => db.UseSqlServer(connectionString,
                                                              sql => sql.MigrationsAssembly("Shop.Migrations"));
       }));
```

در این حالت migrationها متعلق به `ParbadDataContext` پرباد هستند:

```bash
dotnet ef migrations add AddParbad --context ParbadDataContext
```

## Memory Cache و Distributed Cache

```bash
dotnet add package Parbad.Storage.Cache
```

```csharp
.ConfigureStorage(storage => storage.UseMemoryCache())

// or, with the IDistributedCache of your application (Redis, SQL Server, ...)
.ConfigureStorage(storage => storage.UseDistributedCache())
```

`UseDistributedCache` از `IDistributedCache` برنامه شما (Redis، SQL Server و ...) استفاده می‌کند.

:::warning
Memory Cache با راه‌اندازی دوباره برنامه اطلاعاتش را از دست می‌دهد و پرداخت‌های در جریان از بین می‌روند. فقط برای
توسعه و تست از آن استفاده کنید.
:::

## ذخیره‌سازی اختصاصی

`IStorage` را از پکیج `Parbad.Storage.Abstractions` پیاده‌سازی و ثبت کنید:

```csharp
.ConfigureStorage(storage => storage.AddStorage<MyStorage>(ServiceLifetime.Transient))
```

`TryCompletePaymentAsync` باید پرداخت را در یک مرحله اتمی تکمیل کند تا دو تأیید هم‌زمان یک پرداخت نتوانند هر دو موفق
شوند. مستندات XML اینترفیس `IStorage` همه اعضای آن را توضیح می‌دهد.
