---
title: زرین‌پال
slug: /gateways/zarinpal
source_hash: 73b81ae4a00a
---

# زرین‌پال

```bash
dotnet add package Parbad.Gateway.ZarinPal
```

درگاه زرین‌پال برای [پرباد](https://www.nuget.org/packages/Parbad/)، کتابخانه پرداخت آنلاین ‎.NET.

## پیکربندی

```csharp
builder.Services.AddParbad()
       .ConfigureGateways(gateways =>
       {
           gateways
               .AddZarinPal()
               .WithAccounts(accounts =>
               {
                   accounts.AddInMemory(account =>
                   {
                       account.MerchantId = "<Your merchant ID>";
                       account.IsSandbox = false; // true: ZarinPal's sandbox

                       // Only for refunds: the access token of your ZarinPal panel.
                       account.AuthorizationToken = "<Your access token>";
                   });
               });
       })
       // ...
```

## درخواست پرداخت

```csharp
var result = await onlinePayment.RequestAsync(invoice =>
{
    invoice
        .SetAmount(amount)
        .SetCallbackUrl(callbackUrl)
        .UseAutoIncrementTrackingNumber()
        .SetZarinPalData("Order 1234", email: "customer@example.com", mobile: "09121234567") // The description is required
        .UseZarinPal();
});
```

## نتیجه تأیید

`verifyResult.GetZarinPalOriginalVerificationResult()` نتیجه تأیید خود زرین‌پال را برمی‌گرداند.

## استرداد وجه

استرداد وجه از GraphQL API زرین‌پال استفاده می‌کند و به `AuthorizationToken` حساب نیاز دارد. زرین‌پال برای استرداد وجه محیط آزمایشی ندارد.

```csharp
var refundResult = await onlinePayment.RefundCompletelyAsync(trackingNumber);
```
