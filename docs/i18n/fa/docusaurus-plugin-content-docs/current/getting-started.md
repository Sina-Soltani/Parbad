---
sidebar_position: 2
title: شروع کار
source_hash: 166d127d7f2d
---

# شروع کار

این صفحه یک برنامه ASP.NET Core را از نصب پکیج‌ها تا اولین پرداخت تأییدشده همراهی می‌کند.

## ۱. نصب پکیج‌ها

```bash
dotnet add package Parbad
dotnet add package Parbad.AspNetCore
dotnet add package Parbad.Storage.EntityFrameworkCore
```

اگر درگاه شما در پکیج اصلی نیست، پکیج آن درگاه را هم نصب کنید؛ فهرست را در [درگاه‌ها](./intro#gateways) ببینید.

## ۲. پیکربندی پرباد

```csharp
builder.Services.AddParbad()
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

           gateways
               .AddParbadVirtual()
               .WithOptions(options => options.GatewayPath = "/MyVirtualGateway");
       })
       .ConfigureHttpContext(httpContext => httpContext.UseDefaultAspNetCore())
       .ConfigureStorage(storage => storage.UseEfCore<ShopDbContext>());
```

`UseEfCore<ShopDbContext>()` جدول‌های پرباد را در پایگاه داده خود شما نگه می‌دارد؛ تنظیم `DbContext` را در صفحه
[ذخیره‌سازی](./storage) ببینید.

## ۳. درخواست پرداخت

`IOnlinePayment` را تزریق کنید، پرداخت را درخواست کنید و مشتری را به بانک بفرستید:

```csharp
using Parbad.AspNetCore;

public async Task<IActionResult> Pay()
{
    var result = await _onlinePayment.RequestAsync(invoice =>
    {
        invoice
            .SetAmount(250_000)
            .SetCallbackUrl(Url.Action("Verify", "Payment", null, Request.Scheme)!)
            .UseAutoIncrementTrackingNumber()
            .UseMellat();
    });

    if (!result.IsSucceed)
    {
        return View("PaymentFailed", result.Message);
    }

    // Store result.TrackingNumber with the order.
    return result.GatewayTransporter.TransportToGateway();
}
```

:::note
پرباد هیچ‌وقت مبلغ را تبدیل نمی‌کند. مبلغ را با واحدی (ریال یا تومان) وارد کنید که درگاه انتظار دارد.
:::

## ۴. تأیید پرداخت

بانک مشتری را با GET یا POST به آدرس بازگشت برمی‌گرداند:

```csharp
[HttpGet, HttpPost]
public async Task<IActionResult> Verify()
{
    var invoice = await _onlinePayment.FetchAsync();

    if (invoice.Status != PaymentFetchResultStatus.ReadyForVerifying)
    {
        return View("PaymentFailed", invoice.Message);
    }

    if (!IsOrderStillValid(invoice.TrackingNumber))
    {
        await _onlinePayment.CancelAsync(invoice, cancellationReason: "The product is sold out.");

        return View("PaymentFailed", "The product is sold out.");
    }

    var verifyResult = await _onlinePayment.VerifyAsync(invoice);

    if (!verifyResult.IsSucceed)
    {
        return View("PaymentFailed", verifyResult.Message);
    }

    // The payment is complete: deliver the order.
    return View("PaymentSucceeded", verifyResult.TransactionCode);
}
```

سفارش را **پیش از** `VerifyAsync` بررسی کنید: پس از تأیید پرداخت، پول برداشت شده است و بازگرداندن آن به استرداد
(Refund) نیاز دارد.
