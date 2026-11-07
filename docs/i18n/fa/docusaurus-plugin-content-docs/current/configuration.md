---
sidebar_position: 3
title: پیکربندی
source_hash: 7a6a1ba7f59c
---

# پیکربندی

## ثبت پرباد

### با تزریق وابستگی مایکروسافت (ASP.NET Core)

```csharp
builder.Services.AddParbad()
       .ConfigureGateways(gateways => { /* ... */ })
       .ConfigureHttpContext(httpContext => httpContext.UseDefaultAspNetCore())
       .ConfigureStorage(storage => { /* ... */ });
```

سپس `IOnlinePayment` را هر جا که لازم دارید تزریق کنید.

### با یک کانتینر دیگر (مانند Autofac)

```csharp
var parbad = ParbadBuilder.CreateDefaultBuilder()
                          .ConfigureGateways(gateways => { /* ... */ })
                          .ConfigureHttpContext(httpContext => httpContext.UseOwinFromCurrentHttpContext())
                          .ConfigureStorage(storage => { /* ... */ });

containerBuilder.Populate(parbad.Services);
```

### بدون تزریق وابستگی

پرباد را یک بار در شروع برنامه بسازید:

```csharp
ParbadBuilder.CreateDefaultBuilder()
             .ConfigureGateways(gateways => { /* ... */ })
             .ConfigureHttpContext(httpContext => httpContext.UseOwinFromCurrentHttpContext())
             .ConfigureStorage(storage => { /* ... */ })
             .Build();
```

سپس برای هر درخواست وب یک scope بسازید و در پایان درخواست آن را dispose کنید:

```csharp
using (var scope = StaticOnlinePayment.CreateScope())
{
    var result = await scope.OnlinePayment.RequestAsync(invoice => { /* ... */ });
}
```

## حساب‌های درگاه

### یک حساب

```csharp
.ConfigureGateways(gateways =>
{
    gateways
        .AddMellat()
        .WithAccounts(accounts =>
        {
            accounts.AddInMemory(account =>
            {
                account.TerminalId = 123;
                account.UserName = "<Your username>";
                account.UserPassword = "<Your password>";
            });
        });
})
```

### چند حساب برای یک درگاه

به هر حساب یک نام بدهید و برای هر فاکتور با `UseAccount("...")` حساب را انتخاب کنید:

```csharp
gateways
    .AddMellat()
    .WithAccounts(accounts =>
    {
        accounts.AddInMemory(account =>
        {
            account.Name = "Shop";
            // ...
        });

        accounts.AddInMemory(account =>
        {
            account.Name = "Donations";
            // ...
        });
    });
```

### حساب‌ها از پایگاه داده یا یک سرویس

`IGatewayAccountSource<TAccount>` را پیاده‌سازی کنید. پرباد هر بار که پرداختی را پردازش می‌کند حساب‌ها را از این منبع
می‌گیرد؛ بنابراین تغییرات بلافاصله اعمال می‌شوند. منبع می‌تواند سرویس‌های خود شما را تزریق کند:

```csharp
public class MellatAccounts : IGatewayAccountSource<MellatGatewayAccount>
{
    private readonly ISettingsService _settings;

    public MellatAccounts(ISettingsService settings) => _settings = settings;

    public async Task AddAccountsAsync(IGatewayAccountCollection<MellatGatewayAccount> accounts)
    {
        var settings = await _settings.GetMellatSettingsAsync();

        accounts.Add(new MellatGatewayAccount
        {
            TerminalId = settings.TerminalId,
            UserName = settings.UserName,
            UserPassword = settings.UserPassword
        });
    }
}
```

```csharp
gateways
    .AddMellat()
    .WithAccounts(accounts => accounts.Add<MellatAccounts>(ServiceLifetime.Transient));
```

## اتصال به برنامه وب

| برنامه | پکیج | پیکربندی |
|---|---|---|
| ASP.NET Core | `Parbad.AspNetCore` | `.ConfigureHttpContext(builder => builder.UseDefaultAspNetCore())` |
| ASP.NET MVC 5 | `Parbad.Mvc5` | `.ConfigureHttpContext(builder => builder.UseOwinFromCurrentHttpContext())` |
| ASP.NET WebForms | `Parbad.Owin` | `.ConfigureHttpContext(builder => builder.UseOwinFromCurrentHttpContext())` |

## تنظیمات

```csharp
.ConfigureOptions(options =>
{
    // The messages of the results.
    options.Messages.PaymentSucceed = "Thank you for your payment.";

    // Parbad logs with ILogger; this turns it off.
    options.EnableLogging = false;

    // The timeout of every call to a gateway (default: 30 seconds).
    options.GatewayTimeout = TimeSpan.FromSeconds(20);

    // With a Content-Security-Policy nonce: the nonce of the current request, used by the pages that send the
    // customer to the bank and by the Virtual Gateway.
    options.NonceFactory = httpContext => (string)httpContext.Items["csp-nonce"]!;
})
```

* `Messages`: پیام‌های نتیجه‌ها.
* `EnableLogging`: پرباد با `ILogger` لاگ می‌نویسد؛ با `false` لاگ خاموش می‌شود.
* `GatewayTimeout`: حداکثر زمان هر درخواست به درگاه (پیش‌فرض: ۳۰ ثانیه).
* `NonceFactory`: اگر از nonce در Content-Security-Policy استفاده می‌کنید، nonce درخواست جاری را برمی‌گرداند. پرباد آن
  را در صفحه‌هایی که مشتری را به بانک می‌فرستند و در درگاه مجازی به کار می‌برد.

### شماره پیگیری

`UseAutoIncrementTrackingNumber()` و `UseAutoRandomTrackingNumber()` به هر فاکتور یک شماره پیگیری می‌دهند. تنظیمات
آن‌ها:

```csharp
.ConfigureAutoIncrementTrackingNumber(options =>
{
    options.MinimumValue = 1000;
    options.Increment = 1;
})
.ConfigureAutoRandomTrackingNumber(options => options.MinimumValue = 1000)
```
